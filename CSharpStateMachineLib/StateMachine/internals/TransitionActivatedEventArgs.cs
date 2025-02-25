using System;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine.internals
{
    public class TransitionActivatedEventArgs<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        public TState From { get; }
        public TState To { get; }
        public TSignal? Signal { get; }
        public TPayload? Payload { get; }

        public TransitionActivatedEventArgs(TState from, TState to, TSignal? signal, TPayload? payload)
        {
            From = from;
            To = to;
            Signal = signal;
            Payload = payload;
        }
    }
}
