using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Scripts.CustomServer;
using Scripts.Matchmaker;
using UnityEngine;

[RequireComponent(typeof(Client))]
public class IMGUIMenuDemo : MonoBehaviour, IMatchmaker
{
    private List<string> ServerList = new();

    public static bool CreatePublic;

    private string connectText;

    private void Start()
    {
        IMatchmaker.listClient = gameObject.GetComponent<Client>();
    }

    private void Refresh()
    {
        IMatchmaker.Connect();
       /* var x = IMatchmaker.GetAllLobbyIdsByAttribute("public", "True");
        foreach (var y in x)
        {
            ServerList.Add(IMatchmaker.GetLobbyAttribute(y, "_auto_ip"));
        }*/
      //  IMatchmaker.Disconnect();
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 500), "Connection Manager");

        connectText = GUI.TextField(new Rect(30, 30, 160, 20), connectText);

        if (GUI.Button(new Rect(30, 60, 160, 20), "Connect"))
        {
            // Check if it is an IP
            NetworkManager.singleton.networkAddress = connectText;
            NetworkManager.singleton.StartClient();

            // Otherwise
            IMatchmaker.Connect();

            var uuidLobby = IMatchmaker.GetLobbyIdByUuid(connectText);
            var address = "";

            if (uuidLobby > 0)
            {
                NetworkManager.singleton.networkAddress = IMatchmaker.GetLobbyAttribute(uuidLobby, "_auto_uuid");
                IMatchmaker.Disconnect();
                NetworkManager.singleton.StartClient();
            }
            else
            {
                IMatchmaker.Disconnect();
            }
        }

        if (GUI.Button(new Rect(30, 100, 160, 20), "Start Host (Public)"))
        {
            CreatePublic = true;
            NetworkManager.singleton.StartHost();
        }

        if (GUI.Button(new Rect(30, 130, 160, 20), "Start Host (Private)"))
        {
            CreatePublic = false;
            NetworkManager.singleton.StartHost();
        }

        if (GUI.Button(new Rect(30, 170, 160, 20), "Refresh"))
        {
            Refresh();
        }

        int yPos = 200;
        
        foreach (var x in ServerList)
        {
            if(GUI.Button(new Rect(30, yPos+=30, 160, 20), x))
            {
                NetworkManager.singleton.networkAddress = x;
                NetworkManager.singleton.StartClient();
            }
        }
        
    }
}