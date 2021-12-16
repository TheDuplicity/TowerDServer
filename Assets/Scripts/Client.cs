using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;


    public class Client
    {
    public float connectedTimer;
        public static int dataBufferSize = 4096;
        public int id;
        public TCP tcp;


        public Client(int setId)
        {
            id = setId;
            tcp = new TCP(setId);

        }



    public void Connect(TcpClient setSocket)
    {
        connectedTimer = 0;

        tcp = new TCP(id);
        tcp.Connect(setSocket);
    }

    public void Close()
    {
        connectedTimer = 0;
        tcp.closeSocket();
        
    }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;
            private Queue<Packet> writePackets;
        public bool active;
        private readonly int id;

            public TCP(int setId)
            {
                id = setId;
                socket = null;
            }

        public void closeSocket()
        {
            if (active) {
                active = false;
                Debug.Log($"received message to close socket {id}.");
                ServerSend.ServerAliveImmediate(id, false);
                socket.GetStream().Close();
                socket.Close();
                socket = null;
            }
        }

            public void Connect(TcpClient setSocket)
            {

            active = true;
                socket = setSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                 writePackets = new Queue<Packet>();

                stream = socket.GetStream();

            receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                //stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }
            public void SendDataImmediate(Packet packet)
            {
                if (active)
                {

                    stream.WriteAsync(packet.ToArray(), 0, packet.Length());

                }
            }
            public void SendDataQueue(Packet packet)
            {
                

                if (active)
                {

                writePackets.Enqueue(new Packet(packet.ReadBytes(packet.Length(), false)));
                
                if (packet == writePackets.Peek())
                {
                    Debug.Log("packet is the same");
                }
                //stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }

            }


            public void readData()
             {
            if (!active)
            {
                Debug.Log("Didnt read data because connection is not active.");
                return;
            }
            try
            {
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } 
            catch(Exception e){
                Debug.Log($"error reading socket {socket}: {e}");
            }

            }

        public void writeData()
        {
            while (writePackets.Count > 0)
            {
                if (active)
                {
                    Packet packet = writePackets.Dequeue();

                    //stream.BeginWrite(packet.ToArray(),0, packet.Length(), writeCallBack, socket);
                    stream.WriteAsync(packet.ToArray(), 0, packet.Length());
                    break;
                }
            }

        }

       

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {

                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                    // disconnect
                    Debug.Log($"Reading returned no bytes. Disconnecting client {id}");
                    closeSocket();
                    return;
                    }
                    byte[] data = new byte[byteLength];

                    Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                    //stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error receiving TCP data: {ex}");
                    
                closeSocket();
                 
                //disconnect client
                }
            }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {

                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });

                packetLength = 0;

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;

        
        

         }


}

    }
