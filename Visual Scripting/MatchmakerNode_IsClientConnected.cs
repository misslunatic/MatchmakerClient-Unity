using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("Is Client Connected")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_IsClientConnected : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput get;

        protected override void Definition()
        {
            get = ValueOutput<bool>("get", (flow) =>
            {
                if (IMatchmaker.listClient == null)
                {
                    Debug.LogError("Error in Is Client Connected node: listClient is null!");
                    return false;
                }

                return IMatchmaker.listClient.IsConnected;
            });
        }
    }
}