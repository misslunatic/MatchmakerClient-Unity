using System.Collections;
using System.Collections.Generic;
using Mirror;
using Scripts.CustomServer;
using Scripts.Matchmaker;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Client))]
public class LobbyManager : NetworkBehaviour
{
    public Text text;
    
    public override void OnStartServer()
    {
        if (isServer)
        {
            IMatchmaker.lobbyClient = gameObject.GetComponent<Client>();
            IMatchmaker.ConnectLobby();
            IMatchmaker.SetLobbyAttribute("public", IMGUIMenuDemo.CreatePublic.ToString());
            text.text = IMatchmaker.GetLobbyAttributeDirect("_auto_uuid");
        }
    }
}
