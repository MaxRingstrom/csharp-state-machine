using System;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine.internals
{
    public class StateMachineEngine<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        public event EventHandler<TransitionActivatedEventArgs<TState, TSignal, TPayload>>? TransitionActivated;

        private readonly IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> configuration;

        public TState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        private TState currentState;

        public StateMachineEngine(IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> configuration)
        {
            this.configuration = configuration;
        }

        public void ProcessSignal(SignalInfo<TState, TSignal, TPayload> signalInfo)
        {
            var transitions = configuration.GetTransitions(currentState, signalInfo.Signal);
            foreach (var transition in transitions)
            {
                if (transition.Guard == null || transition.Guard(signalInfo.Payload))
                {
                    transition.TransitionFn?.Invoke(signalInfo.Payload);

                    SwitchState(currentState, transition.To, signalInfo.Signal, signalInfo.Payload);
                    
                    signalInfo.MarkComplete();
                    break;
                }
            }
        }

        private void OnTransitionActivated(TState currentState, TState to, TSignal? signal, TPayload? payload)
        {
            TransitionActivated?.Invoke(this, new TransitionActivatedEventArgs<TState, TSignal, TPayload>(currentState, to, signal, payload));
        }

        private void SwitchState(TState currentState, TState to, TSignal? signal, TPayload? payload)
        {
            OnTransitionActivated(currentState, to, signal, payload);
            this.currentState = to;

            // Process auto signal
            ProcessSignal(new SignalInfo<TState, TSignal, TPayload>(null, null, null));
        }

        public void Init(TState state)
        {
            currentState = state;
            ProcessSignal(new SignalInfo<TState, TSignal, TPayload>(null, null, new System.Threading.ManualResetEventSlim()));
        }
    }
}
