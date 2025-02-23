using System;

namespace MaxRingstrom.CSharpStateMachineLib
{
    public class TransitionModel<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable
    {
        public TState From { get; }
        public TState To { get; }
        public TSignal Signal { get; }
        public Func<TPayload, bool>? Guard { get; }
        public Action<TPayload> TransitionFn { get; }

        internal TransitionModel(TState from, TState to, TSignal signal, Func<TPayload, bool>? guard, Action<TPayload> transitionFn)
        {
            From = from;
            To = to;
            Signal = signal;
            Guard = guard;
            TransitionFn = transitionFn;
        }
    }
}
