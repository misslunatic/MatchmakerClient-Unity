using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("Is Lobby Connected")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_IsLobbyConnected : Unit, IMatchmaker
    {
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput get;

        protected override void Definition()
        {
            get = ValueOutput<bool>("get", (flow) =>
            {
                if (IMatchmaker.lobbyClient == null)
                {
                    Debug.LogError("Error in Is Lobby Connected node: listClient is null!");
                    return false;
                }

                return IMatchmaker.lobbyClient.IsConnected;
            });
        }
    }
}