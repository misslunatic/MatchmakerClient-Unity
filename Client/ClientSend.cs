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
using UnityEngine;
using Scripts.Matchmaker;
using DateTime = System.DateTime;

// ReSharper disable once CheckNamespace
namespace Scripts.CustomServer
{
    public static class ClientSend
    {
        

        public enum StatusType
        {
            Ok,
            Received,
            Fail
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public static float timeOutTime = 30000000;

        // ReSharper disable once MemberCanBePrivate.Global
        public static void SendTcpDataNoSync(Client client, Packet packet)
        {
            packet.WriteLength();
            client.tcp.SendData(packet);
        }

        // ReSharper disable once UnusedMember.Global
        public static void SendUdpDataNoSync(Client client, Packet packet)
        {
            packet.WriteLength();
            client.udp.SendData(packet);
        }

        public static bool SendTcpData(Client client, Packet packet)
        {
            if (client.IsConnected)
            {
                // Send the data
                packet.WriteLength();
                client.tcp.SendData(packet);
                if (Config.Sync)
                {
                    // Wait for a response from the server
                    var startTime = DateTime.Now.Ticks;

                    client.lastStatusType = null;

                    while (client.lastStatusType == null &&
                           ((DateTime.Now.Ticks - startTime) < timeOutTime))
                    {
                    }

                    // If timed out
                    if ((DateTime.Now.Ticks - startTime) < timeOutTime)
                    {
                        Debug.LogError($"SendTCPData: Request timed out!.");
                        return false;
                    }


                    return client.lastStatusType is StatusType.Ok or StatusType.Received;
                }

                return true;
            }
            else
            {
                Debug.LogWarning("Attempted to send TCP data through a disconnected Client!");
                return false;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static bool SendUdpData(Client client, Packet packet)
        {
            if (client.IsConnected)
            {
                // Send the data
                packet.WriteLength();
                client.udp.SendData(packet);
                if (Config.Sync)
                {
                    // Wait for a response from the server
                    var startTime = DateTime.Now.Ticks;

                    client.lastStatusType = null;

                    while (client.lastStatusType == null &&
                           ((DateTime.Now.Ticks - startTime) < timeOutTime))
                    {
                    }

                    // If timed out
                    if ((DateTime.Now.Ticks - startTime) < timeOutTime)
                    {
                        Debug.LogError($"SendTCPData: Request timed out!.");
                        return false;
                    }


                    return client.lastStatusType is StatusType.Ok or StatusType.Received;
                }

                return true;
            }
            // If status received
            else
            {
                Debug.LogWarning("Attempted to send UDP data through a disconnected Client!");
                return false;
            }
        }

        #region Built-in Packets

        public static void WelcomeReceived(Client client)
        {
            Debug.Log("Sending 'Welcome Received' message...");
            using var packet = new Packet(int.MaxValue);

            packet.Write(client.myId);
            packet.Write(Config.MatchmakerAPIVersion);
            packet.Write(Config.GameVersion);
            packet.Write(Config.GameId);
            
            SendTcpData(client, packet);
        }

        public static bool SetClientAttribute(Client client, String name, String value)
        {
            Debug.Log("Sending request to change a Client Attribute...");
            using var packet = new Packet(int.MaxValue - 1);
            packet.Write(client.myId);
            packet.Write(name);
            packet.Write(value);

            if (ClientSend.SendTcpData(client, packet))
            {
                return true;
            }

            return false;
        }

        public static ClientHandle.ClientAttrib GetClientAttribute(Client client, int id, String name)
        {
            Debug.Log("Sending request to be sent a Client Attribute...");
            using var packet = new Packet(int.MaxValue - 2);
            packet.Write(client.myId);
            packet.Write(id);
            packet.Write(name);

            ClientHandle.lastReceivedAttrib = null;
            ClientSend.SendTcpData(client, packet);

            var startTime = DateTime.Now.Ticks;

            while (ClientHandle.lastReceivedAttrib == null && ((DateTime.Now.Ticks - startTime) < timeOutTime))
            {
            }

            if (!((DateTime.Now.Ticks - startTime) < timeOutTime))
            {
                Debug.LogError("GetClientAttribute: Request timed out!");
            }

            return ClientHandle.lastReceivedAttrib;
        }

        /// <summary>
        /// Respond to the listClient with a status
        /// </summary>
        /// <param name="client">What Client is sending it</param>
        /// <param name="status">What status</param>
        public static void Status(Client client, StatusType status)
        {
            using var packet = new Packet(int.MaxValue - 3);
            packet.Write((int)status);

            SendTcpDataNoSync(client, packet);
        }

        #endregion
    }
}