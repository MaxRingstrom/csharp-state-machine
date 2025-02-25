using MaxRingstrom.CSharpStateMachineLib.StateMachine.@internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine
{
    public interface IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        public IEnumerable<TransitionModel<TState, TSignal, TPayload>> GetTransitions(TState fromState, TSignal? signal);
    }
    public class StateMachineConfiguration<TState, TSignal, TPayload> : IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        private readonly string name;
        private HashSet<TState> states = new HashSet<TState>();
        private HashSet<TSignal> signals = new HashSet<TSignal>();
        private Dictionary<TState, List<TransitionModel<TState, TSignal, TPayload>>> transitions = new Dictionary<TState, List<TransitionModel<TState, TSignal, TPayload>>>();

        public StateMachineConfiguration(string name)
        {
            this.name = name;
        }

        public IEnumerable<TransitionModel<TState, TSignal, TPayload>> GetTransitions(TState fromState, TSignal? signal)
        {
            return (transitions.GetValueOrDefault(fromState) ?? Enumerable.Empty<TransitionModel<TState, TSignal, TPayload>>()).Where(t => t.From.Equals(fromState) && signal == null ? t.Signal == null : signal.Equals(t.Signal));
        }

        internal void AddSignal(TSignal signal)
        {
            signals.Add(signal);
        }

        internal void AddState(TState state)
        {
            states.Add(state);
        }

        internal void AddTransition(TState fromState, TState toState, TSignal? signal, Func<TPayload, bool>? guard, Action<TPayload>? transitionFn)
        {
            if (!transitions.TryGetValue(fromState, out var transitionsForFromState))
            {
                transitionsForFromState = new List<TransitionModel<TState, TSignal, TPayload>>();
                transitions.Add(fromState, transitionsForFromState);
            }

            transitionsForFromState.Add(new TransitionModel<TState, TSignal, TPayload>(fromState, toState, signal, guard, transitionFn));
        }
    }
}
