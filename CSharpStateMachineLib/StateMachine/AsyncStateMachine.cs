using MaxRingstrom.CSharpStateMachineLib.StateMachine.internals;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine
{
    public class AsyncStateMachine<TState, TSignal, TPayload> : IDisposable where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        public event EventHandler<TransitionActivatedEventArgs<TState, TSignal, TPayload>>? TransitionActivated;

        private CancellationTokenSource? cancellationTokenSource;
        private BlockingCollection<SignalInfo<TState, TSignal, TPayload>>? signalQueue = new BlockingCollection<SignalInfo<TState, TSignal, TPayload>>(new ConcurrentQueue<SignalInfo<TState, TSignal, TPayload>>());
        private readonly StateMachineEngine<TState, TSignal, TPayload> engine;

        public AsyncStateMachine(IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> configuration)
        {
            engine = new StateMachineEngine<TState, TSignal, TPayload>(configuration);
            engine.TransitionActivated += Context_TransitionActivated;
        }

        private void Context_TransitionActivated(object sender, TransitionActivatedEventArgs<TState, TSignal, TPayload> e)
        {
            TransitionActivated?.Invoke(sender, e);
        }

        public void Init(TState state)
        {
            cancellationTokenSource = new CancellationTokenSource();

            new Thread(() =>
            {
                try
                {
                    engine.Init(state);
                    while (!cancellationTokenSource.Token.IsCancellationRequested && signalQueue != null)
                    {
                        var signalInfo = signalQueue.Take(cancellationTokenSource.Token);
                        engine.ProcessSignal(signalInfo);
                    }

                    Console.WriteLine("State machine signal dispatcher stopping...");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }).Start();
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

            signalQueue?.Add(new SignalInfo<TState, TSignal, TPayload>(signal, payload, finishEvent));
            return task;
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            signalQueue?.Dispose();
            signalQueue = null;
        }
    }
}
