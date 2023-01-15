using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Matchmaker.Visual_Scripting
{
    
    [UnitTitle("Get Client Attribute")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_GetClientAttribute : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnSuccess;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnFail;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports
        public ValueInput lobbyId;
        
        [DoNotSerialize] // No need to serialize ports
        public ValueInput name;
        
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput value;
        
        private string resultValue;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                try
                {
                    resultValue =
                        IMatchmaker.GetLobbyAttribute(flow.GetValue<int>(lobbyId), flow.GetValue<string>(name));

                    if (resultValue != null)
                    {
                        return outputOnSuccess;
                    }

                    resultValue = "";
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in Get Lobby Attribute node: {ex}");
                }
                
                resultValue = "";
                return outputOnFail;
            });
            //Making the ControlOutput port visible and setting its key.
            outputOnSuccess = ControlOutput("onSuccess");
            
            outputOnFail = ControlOutput("onFail");
            
            value = ValueOutput<string>("value", (flow) => resultValue);

            name = ValueInput<string>("Name", "");
            
            lobbyId = ValueInput<int>("clientId");
            
            
            Succession(inputTrigger, outputOnFail);
            Succession(inputTrigger, outputOnSuccess);
            
        }
    }
}