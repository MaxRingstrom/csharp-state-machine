using System;
using System.Threading;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine.internals
{
    public class SignalInfo<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
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
}
