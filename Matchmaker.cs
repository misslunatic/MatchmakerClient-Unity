/*
 * MIT License
 *
 * Copyright (c) 2020 Tom Weiland
 * Copyright (c) 2022 Vincent Dowling
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */


using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Scripts.CustomServer;
using UnityEngine;
using Client = Scripts.CustomServer.Client;

// ReSharper disable once CheckNamespace
namespace Scripts.Matchmaker
{
    public interface IMatchmaker
    {
        /**************************************************************************************************************
         * Matchmaker Programming Interface - External
         **************************************************************************************************************/

        // Global Variables --------------------------------------------------------------------------------------------
        public static Client listClient;
        public static Client lobbyClient;
        public static int port = Config.Port;
        public static string ip = Config.Address;
        public static string GameId = Config.GameId;

        public static readonly Client.ClientPackets apiPackets = new Client.ClientPackets(
            new Dictionary<int, Client.ClientPackets.PacketHandler>()
            {
                { 10, API.APIPacketHandlers.ReceiveLobbyId },
                { 11, API.APIPacketHandlers.GetLobbyAttributesReceived },
                { 12, API.APIPacketHandlers.FinishedSendingLobbyIds },
            });

        // API Functions - Connection Management -----------------------------------------------------------------------

        /// <summary>
        /// MatchmakerNode_Connect the List Client.
        /// </summary>
        public static bool Connect()
        {
            if (listClient != null)
            {
                listClient.ip = ip;
                listClient.port = port;
                listClient.welcomeReceived = false;
                if (!listClient.ConnectToServer(apiPackets)) return false;
                var startTime = DateTime.Now.Ticks;

                while (listClient.welcomeReceived == false && ((DateTime.Now.Ticks - startTime) < 30000000))
                {
                }

                if ((DateTime.Now.Ticks - startTime) < 30000000) return true;
                Debug.LogError("Failed to connect to List Server: Connection timed out!");
            }
            else
            {
                Debug.LogError(
                    "Attempted to connect to the List Server while listClient is null. Please set Matchmaker.listClient first!");
            }

            return false;
        }

        /// <summary>
        /// MatchmakerNode_Connect the Lobby Client.
        /// </summary>
        public static bool ConnectLobby()
        {
   
            lobbyClient.ip = ip;
            lobbyClient.port = port + 2;
            lobbyClient.welcomeReceived = false;
            if (lobbyClient.ConnectToServer(new Client.ClientPackets(new Dictionary<int, Client.ClientPackets.PacketHandler>())))
            {
                var startTime = DateTime.Now.Ticks;

                while (lobbyClient.welcomeReceived == false && ((DateTime.Now.Ticks - startTime) < 30000000))
                {
                }

                if ((DateTime.Now.Ticks - startTime) < 30000000) return true;
                Debug.LogError("Failed to connect to to Lobby Server: Connection timed out!");
                return false;
            }

            return false;
        }

        /// <summary>
        /// Disconnect the List Client.
        /// </summary>
        public static void Disconnect()
        {
            if (listClient != null)
            {
                listClient.Disconnect();
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to disconnect a List Client while listClient is null. Please set Matchmaker.listClient first!");
            }
        }

        /// <summary>
        /// Disconnect the Lobby Client.
        /// </summary>
        public static void DisconnectLobby()
        {
            Debug.Log("Disconnecting from Lobby server...");
            // SetLobbyAttribute("_auto_uuid", "DISCONNECTED_LOBBY");
            // SetLobbyAttribute("_auto_ip", "DISCONNECTED_LOBBY");
            // SetLobbyAttribute("_auto_port", "DISCONNECTED_LOBBY");
            lobbyClient.Disconnect();
        }

