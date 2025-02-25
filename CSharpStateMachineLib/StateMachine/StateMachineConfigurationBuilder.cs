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

        public IFromStateSelectedOptions From(TState fromState)
        {
            return new StateMachineTransitionBuilder(fromState, configuration, this);
        }

        public IReadOnlyStateMachineConfiguration<TState, TSignal, TPayload> Build()
        {
            return configuration;
        }

        public interface IFromStateSelectedOptions
        {
            IToStateSelectedOptions To(TState toState);
        }

        public interface IToStateSelectedOptions
        {
            ISignalSelectedOptions On(TSignal signal);
            ISignalSelectedOptions Automatically();
        }

        public interface ISignalSelectedOptions : IGuardSkippedOrSelectedOptions
        {
            IGuardSkippedOrSelectedOptions If(Func<TPayload, bool> guard);
        }

        public interface IGuardSkippedOrSelectedOptions 
        {
            StateMachineConfigurationBuilder<TState, TSignal, TPayload> Do(Action<TPayload> payload);
            StateMachineConfigurationBuilder<TState, TSignal, TPayload> Done();
        }

        public class StateMachineTransitionBuilder : IFromStateSelectedOptions, IToStateSelectedOptions, ISignalSelectedOptions
        {
            private readonly TState fromState;
            private readonly StateMachineConfiguration<TState, TSignal, TPayload> configuration;
            private readonly StateMachineConfigurationBuilder<TState, TSignal, TPayload> configurationBuilder;
            private TState toState;
            private TSignal? signal;
            private Func<TPayload, bool>? guard;

            public StateMachineTransitionBuilder(TState fromState, StateMachineConfiguration<TState, TSignal, TPayload> configuration, StateMachineConfigurationBuilder<TState, TSignal, TPayload> configurationBuilder)
            {
                this.fromState = fromState;
                this.configuration = configuration;
                this.configurationBuilder = configurationBuilder;
            }

            public StateMachineConfigurationBuilder<TState, TSignal, TPayload> Do(Action<TPayload>? transitionFn)
            {
                configuration.AddState(fromState);
                configuration.AddState(toState);
                if (signal != null)
                {
                    configuration.AddSignal((TSignal)signal);
                }

                configuration.AddTransition(fromState, toState, signal, guard, transitionFn);

                return configurationBuilder;
            }
            public StateMachineConfigurationBuilder<TState, TSignal, TPayload> Done()
            {
                return Do(null);
            }


            public IGuardSkippedOrSelectedOptions If(Func<TPayload, bool> guard)
            {
                this.guard = guard;
                return this;
            }

            public ISignalSelectedOptions On(TSignal signal)
            {
                this.signal = signal;
                return this;
            }

            public ISignalSelectedOptions Automatically()
            {
                signal = null;
                return this;
            }

            public IToStateSelectedOptions To(TState toState)
            {
                this.toState = toState;
                return this;
            }
        }
    }
}
