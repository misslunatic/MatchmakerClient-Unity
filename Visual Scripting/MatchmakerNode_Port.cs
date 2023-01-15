using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("Matchmaking Server Port")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_Port : Unit, IMatchmaker
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
                IMatchmaker.port = flow.GetValue<int>(set);
                return outputTrigger;
            });
            //Making the ControlOutput port visible and setting its key.
            outputTrigger = ControlOutput("");

            set = ValueInput<int>("set");
            get = ValueOutput<int>("get", (flow) => IMatchmaker.port);
            Succession(inputTrigger, outputTrigger);
        }
    }
}