using Mirror;
using Scripts.CustomServer;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Matchmaker.Visual_Scripting
{
    
    [UnitTitle("Set Client Objects")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_SetClientObjects : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.
    
        [DoNotSerialize] // No need to serialize ports
        public ValueInput listclient; 
        
        [DoNotSerialize] // No need to serialize ports
        public ValueInput lobbyclient; 

        private bool resultValue;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                if (flow.GetValue<Client>(listclient) != null)
                {
                    IMatchmaker.listClient = flow.GetValue<Client>(listclient);
                }
                if (flow.GetValue<Client>(lobbyclient) != null)
                {
                    IMatchmaker.lobbyClient = flow.GetValue<Client>(lobbyclient);
                }

                return outputTrigger;
            });
            //Making the ControlOutput port visible and setting its key.
            outputTrigger = ControlOutput("");

            listclient = ValueInput<Client>("listClient", null);
            
            lobbyclient = ValueInput<Client>("lobbyClient", null);
            
            Succession(inputTrigger, outputTrigger);
        }
    }
}