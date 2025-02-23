using MaxRingstrom.CSharpStateMachineLib;
using Xunit.Abstractions;

namespace CSharpStateMachineLibTest
{
    public class StateMachineTests
    {

        private readonly ITestOutputHelper output;

        public StateMachineTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        enum State
        {
            Idle,
            Dialing,
            Ringing,
            InCall,
            HangingUp
        }
        enum Signal
        {
            Dial,
            HangUp,
            PlayOutgoingCallToneTimeout,
            Reject,
            IncomingCall,
            PlayRingToneTimeout,
            LocalAccept,
            RemoteAccept,
            RemoteDisconnect,
            HangUpTonePlayed
        }

        class Payload
        {
            public string? StringValue { get; init; }
        }

        [Fact]
        public async void SendAsync_WithTransitionFn_RunWhenGuardSatisfied()
        {

            string? theTargetValue = "default";
            string expectedValue = "expected";

            var model = new StateMachineModelBuilder<State, Signal, Payload>("Call state machine")

                // Idle -Dial-> Dialing
                .From(State.Idle).To(State.Dialing).On(Signal.Dial)
                    .If(payload => !string.IsNullOrWhiteSpace(payload.StringValue))
                    .Do(payload =>
                        {
                            theTargetValue = payload.StringValue;
                        })
                .Build();

            using (var stateMachine = new StateMachine<State, Signal, Payload>(model))
            {
                stateMachine.TransitionActivated += StateMachine_TransitionActivated;

                stateMachine.Init(State.Idle);
                await stateMachine.SendAsync(Signal.Dial, new Payload() { StringValue = expectedValue });
            }
            Assert.Equal(expectedValue, theTargetValue);
        }

        private void StateMachine_TransitionActivated(object? sender, StateMachine<State, Signal, Payload>.TransitionActivatedEventArguments e)
        {
            output.WriteLine($"{e.From} -{e.Signal}-> {e.To}");
        }
    }
}