using System;

namespace MaxRingstrom.CSharpStateMachineLib
{
    public class StateMachineModelBuilder<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable
    {
        private StateMachineModel<TState, TSignal, TPayload> model;

        public StateMachineModelBuilder(string name)
        {
            model = new StateMachineModel<TState, TSignal, TPayload>(name);
        }

        public StateMachineTransitionBuilder From(TState fromState)
        {
            return new StateMachineTransitionBuilder(fromState, this.model, this);
        }

        public IReadOnlyStateMachineModel<TState, TSignal, TPayload> Build()
        {
            return model;
        }

        public interface ITransitionTargetSelector
        {
            ITransitionSignalSelector To(TState toState);
        }

        public interface ITransitionSignalSelector
        {
            ITransitionGuardSelector On(TSignal signal);
        }

        public interface ITransitionGuardSelector
        {
            ITransitionActionSelector If(Func<TPayload, bool> guard);
        }

        public interface ITransitionActionSelector
        {
            StateMachineModelBuilder<TState, TSignal, TPayload> Do(Action<TPayload> payload);
        }

        public interface ITransitionGuardAndActionSelector : ITransitionActionSelector, ITransitionGuardSelector
        {
        }

        public class StateMachineTransitionBuilder : ITransitionTargetSelector, ITransitionSignalSelector, ITransitionGuardAndActionSelector
        {
            private readonly TState fromState;
            private readonly StateMachineModel<TState, TSignal, TPayload> model;
            private readonly StateMachineModelBuilder<TState, TSignal, TPayload> modelBuilder;
            private TState toState;
            private TSignal signal;
            private Func<TPayload, bool>? guard;

            public StateMachineTransitionBuilder(TState fromState, StateMachineModel<TState, TSignal, TPayload> model, StateMachineModelBuilder<TState, TSignal, TPayload> modelBuilder)
            {
                this.fromState = fromState;
                this.model = model;
                this.modelBuilder = modelBuilder;
            }

            public StateMachineModelBuilder<TState, TSignal, TPayload> Do(Action<TPayload> transitionFn)
            {
                model.AddState(fromState);
                model.AddState(toState);
                model.AddSignal(signal);

                model.AddTransition(fromState, toState, signal, guard, transitionFn);

                return modelBuilder;
            }

            public StateMachineModelBuilder<TState, TSignal, TPayload>.ITransitionActionSelector If(Func<TPayload, bool> guard)
            {
                this.guard = guard;
                return this;
            }

            public ITransitionGuardSelector On(TSignal signal)
            {
                this.signal = signal;
                return this;
            }

            public ITransitionSignalSelector To(TState toState)
            {
                this.toState = toState;
                return this;
            }
        }
    }
}
