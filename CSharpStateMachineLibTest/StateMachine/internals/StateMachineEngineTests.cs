using MaxRingstrom.CSharpStateMachineLib.StateMachine;
using MaxRingstrom.CSharpStateMachineLib.StateMachine.internals;
using Xunit.Abstractions;

namespace CSharpStateMachineLibTest.StateMachine.internals
{
    enum State
    {
        Idle,
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

        private void LogTransitionActivated(object? sender, TransitionActivatedEventArgs<State, Signal, Payload> e)
        {
            output.WriteLine($"{e.From} -{e.Signal}-> {e.To}");
        }
    }
}
