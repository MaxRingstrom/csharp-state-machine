using System;
using System.Collections.Generic;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine.internals
{
    public class StateMachineModel<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        public event EventHandler<TransitionActivatedEventArgs<TState, TSignal, TPayload>>? TransitionActivated;

        private readonly IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> model;

        public TState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        private TState currentState;

        public StateMachineModel(IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> model)
        {
            this.model = model;
        }

        public void ProcessSignal(SignalInfo<TState, TSignal, TPayload> signalInfo)
        {
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

        private void OnTransitionActivated(TState currentState, TState to, TSignal signal, TPayload? payload)
        {
            TransitionActivated?.Invoke(this, new TransitionActivatedEventArgs<TState, TSignal, TPayload>(currentState, to, signal, payload));
        }

        private void SwitchState(TState currentState, TState to, TSignal signal, TPayload? payload)
        {
            OnTransitionActivated(currentState, to, signal, payload);
            this.currentState = to;
        }

        public void Init(TState state)
        {
            currentState = state;
        }
    }
}
