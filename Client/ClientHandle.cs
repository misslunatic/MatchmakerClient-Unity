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
using System.Net;
using Scripts.Matchmaker;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Scripts.CustomServer
{
    public static class ClientHandle
    {
        public class ClientAttrib
        {
            // ReSharper disable once MemberCanBePrivate.Global
            public int id;
            public readonly string name;
            public readonly string value;

            public ClientAttrib(int id, string name, string value)
            {
                this.id = id;
                this.name = name;
                this.value = value;
            }
        }

        public static ClientAttrib lastReceivedAttrib;

        public static void GetClientAttributesReceived(Client client, Packet packet)
        {
            var requestedId = packet.ReadInt();
            var name = packet.ReadString();
            var value = packet.ReadString();

            Debug.Log("Received object attributes (Obj. ID: " + requestedId.ToString() + "): '" + name + "', '" +
                      value + "'.");

            lastReceivedAttrib = new ClientAttrib(requestedId, name, value);
        }

        public static void Welcome(Client client, Packet packet)
        {
            var msg = packet.ReadString();
            var myId = packet.ReadInt();
            
            var serverAPIVersion = packet.ReadString();
            var serverId = packet.ReadString();
            var gameVersion = packet.ReadString();
            
            // Version check
            if (serverAPIVersion != Config.MatchmakerAPIVersion)
            {
                Debug.LogWarning(
                    $"Client API version mismatch! Server API version is {serverAPIVersion} while Client API version is {Config.MatchmakerAPIVersion}.");
                client.Disconnect();
                return;
            }

            if (serverId != Config.GameId)
            {
                Debug.LogWarning($"Client-Server Game ID Mismatch! Server ID is {serverId} while Client Game ID is {Config.GameId}.");
                client.Disconnect();
                return;
            }

            // Version check
            if (gameVersion != Config.GameVersion)
            {
                Debug.LogWarning(
                    $"Client game version mismatch! Server game version is {gameVersion} while Client game version is {Config.GameVersion}.");
                client.Disconnect();
                return;
            }
            
            client.welcomeReceived = true;

            Debug.Log($"Message from server: {msg}");
            client.myId = myId;
            ClientSend.WelcomeReceived(client);

            client.udp.Connect(((IPEndPoint)client.tcp.socket.Client.LocalEndPoint).Port);
            client.welcomeReceived = true;
            //MatchmakerBehaviour.APIClientPackets.RequestAllLobbyIds(listClient);
        }

        public static void ServerDisconnect(Client client, Packet packet)
        {
            client.Disconnect();
        }

        public static void StatusReceived(Client client, Packet packet)
        {
            var type = packet.ReadInt();
            Debug.Log($"Status received: {(ClientSend.StatusType)type}");
            client.lastStatusType = (ClientSend.StatusType)type;
        }
    }
}