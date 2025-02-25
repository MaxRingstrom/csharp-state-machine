using MaxRingstrom.CSharpStateMachineLib.StateMachine;
using MaxRingstrom.CSharpStateMachineLib.StateMachine.internals;
using Xunit.Abstractions;

namespace CSharpStateMachineLibTest.StateMachine.internals
{
    enum State
    {
        Idle,
        StateWithAutoTransition,
        AutoTarget
    }
    enum Signal
    {
        Start
    }

    class Payload
    {
        public string? StringValue { get; init; }
    }

    public class StateMachineEngineTests(ITestOutputHelper output)
    {
        private ITestOutputHelper output = output;

        [Fact]
        public void Init_WithState_SetsCurrentState()
        {
            var configuration = new StateMachineConfigurationBuilder<State, Signal, Payload>("Test machine")
                .Build();

            var engine = new StateMachineEngine<State, Signal, Payload>(configuration);
            engine.TransitionActivated += LogTransitionActivated;
            engine.Init(State.Idle);

            Assert.Equal(State.Idle, engine.CurrentState);
        }

        [Fact]
        public void Init_WithAutoTransition_EndsUpInNextState()
        {
            var configuration = new StateMachineConfigurationBuilder<State, Signal, Payload>("Test machine")
                .From(State.Idle).To(State.AutoTarget).Automatically().Done()
                .Build();

            var engine = new StateMachineEngine<State, Signal, Payload>(configuration);
            engine.TransitionActivated += LogTransitionActivated;
            engine.Init(State.Idle);

            Assert.Equal(State.AutoTarget, engine.CurrentState);
        }

        [Fact]
        public void SendAsync_WithTransitionToStateWithAutoTransition_EndsUpInNextState()
        {
            var configuration = new StateMachineConfigurationBuilder<State, Signal, Payload>("Test machine")
                .From(State.Idle).To(State.StateWithAutoTransition).On(Signal.Start).Done()
                .From(State.StateWithAutoTransition).To(State.AutoTarget) .Automatically().Done()
                .Build();

            var engine = new StateMachineEngine<State, Signal, Payload>(configuration);
            engine.TransitionActivated += LogTransitionActivated;
            engine.Init(State.Idle);

            Assert.Equal(State.Idle, engine.CurrentState);

            engine.ProcessSignal(new(Signal.Start, null, new ManualResetEventSlim()));

            Assert.Equal(State.AutoTarget, engine.CurrentState);
        }

        private void LogTransitionActivated(object? sender, TransitionActivatedEventArgs<State, Signal, Payload> e)
        {
            string signalName = e.Signal?.ToString() ?? "_auto_";
            output.WriteLine($"{e.From} -{signalName}-> {e.To}");
        }
    }
}
