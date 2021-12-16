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
    // positions, euler rotations about z  ,ids, type(minion/tower), some time data?, number of other players
    public static void JoinGameData(int toClient, int playerType, int numPlayers ,Vector2[] positions, int[] ids, int[] type, float[] zRotations)
    {
        using (Packet packet = new Packet((int)ServerPackets.joinGameData))
        {
            //0 for tower 1 for minion
            packet.Write(playerType);
            packet.Write(numPlayers);
            for (int i = 0; i < numPlayers; i++)
            {
                packet.Write(type[i]);
                packet.Write(ids[i]);
                packet.Write(positions[i].x);
                packet.Write(positions[i].y);
                packet.Write(zRotations[i]);
            }

            SendTCPData(toClient, packet);
        }
    }

}

