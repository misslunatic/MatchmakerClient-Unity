using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Matchmaker.Visual_Scripting
{
    
    [UnitTitle("Get Lobby By UUID")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_GetLobbyByUuid : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnSuccess;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnFail;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports
        public ValueInput uuid;
        
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput id;

        public int resultValue;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                try
                {
                    resultValue = IMatchmaker.GetLobbyIdByUuid(flow.GetValue<string>(uuid).ToUpper());
                    if (resultValue > 0)
                    {
                        return outputOnSuccess;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in Get Lobby By Uuid node: {ex}");
                }

                return outputOnFail;
            });
            //Making the ControlOutput port visible and setting its key.
            outputOnSuccess = ControlOutput("onSuccess");
            
            outputOnFail = ControlOutput("onFail");

            uuid = ValueInput<string>("uuid", "");
            id = ValueOutput<int>("result", (flow) => resultValue);

            Succession(inputTrigger, outputOnFail);
            Succession(inputTrigger, outputOnSuccess);
            
            Assignment(inputTrigger, id);
        }
    }
}