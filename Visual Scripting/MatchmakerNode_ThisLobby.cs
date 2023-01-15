using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Matchmaker.Visual_Scripting
{
    [UnitTitle("This Lobby's ID")]
    [UnitSubtitle("Matchmaker API")]
    [UnitCategory("Matchmaking")]
    [TypeIcon(typeof(DummyScript_Icon))]
    public class MatchmakerNode_ThisLobby : Unit, IMatchmaker
    {

        
        [DoNotSerialize] // No need to serialize ports
        public ValueOutput get;


        private bool state;
        protected override void Definition()
        {
            get = ValueOutput<int>("get", (flow) =>
            {
                if (IMatchmaker.lobbyClient == null)
                {
                    Debug.LogError("Error in This Lobby's ID node: lobbyClient is null!");
                    return 0;
                }
                return IMatchmaker.lobbyClient.myId;
            });

        }
    }
}