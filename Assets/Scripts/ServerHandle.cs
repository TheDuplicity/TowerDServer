using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ServerHandle : MonoBehaviour
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        int clientId = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint}Received message from client: {clientId}, with username: {username}.");
        if (fromClient != clientId) {
            Debug.Log($"player\"{username}\" (ID: {fromClient}) has assumed the wrong client ID({clientId})");
        }
        //send player into game
    }

    public static void ClientAlive(int fromClient, Packet packet) {
        bool alive = packet.ReadBool();
        if (!alive)
        {
            //maybe start a timer instead and at the end of the timer, close the connection if they havent come back
            //need to have a unique id that is new for each client that joins to tell them apart from just their id's in the collection
            //disconnect client
            Debug.Log($"client {fromClient} closed");
            //orrrr have a timer that only gets reset once it gets an alive response
            Server.clients[fromClient].Close();

            // so like Server.clients[fromClient].resetAliveTimer() if
        }
        else
        {
            //have a timer that only gets reset once it gets an alive response
            Server.clients[fromClient].connectedTimer = 0;
            //Debug.Log($"client {fromClient} alive");
        }
    }

    public static void AttemptMinionCreation(int fromClient, Packet packet)
    {


        if (GameManager.Instance.addMinion(fromClient))
        {
            bool inGame = packet.ReadBool();
            if (!inGame)
            {
                if (GameManager.Instance.gameStarted)
                {
                    GameManager.Instance.sendWelcomePackage(fromClient);
                }
            }
            else
            {
                GameManager.Instance.tellOtherPlayersIExist(fromClient);
            }
        }
        

    }


    public static void AttemptTowerCreation(int fromClient, Packet packet)
    {
        Vector3 towerMousePos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
        //if you were spawned and the game has started: send welcome package
        if(GameManager.Instance.trySpawnClientAsTower(fromClient, towerMousePos))
        {
            bool inGame = packet.ReadBool();
            if (!inGame) {
                if (GameManager.Instance.gameStarted)
                {
                    GameManager.Instance.sendWelcomePackage(fromClient);
                }

            }
            else
            {
                GameManager.Instance.tellOtherPlayersIExist(fromClient);
            }
        }
    }
    public static void TimePing(int fromClient, Packet packet)
    {
        //send a ping back immediately which will stop the timer on the other side to get round trip time
        int timerId = packet.ReadInt();
        ServerSend.TimePing(fromClient, timerId);
    }

    public static void TowerUpdate(int fromClient, Packet packet)
    {
        GameManager.towerDefaultMessage message;
        message.time = packet.ReadFloat();
        message.zRotation = packet.ReadFloat();
        message.clientId = fromClient;
        GameManager.Instance.updateTower(fromClient, message);
    }

    public static void MinionUpdate(int fromClient, Packet packet)
    {
        GameManager.minionDefaultMessage message;
        message.time = packet.ReadFloat();
        message.position = new Vector2(packet.ReadFloat(), packet.ReadFloat());
        message.clientId = fromClient;
        GameManager.Instance.UpdateMinion(fromClient, message);
    }

    public static void ShotBullet(int fromClient, Packet packet)
    {
        GameManager.Instance.shootBulletFromTower(fromClient);
    }


}