        // API Functions - Client Attributes ---------------------------------------------------------------------------
        /// <summary>
        /// Set an Attribute on the List Client.
        /// </summary>
        /// <param name="name">Name of the Attribute to set</param>
        /// <param name="value">Value of the Attribute to set to</param>
        public static bool SetClientAttribute(string name, string value)
        {
            if (listClient != null)
            {
                if (listClient.IsConnected)
                {
                    if (ClientSend.SetClientAttribute(listClient, name, value))
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to set a List Client Attribute while listClient is null. Please set Matchmaker.listClient first!");
            }

            return false;
        }

        /// <summary>
        /// Get a List Client's Attribute.
        /// </summary>
        /// <param name="clientId">What List Client to find the Attribute in</param>
        /// <param name="name">Name of the Attribute to get.</param>
        /// <returns>The value of the Attribute. Returns null on failure.</returns>
        public static string GetClientAttribute(int clientId, string name)
        {
            if (listClient != null)
            {
                if (listClient.IsConnected)
                {
                    var foo = ClientSend.GetClientAttribute(listClient, clientId, name);


                    // Debug check - Is the received Attributes Name not the same as the requested one?
                    if (foo.name == name) return foo.value;
                    Debug.LogError("Requested Attribute Name and Received Attribute Name do not match!");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to get a List Client Attribute while listClient is null. Please set Matchmaker.listClient first!");
            }


            return null;
        }

        // API Functions - Lobby Attributes ----------------------------------------------------------------------------
        /// <summary>
        /// Get a Lobby Client's Attribute straight from the Lobby Server.
        /// </summary>
        /// <param name="clientId">What Lobby Client to find the Attribute in</param>
        /// <param name="name">Name of the Attribute to get.</param>
        /// <returns>The value of the Attribute. Returns null on failure.</returns>
        public static string GetLobbyAttributeDirect(string name)
        {
            if (lobbyClient != null)
            {
                if (lobbyClient.IsConnected)
                {
                    var foo = ClientSend.GetClientAttribute(lobbyClient, lobbyClient.myId, name);


                    // Debug check - Is the received Attributes Name not the same as the requested one?
                    if (foo.name == name) return foo.value;
                    Debug.LogError("Requested Attribute Name and Received Attribute Name do not match!");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to get a Lobby Client's Attribute *directly* while lobbyClient is null. Please set Matchmaker.lobbyClient first!");
            }

            return null;
        }

        /// <summary>
        /// Set a Attribute on the Lobby Client.
        /// </summary>
        /// <param name="name">Name of the Attribute to set.</param>
        /// <param name="value">What value to set the Attribute to.</param>
        public static bool SetLobbyAttribute(string name, string value)
        {
            if (lobbyClient != null)
            {
                if (lobbyClient.IsConnected)
                {
                    return ClientSend.SetClientAttribute(lobbyClient, name, value);
                }
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to set a Lobby Client Attribute while lobbyClient is null. Please set Matchmaker.lobbyClient first!");
            }

            return false;
        }

        /// <summary>
        /// Get a Lobby Client Attribute by passing through the List Server.
        /// </summary>
        /// <param name="clientId">What Lobby Client to find the Attribute in</param>
        /// <param name="name">Name of the Attribute to get.</param>
        /// <returns>The value of the Attribute. Returns null on failure.</returns>
        public static string GetLobbyAttribute(int clientId, string name)
        {
            if (listClient != null)
            {
                if (listClient.IsConnected)
                {
                    var foo = API.APIClientPackets.GetLobbyAttribute(listClient, clientId, name);

                    if (foo == null)
                    {
                        Debug.LogWarning("Received Lobby Attribute is null!");
                        return null;
                    }

                    // Debug check - Is the received Attributes Name not the same as the requested one?
                    if (foo.name == name) return foo.value;
                    Debug.LogError(
                        $"Requested Attribute Name {foo.name} and Received Attribute Name {name} do not match!");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to get a Lobby Client's Attribute while listClient is null. Please set Matchmaker.listClient first!");
            }


            return null;
        }

        // API Functions - Lobby Discovery -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Lobby Client's ID by its UUID.
        /// </summary>
        /// <param name="uuid">The UUID to find a Lobby Client with.</param>
        /// <returns>A Lobby Client's ID with matching UUID.</returns>
        public static int GetLobbyIdByUuid(string uuid)
        {
            if (listClient != null)
            {
                if (listClient.IsConnected)
                {
                    API.APIPacketHandlers.lobbyIds.Clear();
                    var foo = GetAllLobbyIdsByAttribute("_auto_uuid", uuid);
                    if(foo.Count > 1)
                    {
                        Debug.LogWarning("Found more than one lobby with UUID!");
                    }
                    if (foo.Count <= 0)
                    {
                        Debug.LogWarning(
                            "Failed to find Lobby IDs with matching UUID!");
                        return 0;
                    }
                    
                    else
                    {  
                        return foo[0];
                    }
                }
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to get a Lobby Client's ID through a UUID while listClient is null. Please set Matchmaker.listClient first!");
            }


            return 0;
        }
        
        /// <summary>
        /// Gets all Lobby IDs by matching Attribute
        /// </summary>
        /// <param name="attribName">The name of the Attribute</param>
        /// <param name="attribValue">The value of the Attribute</param>
        public static List<int> GetAllLobbyIdsByAttribute(string attribName, string attribValue)
        {
            API.APIPacketHandlers.lobbyIds.Clear();
            if (listClient != null)
            {
                if (!listClient.IsConnected) return API.APIPacketHandlers.lobbyIds;
                
                var foo = API.APIClientPackets.RequestLobbyIdsWithMatchingAttribute(listClient, attribName, attribValue);
                if (!foo)
                {
                    Debug.LogWarning(
                        "Failed to find Lobby IDs with matching Attrbute!");
                        
                }
                return API.APIPacketHandlers.lobbyIds;
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to get all lobby IDs with matching Attribute while listClient is null. Please set Matchmaker.listClient first!");
            }


            return API.APIPacketHandlers.lobbyIds;
        }

        /// <summary>
        /// Gets a list of the IDs of all connected Lobby Clients. Also calls a callback at Matchmaker.API.lobbyCallback for every ID.
        /// </summary>
        /// <returns>List of Lobby Client IDs</returns>
        [CanBeNull]
        public static List<int> GetAllLobbyIds()
        {
            if (listClient != null)
            {
                API.APIPacketHandlers.lobbyIds.Clear();

                Debug.Log("GetAllLobbyIds() called.");
                if (listClient.IsConnected)
                {
                    API.APIClientPackets.RequestAllLobbyIds(listClient);
                }

                Debug.LogWarning("Attempted to request all Lobby IDs while Client is disconnected!");
            }
            else
            {
                Debug.LogWarning(
                    "Attempted to get a list of all Lobby IDs while listClient is null. Please set Matchmaker.listClient first!");
            }

            Debug.Log($"Lobby ID list length: {API.APIPacketHandlers.lobbyIds.Count}");
            return API.APIPacketHandlers.lobbyIds;
        }

        /**************************************************************************************************************
         * Matchmaker Programming Interface - Internal
         **************************************************************************************************************/
        /// <summary>
        /// Contains the internal functionality for the Matchmaker API
        /// </summary>
        public static class API
        {
            // API Internals - Global Variables ------------------------------------------------------------------------
            public delegate void UserCallbackDelegate(int userId);

            /// <summary>
            /// Gets called back whenever a Lobby Client ID is received by GetAllLobbyIds().
            /// </summary>
            public static UserCallbackDelegate lobbyCallback;

            private static readonly List<Action> ExecuteOnApiThread = new();
            private static readonly List<Action> ExecuteCopiedOnApiThread = new();
            private static bool _actionToExecuteOnApiThread;

            private const int TicksPerSec = 30;
            private const float MsPerTick = 1000f / TicksPerSec;
            private static bool _isRunning = true;

            private static Thread _apiThread;

            // API Internals - API Thread Manager ----------------------------------------------------------------------
            private static void APIThread(object data)
            {
                Debug.Log($"MatchmakerBehaviour thread started. Running at {TicksPerSec} ticks per second.");
                var nextLoop = DateTime.Now;

                while (_isRunning)
                {
                    while (nextLoop < DateTime.Now)
                    {
                        UpdateAPI();

                        nextLoop = nextLoop.AddMilliseconds(MsPerTick);

                        if (nextLoop > DateTime.Now)
                        {
                            Thread.Sleep(nextLoop - DateTime.Now);
                        }
                    }
                }
            }

            public static void StartThreads()
            {
                // if (apiThread != null || _isRunning != false) return;

                _isRunning = true;
                _apiThread = new Thread(APIThread);
                _apiThread.Start(0);
            }


            public static void StopThreads()
            {
                _isRunning = false;
            }

            /// <summary>Sets an action to be executed on the main thread.</summary>
            /// <param name="action">The action to be executed on the main thread.</param>
            public static void ExecuteOnAPIThread(Action action)
            {
                if (action == null)
                {
                    Debug.Log("No action to execute on MatchmakerBehaviour thread!");
                    return;
                }

                lock (ExecuteOnApiThread)
                {
                    ExecuteOnApiThread.Add(action);
                    _actionToExecuteOnApiThread = true;
                }
            }


            /// <summary>
            /// Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.
            /// </summary>
            private static void UpdateAPI()
            {
                if (_actionToExecuteOnApiThread)
                {
                    ExecuteCopiedOnApiThread.Clear();
                    lock (ExecuteOnApiThread)
                    {
                        ExecuteCopiedOnApiThread.AddRange(ExecuteOnApiThread);
                        ExecuteOnApiThread.Clear();
                        _actionToExecuteOnApiThread = false;
                    }

                    foreach (var t in ExecuteCopiedOnApiThread)
                    {
                        t();
                    }
                }
            }

            // API Internals - Packet Handling -------------------------------------------------------------------------
            public static class APIPacketHandlers
            {
                public static readonly List<int> lobbyIds = new();
                public static bool finishedSendingIds;
                public static bool receivedId;

                public static void ReceiveLobbyId(Client client, Packet packet)
                {
                    receivedId = true;
                    int userId = packet.ReadInt();
                    int mod = packet.ReadInt();
                    Debug.Log("Received lobby id: " + userId.ToString());
                    lobbyIds.Add(userId);
                    switch (mod)
                    {
                        case 0:
                        {
                            if (API.lobbyCallback != null)
                            {
                                API.lobbyCallback.Invoke(userId);
                            }

                            break;
                        }
                        default:
                            client.lastReceivedInt = userId;
                            break;
                    }
                }


                public static void GetLobbyAttributesReceived(Client client, Packet packet)
                {
                    int requestedId = packet.ReadInt();
                    string name = packet.ReadString();
                    string value = packet.ReadString();

                    Debug.Log("Received lobby attributes (Obj. ID: " + requestedId.ToString() + "): '" + name + "', '" +
                              value + "'.");

                    client.lastReceivedAttrib = new ClientHandle.ClientAttrib(requestedId, name, value);
                }

                public static void FinishedSendingLobbyIds(Client client, Packet packet)
                {
                    Debug.Log("Finished receiving Lobby IDs.");
                    finishedSendingIds = true;
                }
            }

            // API Internals - Packet Sending --------------------------------------------------------------------------
            public static class APIClientPackets
            {
                // ReSharper disable once FieldCanBeMadeReadOnly.Global
                // ReSharper disable once ConvertToConstant.Global
                // ReSharper disable once MemberCanBePrivate.Global
                public static long timeOutTime = 300000000;

                public static bool RequestAllLobbyIds(Client client)
                {
                    if (client.IsConnected)
                    {
                        APIPacketHandlers.finishedSendingIds = false;
                        Debug.Log("Getting all the connected lobby IDs...");
                        using var packet = new Packet(10);
                        packet.Write(client.myId);
                        ClientSend.SendTcpData(client, packet);


                        var startTime = DateTime.Now.Ticks;

                        while (!APIPacketHandlers.finishedSendingIds &&
                               ((DateTime.Now.Ticks - startTime) < timeOutTime))
                        {
                            if (!APIPacketHandlers.receivedId) continue;
                            startTime = DateTime.Now.Ticks;
                            APIPacketHandlers.receivedId = false;
                        }

                        if (!((DateTime.Now.Ticks - startTime) < timeOutTime))
                        {
                            Debug.LogError(
                                $"RequestAllLobbyIds: Request timed out! Returning {client.lastReceivedInt}");
                            return false;
                        }

                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("Tried to request all Lobby IDs while the Client is disconnected!");
                        return false;
                    }
                }


                public static bool RequestLobbyIdsWithMatchingAttribute(Client client, string attribName,
                    string attribValue)
                {
                    Debug.Log("Getting a lobby ID by UUID...");
                 
                    using var packet = new Packet(11);
                    APIPacketHandlers.finishedSendingIds = false;

                    packet.Write(client.myId);
                    packet.Write(attribName);
                    packet.Write(attribValue);
                    
                    ClientSend.SendTcpData(client, packet);
                    var startTime = DateTime.Now.Ticks;

                    while (!APIPacketHandlers.finishedSendingIds &&
                           ((DateTime.Now.Ticks - startTime) < timeOutTime))
                    {
                      //  if (!APIPacketHandlers.receivedId) continue;
                       // startTime = DateTime.Now.Ticks;
                      //  APIPacketHandlers.receivedId = false;
                    }

                    if (!((DateTime.Now.Ticks - startTime) < timeOutTime))
                    {
                        Debug.LogError(
                            $"RequestLobbyIdsWithMatchingAttribute: Request timed out!");
                        return false;
                    }

                    return true;
                }

                public static ClientHandle.ClientAttrib GetLobbyAttribute(Client client, int requestedId, string name)
                {
                    Debug.Log("Getting a lobby attribute...");
                    using var packet = new Packet(12);
                    packet.Write(client.myId);
                    packet.Write(requestedId);
                    packet.Write(name);

                    ClientSend.SendTcpData(client, packet);
                    var startTime = DateTime.Now.Ticks;

                    client.lastReceivedAttrib = null;

                    while (client.lastReceivedAttrib == null &&
                           ((DateTime.Now.Ticks - startTime) < timeOutTime))
                    {
                    }

                    if (!((DateTime.Now.Ticks - startTime) < timeOutTime))
                    {
                        Debug.LogError($"GetLobbyAttribute: Request timed out!.");
                    }

                    return client.lastReceivedAttrib;
                }
            }
        }
    }
}