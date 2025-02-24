using System;

namespace MaxRingstrom.CSharpStateMachineLib.StateMachine
{
    public class StateMachineConfigurationBuilder<TState, TSignal, TPayload> where TState : struct, IConvertible, IComparable where TSignal : struct, IConvertible, IComparable where TPayload : class
    {
        private readonly StateMachineConfiguration<TState, TSignal, TPayload> configuration;

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
            private readonly StateMachineConfiguration<TState, TSignal, TPayload> configuration;
            private readonly StateMachineConfigurationBuilder<TState, TSignal, TPayload> configurationBuilder;
            private TState toState;
            private TSignal signal;
            private Func<TPayload, bool>? guard;

            public StateMachineTransitionBuilder(TState fromState, StateMachineConfiguration<TState, TSignal, TPayload> configuration, StateMachineConfigurationBuilder<TState, TSignal, TPayload> configurationBuilder)
            {
                this.fromState = fromState;
                this.configuration = configuration;
                this.configurationBuilder = configurationBuilder;
            }

            public StateMachineConfigurationBuilder<TState, TSignal, TPayload> Do(Action<TPayload> transitionFn)
            {
                configuration.AddState(fromState);
                configuration.AddState(toState);
                configuration.AddSignal(signal);

                configuration.AddTransition(fromState, toState, signal, guard, transitionFn);

                return configurationBuilder;
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
