using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ServerSend : MonoBehaviour
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendDataQueue(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].tcp.SendDataQueue(packet);
            }
        }

        private static void SendTCPDataToAllButOne(int avoidClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                if (i != avoidClient)
                {
                    Server.clients[i].tcp.SendDataQueue(packet);
                }
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

    public static void ServerAlive(int toClient, bool aliveOrDead)
    {
        using (Packet packet = new Packet((int)ServerPackets.serverAlive))
        {
            packet.Write(aliveOrDead);
            SendTCPData(toClient, packet);
        }
        
    }

    public static void ServerAliveImmediate(int toClient, bool aliveOrDead)
    {
        using (Packet packet = new Packet((int)ServerPackets.serverAlive))
        {
            packet.Write(aliveOrDead);
            packet.WriteLength();
            Server.clients[toClient].tcp.SendDataImmediate(packet);
        }

    }

    public static void JoinGameData(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.joinGameData))
        {
            packet.Write("data here");
            SendTCPData(toClient, packet);
        }
    }

}

