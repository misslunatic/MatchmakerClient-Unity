using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Matchmaker.Visual_Scripting
{
    
    [UnitTitle("Set Client Attribute")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_SetClientAttribute : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnSuccess;//Adding the ControlOutput port variable.
        
        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputOnFail;//Adding the ControlOutput port variable.
    
        [DoNotSerialize] // No need to serialize ports
        public ValueInput name;
        
        [DoNotSerialize] // No need to serialize ports
        public ValueInput value;

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            inputTrigger = ControlInput("", (flow) =>
            {
                try
                {
                    var result =
                        IMatchmaker.SetClientAttribute(flow.GetValue<string>(name), flow.GetValue<string>(value));

                    if (result)
                    {
                        return outputOnSuccess;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in Set Client Attribute node: {ex}");
                }

                return outputOnFail;
            });
            //Making the ControlOutput port visible and setting its key.
            outputOnSuccess = ControlOutput("oOnSuccess");
            
            outputOnFail = ControlOutput("onFail");

            name = ValueInput<string>("Name", "");
            
            value = ValueInput<string>("value", "");
            
            Succession(inputTrigger, outputOnFail);
            Succession(inputTrigger, outputOnSuccess);
            
        }
    }
}