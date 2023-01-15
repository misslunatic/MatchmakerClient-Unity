
using Scripts.Matchmaker;
using Unity.VisualScripting;

namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("Connect to List Server")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_Connect : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnSuccess; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnFail; //Adding the ControlOutput port variable.

        private bool resultValue;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("",
                (flow) => { return IMatchmaker.Connect() ? outputOnSuccess : outputOnFail; });
            //Making the ControlOutput port visible and setting its key.
            outputOnSuccess = ControlOutput("onSuccess");
            outputOnFail = ControlOutput("onFail");

            Succession(inputTrigger, outputOnSuccess);
            Succession(inputTrigger, outputOnFail);

        }
    }

}