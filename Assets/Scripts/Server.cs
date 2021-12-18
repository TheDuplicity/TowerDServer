using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

    public class Server : MonoBehaviour
    {

        public static int maxPlayers { get; private set; }


        public static int port { get; private set; }

        private static TcpListener tcpListener;

        public static Dictionary<int, Client> clients;

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static Dictionary<int, PacketHandler> packetHandlers;

    // Start is called before the first frame update



        public static void Start(int maxNumPlayers, int setPort)
        {

            maxPlayers= maxNumPlayers;

            clients = new Dictionary<int, Client>();


            port = setPort;

            initialiseServerData();

            Debug.Log("starting server");

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Debug.Log($"Server started on port: {port}.");

        }

    public static void handleServerReadWrites()
    {

        //find out if we want to read/write

        Dictionary<Socket, int> socketArrLocations = new Dictionary<Socket, int>();

        IList readables = new ArrayList();
        IList writeables = new ArrayList();

        int activeSockets = 0;

        for (int i = 1; i <= maxPlayers; i++)
        {
            //maybe add the socket references in an array of the size of the number of clients that are connected 

            if (clients[i].tcp.active)
            {
                readables.Add(clients[i].tcp.socket.Client);
                writeables.Add(clients[i].tcp.socket.Client);
                socketArrLocations.Add(clients[i].tcp.socket.Client, i);
                activeSockets++;
            }
        }

        if (readables.Count > 0 || writeables.Count > 0)
        {
            Socket.Select(readables, writeables, null, 100);
        }

        for (int i = 0; i < readables.Count; i++)
        {
            Socket socket = (Socket)readables[i];
            int clientPos = socketArrLocations[socket];
            clients[clientPos].tcp.readData();
        }
        for (int i = 0; i < writeables.Count; i++)
        {
            Socket socket = (Socket)writeables[i];
            int clientPos = socketArrLocations[socket];
            clients[clientPos].tcp.writeData();
        }

    }

    public static void disconnectAllClients()
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            //maybe add the socket references in an array of the size of the number of clients that are connected 

            if (clients[i].tcp.active)
            {
                Debug.Log($"mass closing: client {i} closed");
                clients[i].Close();
            }
        }
    }
    public static int getNumPlayers()
    {
        int numPlayers = 0;
        for (int i = 1; i <= maxPlayers; i++)
        {
            //maybe add the socket references in an array of the size of the number of clients that are connected 

            if (clients[i].tcp.active)
            {
                numPlayers++;
            }
        }
        return numPlayers;
    }
    public static void updateClientTimers()
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            //maybe add the socket references in an array of the size of the number of clients that are connected 

            if (clients[i].tcp.active)
            {

                clients[i].connectedTimer += Time.deltaTime;
                if (clients[i].connectedTimer > 10)
                {
                    clients[i].connectedTimer = 0;
                    clients[i].Close();
                }
            }
        }
    }

    public static void pingAliveToClients()
    {
        for (int i = 1; i < maxPlayers; i++) {
            if (clients[i].tcp.active)
            {
                ServerSend.ServerAlive(i, true);
            }
        }
    }

        private static void TCPConnectCallback(IAsyncResult result)
        {

            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Debug.Log($"incoming connection from: {client.Client.RemoteEndPoint}");

            for (int i = 1; i <= maxPlayers; i++)
            {
            if (!clients[i].tcp.active)
            {
                    clients[i].Connect(client);
                    Debug.Log($"{client.Client.RemoteEndPoint} connected.");
                    return;
                }
            }

            Debug.Log($"{client.Client.RemoteEndPoint} Connection denied: no valid connections.");
        }

        private static void initialiseServerData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                { (int)ClientPackets.clientAlive, ServerHandle.ClientAlive },
                { (int)ClientPackets.attemptMinionCreation, ServerHandle.AttemptMinionCreation},
                { (int)ClientPackets.attemptTowerCreation, ServerHandle.AttemptTowerCreation},
                { (int)ClientPackets.timePing, ServerHandle.TimePing}

            };

            Debug.Log("initialised packets");
        }


}

