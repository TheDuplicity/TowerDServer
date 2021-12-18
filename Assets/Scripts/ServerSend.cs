using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ServerSend : MonoBehaviour
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
        if (Server.clients[toClient].tcp.active)
        {
            Server.clients[toClient].tcp.SendDataQueue(packet);
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
    public static void JoinGameData(int toClient, float gameTime, int numPlayers ,Vector2[] positions, int[] ids, int[] type, float[] zRotations)
    {

        using (Packet packet = new Packet((int)ServerPackets.joinGameData))
        {
            //0 for tower 1 for minion
            packet.Write(gameTime);
            packet.Write(numPlayers);
            for (int i = 0; i < numPlayers; i++)
            {
                packet.Write(type[i]);
                packet.Write(ids[i]);
                packet.Write(positions[i].x);
                packet.Write(positions[i].y);
                packet.Write(zRotations[i]);
                if (ids[i] == toClient)
                {
                    string towermin  ="";
                    if (type[i] == 0)
                    {
                        towermin = "tower";

                    } else if(type[i] == 1){
                        towermin = "minion";
                    }
                    Debug.Log($"send start game to client {toClient}, they will be type {towermin} and spawn at x,y: {positions[i].x}, {positions[i].y}");
                }
            }

            SendTCPData(toClient, packet);
        }
    }
    public static void SendWorldUpdate(int toClient, float gameTime, int minionScore, int towerScore, GameManager.minionDefaultMessage[] minionMessages, GameManager.towerDefaultMessage[] towerMessages)
    {

        using (Packet packet = new Packet((int)ServerPackets.sendWorldUpdate))
        {

            packet.Write(gameTime);
            packet.Write(minionScore);
            packet.Write(towerScore);
            packet.Write(minionMessages.Length);
            for (int i = 0; i < minionMessages.Length; i++)
            {
                packet.Write(minionMessages[i].clientId);
                packet.Write(minionMessages[i].position.x);
                packet.Write(minionMessages[i].position.y);
            }
            packet.Write(towerMessages.Length);
            for (int i = 0; i < towerMessages.Length; i++)
            {
                packet.Write(towerMessages[i].clientId);
                packet.Write(towerMessages[i].zRotation);
            }
            SendTCPData(toClient, packet);
        }
    }
    public static void TimePing(int toClient, int timerId)
    {

        using (Packet packet = new Packet((int)ServerPackets.timePing))
        {
            packet.Write(timerId);
            SendTCPData(toClient, packet);
        }
    }

    public static void SendNewConnectedPlayerInit(int toClient, Vector2 position, int id, int type, float zRotations)
    {
        using (Packet packet = new Packet((int)ServerPackets.newPlayerJoined))
        {

            packet.Write(id);
            packet.Write(type);
            packet.Write(zRotations);         
            packet.Write(position.x);
            packet.Write(position.y);

            SendTCPData(toClient, packet);
        }

                
    }

    public static void TowerShot(int toClient, int towerShotId)
    {

        using (Packet packet = new Packet((int)ServerPackets.towerShot))
        {
            packet.Write(towerShotId);
            SendTCPData(toClient, packet);
        }
    }

    public static void PlayerDied(int toClient, int deadPlayerId)
    {

        using (Packet packet = new Packet((int)ServerPackets.playerDied))
        {
            //send data
            packet.Write(deadPlayerId);
            SendTCPData(toClient, packet);
        }
    }

}

