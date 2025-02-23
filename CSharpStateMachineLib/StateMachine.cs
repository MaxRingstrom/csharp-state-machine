using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MaxRingstrom.CSharpStateMachineLib
{


    public class StateMachine<TState, TSignal, TPayload> : IDisposable where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        public class TransitionActivatedEventArguments : EventArgs
        {
            public TState From { get; }
            public TState To { get; }
            public TSignal Signal { get; }
            public TPayload? Payload { get; }

            public TransitionActivatedEventArguments(TState from, TState to, TSignal signal, TPayload? payload)
            {
                From = from;
                To = to;
                Signal = signal;
                Payload = payload;
            }
        }

        private class SignalInfo
        {
            public TSignal Signal { get; }
            public TPayload Payload { get; }
            private ManualResetEventSlim FinishEvent { get; }
            public void MarkComplete()
            {
                FinishEvent.Set();
            }

            public SignalInfo(TSignal signal, TPayload payload, ManualResetEventSlim finishEvent)
            {
                Signal = signal;
                Payload = payload;
                FinishEvent = finishEvent;
            }
        }

        public event EventHandler<TransitionActivatedEventArguments>? TransitionActivated;

        private IReadOnlyStateMachineModel<TState, TSignal, TPayload> model;
        private TState currentState;

        private CancellationTokenSource? cancellationTokenSource;

        private BlockingCollection<SignalInfo> signalQueue = new BlockingCollection<SignalInfo>(new ConcurrentQueue<SignalInfo>());

        public StateMachine(IReadOnlyStateMachineModel<TState, TSignal, TPayload> model)
        {
            this.model = model;
        }

        public void Init(TState state)
        {
            currentState = state;
            cancellationTokenSource = new CancellationTokenSource();

            new Thread(() =>
            {
                try
                {
                    while (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var signalInfo = signalQueue.Take(cancellationTokenSource.Token);
                        var transitions = model.GetTransitions(currentState, signalInfo.Signal);
                        foreach (var transition in transitions)
                        {
                            if (transition.Guard == null || transition.Guard(signalInfo.Payload))
                            {
                                transition.TransitionFn(signalInfo.Payload);
                                SwitchState(currentState, transition.To, signalInfo.Signal, signalInfo.Payload);
                                signalInfo.MarkComplete();
                                break;
                            }
                        }
                    }

                    Console.WriteLine("State machine signal dispatcher stopping...");
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex.ToString());
                }
            }).Start();
        }

        private void SwitchState(TState currentState, TState to, TSignal signal, TPayload? payload)
        {
            OnTransitionActivated(currentState, to, signal, payload);
        }

        private void OnTransitionActivated(TState currentState, TState to, TSignal signal, TPayload? payload)
        {
            TransitionActivated?.Invoke(this, new TransitionActivatedEventArguments(currentState, to, signal, payload));
        }

        public Task SendAsync(TSignal signal, TPayload payload, int timeout = 5000)
        {
            var finishEvent = new ManualResetEventSlim();
            var task = Task.Run(() =>
            {
                if (!finishEvent.Wait(timeout))
                {
                    throw new TimeoutException();
                }
            });
            signalQueue.Add(new SignalInfo(signal, payload, finishEvent));
            return task;
        }

        public void Dispose()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }
    }
}
