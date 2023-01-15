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
using System.Net;
using System.Net.Sockets;
using Scripts.Matchmaker;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Scripts.CustomServer
{
    /// <summary>
    /// Stores information about the connection.
    /// </summary>
    public class Client : MonoBehaviour
    {
        public ClientSend.StatusType? lastStatusType;
        public int lastReceivedInt;
        public ClientHandle.ClientAttrib lastReceivedAttrib;

        public bool welcomeReceived;
        private const int DataBufferSize = 4096;

        
        /// <summary>
        /// For editor readability.
        /// </summary>
        public string displayName;

        public string ip = Config.Address;
        public int port = 26950;
        public int myId;
        public Tcp tcp;
        public UDP udp;
        private bool isConnected;

        /// <summary>
        /// Checks if it is connected to a server
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (tcp.socket is { Client: { Connected: true } })
                    {
                        /* pear to the documentation on Poll:
                         * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                         * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                         * -or- true if data is available for reading; 
                         * -or- true if the connection has been closed, reset, or terminated; 
                         * otherwise, returns false
                         */

                        // Detect if listClient disconnected
                        if (tcp.socket.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcp.socket.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public void OnDestroy()
        {
            Disconnect();
        }


        public class ClientPackets
        {
            public delegate void PacketHandler(Client server, Packet packet);

            // ReSharper disable once FieldCanBeMadeReadOnly.Global
            public Dictionary<int, PacketHandler> packetHandlers;

            public ClientPackets(Dictionary<int, PacketHandler> packetHandlers)
            {
                this.packetHandlers = packetHandlers;
                packetHandlers.Add(int.MaxValue, ClientHandle.Welcome);
                packetHandlers.Add(int.MaxValue - 1, ClientHandle.ServerDisconnect);
                packetHandlers.Add(int.MaxValue - 2, ClientHandle.GetClientAttributesReceived);
                packetHandlers.Add(int.MaxValue - 3, ClientHandle.StatusReceived);
            }
        }

        private ClientPackets packets = new(new Dictionary<int, ClientPackets.PacketHandler>());

        private void OnApplicationQuit()
        {
            Disconnect(); // Disconnect when the game is closed
        }

        /// <summary>Attempts to connect to the server.</summary>
        public bool ConnectToServer(ClientPackets packetList)
        {
            welcomeReceived = false;
            Debug.Log($"Attempting connection to server with ip={ip} port={port}");
            this.packets = packetList;
            tcp = new Tcp(this);
            udp = new UDP(this);

            packetList.packetHandlers.TryAdd(int.MaxValue, ClientHandle.Welcome);
            packetList.packetHandlers.TryAdd(int.MaxValue - 1, ClientHandle.ServerDisconnect);
            packetList.packetHandlers.TryAdd(int.MaxValue - 2, ClientHandle.GetClientAttributesReceived);
            packetList.packetHandlers.TryAdd(int.MaxValue - 3, ClientHandle.StatusReceived);

            InitializeClientData();

            isConnected = true;
            tcp.Connect(); // MatchmakerNode_Connect tcp, udp gets connected once tcp is done

            // Wait for a response from the server
            var startTime = DateTime.Now.Ticks;

            while (!welcomeReceived &&
                   ((DateTime.Now.Ticks - startTime) < ClientSend.timeOutTime))
            {
            }

            // If timed out
            if ((DateTime.Now.Ticks - startTime) >= ClientSend.timeOutTime)
            {
                Debug.LogError($"ConnectToServer: Request timed out!.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Subclass to handle TCP networking
        /// </summary>
        public class Tcp
        {
            public TcpClient socket;
            private readonly Client instance;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public Tcp(Client inst)
            {
                instance = inst;
            }

            /// <summary>Attempts to connect to the server via TCP.</summary>
            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = DataBufferSize,
                    SendBufferSize = DataBufferSize
                };

                receiveBuffer = new byte[DataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }

            /// <summary>Initializes the newly connected listClient's TCP-related info.</summary>
            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }

            /// <summary>Sends data to the listClient via TCP.</summary>
            /// <param name="packet">The packet to send.</param>
            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); // Send data to server
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data to server via TCP: {ex}");
                }
            }

            /// <summary>Reads incoming data from the stream.</summary>
            private void ReceiveCallback(IAsyncResult result)
            {
                Debug.Log("Received TCP data. Preparing to handle...");
                try
                {
                    if (instance.IsConnected)
                    {
                        int byteLength = stream.EndRead(result);
                        if (byteLength <= 0)
                        {
                            instance.Disconnect();
                            return;
                        }

                        byte[] data = new byte[byteLength];
                        Array.Copy(receiveBuffer, data, byteLength);

                        receivedData.Reset(HandleData(data)); // Reset receivedData if all data was handled
                        stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in TCP Receive Callback: {ex}");
                    Disconnect();
                }
            }

            /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
            /// <param name="data">The received data.</param>
            private bool HandleData(byte[] data)
            {
                Debug.Log("Now handling data (TCP)...");
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    // If listClient's received data contains a packet
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    if (socket != null)
                    {
                        // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                        var packetBytes = receivedData.ReadBytes(packetLength);
                //        ThreadManager.Start();
                //        ThreadManager.ExecuteOnMainThread(() =>
                 //       {
                        using var packet = new Packet(packetBytes);
                        int packetId = packet.ReadInt();
                        Debug.Log($"Received TCP Packet number {packetId}");
                        instance.packets.packetHandlers
                            [packetId](instance, packet); // Call appropriate method to handle the packet
                    //    });
                    }

                    packetLength = 0; // Reset packet length
                    if (receivedData.UnreadLength() >= 4)
                    {
                        // If listClient's received data contains another packet
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            // If packet contains no data
                            return true; // Reset receivedData instance to allow it to be reused
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true; // Reset receivedData instance to allow it to be reused
                }

                return false;
            }

            /// <summary>Disconnects from the server and cleans up the TCP connection.</summary>
            private void Disconnect()
            {
                instance.Disconnect();

                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        /// <summary>
        /// Subclass to handle UDP networking
        /// </summary>
        public class UDP
        {
            public UdpClient socket;
            private IPEndPoint endPoint;
            private readonly Client instance;

            public UDP(Client inst)
            {
                instance = inst;
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            /// <summary>Attempts to connect to the server via UDP.</summary>
            /// <param name="localPort">The port number to bind the UDP socket to.</param>
            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using Packet packet = new Packet();
                SendData(packet);
            }

            /// <summary>Sends data to the listClient via UDP.</summary>
            /// <param name="packet">The packet to send.</param>
            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(instance.myId); // Insert the listClient's ID at the start of the packet
                    if (socket != null)
                    {
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data to server via UDP: {ex}");
                }
            }

            /// <summary>Receives incoming UDP data.</summary>
            private void ReceiveCallback(IAsyncResult result)
            {
                Debug.Log("Received UDP data. Preparing to handle...");
                try
                {
                    if (instance.IsConnected)
                    {
                        byte[] data = socket.EndReceive(result, ref endPoint);
                        socket.BeginReceive(ReceiveCallback, null);

                        if (data.Length < 4)
                        {
                            instance.Disconnect();
                            return;
                        }

                        HandleData(data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in UDP Receive Callback: {ex}");
                    Disconnect();
                }
            }

            /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
            /// <param name="data">The received data.</param>
            private void HandleData(byte[] data)
            {
                Debug.Log("Now handling data (UDP)...");
                using var packet = new Packet(data);

                var packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);


                if (packet == null) throw new ArgumentNullException(nameof(packet));

              //  ThreadManager.Start();
              //  ThreadManager.ExecuteOnMainThread(() =>
              //    {
                using var packet2 = new Packet(data);
                var packetId = packet2.ReadInt();
                try
                {
                    instance.packets.packetHandlers
                        [packetId](instance, packet2); // Call appropriate method to handle the packet
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error in UDP packet handler {packetId}: {e}");
                    throw;
                }

                //   });
            }

            /// <summary>Disconnects from the server and cleans up the UDP connection.</summary>
            private void Disconnect()
            {
                instance.Disconnect();

                endPoint = null;
                socket = null;
            }
        }

        /// <summary>Initializes all necessary listClient data.</summary>
        private void InitializeClientData()
        {
        }

        /// <summary>Disconnects from the server and stops all network traffic.</summary>
        public void Disconnect()
        {
            if (!isConnected) return;
            isConnected = false;
            tcp.socket.Close();

            udp?.socket?.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}