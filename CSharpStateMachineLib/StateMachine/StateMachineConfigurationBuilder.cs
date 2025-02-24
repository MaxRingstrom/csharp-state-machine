using System;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine
{
    public class StateMachineConfigurationBuilder<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        private StateMachineConfiguration<TState, TSignal, TPayload> configuration;

        public StateMachineConfigurationBuilder(string name)
        {
            configuration = new StateMachineConfiguration<TState, TSignal, TPayload>(name);
        }

        public StateMachineTransitionBuilder From(TState fromState)
        {
            return new StateMachineTransitionBuilder(fromState, configuration, this);
        }

        public IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> Build()
        {
            return configuration;
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
            StateMachineConfigurationBuilder<TState, TSignal, TPayload> Do(Action<TPayload> payload);
        }

        public interface ITransitionGuardAndActionSelector : ITransitionActionSelector, ITransitionGuardSelector
        {
        }

        public class StateMachineTransitionBuilder : ITransitionTargetSelector, ITransitionSignalSelector, ITransitionGuardAndActionSelector
        {
            private readonly TState fromState;
            private readonly StateMachineConfiguration<TState, TSignal, TPayload> model;
            private readonly StateMachineConfigurationBuilder<TState, TSignal, TPayload> modelBuilder;
            private TState toState;
            private TSignal signal;
            private Func<TPayload, bool>? guard;

            public StateMachineTransitionBuilder(TState fromState, StateMachineConfiguration<TState, TSignal, TPayload> model, StateMachineConfigurationBuilder<TState, TSignal, TPayload> modelBuilder)
            {
                this.fromState = fromState;
                this.model = model;
                this.modelBuilder = modelBuilder;
            }

            public StateMachineConfigurationBuilder<TState, TSignal, TPayload> Do(Action<TPayload> transitionFn)
            {
                model.AddState(fromState);
                model.AddState(toState);
                model.AddSignal(signal);

                model.AddTransition(fromState, toState, signal, guard, transitionFn);

                return modelBuilder;
            }

            public StateMachineConfigurationBuilder<TState, TSignal, TPayload>.ITransitionActionSelector If(Func<TPayload, bool> guard)
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
