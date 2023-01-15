using Unity.VisualScripting;

namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("Disconnect from List Server")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_Disconnect : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.
        
    
        private bool resultValue;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                IMatchmaker.Disconnect();

                return outputTrigger;
            });
            //Making the ControlOutput port visible and setting its key.
            outputTrigger = ControlOutput("");

            Succession(inputTrigger, outputTrigger);

        }
    }

}