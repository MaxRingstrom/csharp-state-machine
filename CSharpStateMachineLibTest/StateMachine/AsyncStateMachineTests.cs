using MaxRingstrom.CSharpStateMachineLib.StateMachine;
using MaxRingstrom.CSharpStateMachineLib.StateMachine.internals;
using Xunit.Abstractions;

namespace CSharpStateMachineLibTest.StateMachine
{
    public class AsyncStateMachineTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper output = output;

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

            var configuration = new StateMachineConfigurationBuilder<State, Signal, Payload>("Call state machine")

                // Idle -Dial-> Dialing
                .From(State.Idle).To(State.Dialing).On(Signal.Dial)
                    .If(payload => !string.IsNullOrWhiteSpace(payload.StringValue))
                    .Do(payload =>
                        {
                            theTargetValue = payload.StringValue;
                        })
                .Build();

            using (var stateMachine = new AsyncStateMachine<State, Signal, Payload>(configuration))
            {
                stateMachine.TransitionActivated += StateMachine_TransitionActivated;

                stateMachine.Init(State.Idle);
                await stateMachine.SendAsync(Signal.Dial, new Payload() { StringValue = expectedValue });
            }

            Assert.Equal(expectedValue, theTargetValue);
        }

        private void StateMachine_TransitionActivated(object? sender, TransitionActivatedEventArgs<State, Signal, Payload> e)
        {
            output.WriteLine($"{e.From} -{e.Signal}-> {e.To}");
        }
    }
}