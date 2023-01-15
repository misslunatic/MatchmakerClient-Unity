using Unity.VisualScripting;

// ReSharper disable once CheckNamespace
namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("Matchmaking Server Address")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_Address : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput set;
        
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput get;


        private bool state;
        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                IMatchmaker.ip = flow.GetValue<string>(set);
                return outputTrigger;
            });
            //Making the ControlOutput port visible and setting its key.
            outputTrigger = ControlOutput("");

            set = ValueInput<string>("set");
            get = ValueOutput<string>("get", (flow) => IMatchmaker.ip);
            Succession(inputTrigger, outputTrigger);
        }
    }
}