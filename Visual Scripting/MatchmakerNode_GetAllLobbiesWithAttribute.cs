using System;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Matchmaker.Visual_Scripting
{
    
    [UnitTitle("Get All Lobby IDs with Attribute")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_GetAllLobbiesWithAttribute : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputIfSome;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputIfNone;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput idList;

        [DoNotSerialize] // No need to serialize ports
        public ValueInput attribName;

        [DoNotSerialize] // No need to serialize ports
        public ValueInput attribValue;

        public List<int> resultValue;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                try
                {
                    resultValue = IMatchmaker.GetAllLobbyIdsByAttribute(flow.GetValue<string>(attribName),
                        flow.GetValue<string>(attribValue));
;                    if (resultValue is { Count: > 0 })
                    {
                        return outputIfSome;
                    }

                    return outputIfNone;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in Get All Lobby IDs node: {ex}");
                }

                return outputIfNone;
            });
            //Making the ControlOutput port visible and setting its key.
            outputIfNone = ControlOutput("ifNone");
            
            outputIfSome = ControlOutput("ifSome");
            
            idList = ValueOutput<List<int>>("result", (flow) => resultValue);

            attribName = ValueInput<string>("attributeName");
            
            attribValue = ValueInput<string>("attributeValue");
            
            
            Succession(inputTrigger, outputIfSome);
            Succession(inputTrigger, outputIfNone);
            
            Assignment(inputTrigger, idList);
        }
    }
}