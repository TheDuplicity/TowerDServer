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
            Debug.Log($"client {fromClient} alive");

        }
    }

    public static void ChosePlayerType(int fromClient, Packet packet)
    {
        Debug.Log("received player type: sending back");
        int type = packet.ReadInt();
        ServerSend.JoinGameData(fromClient);
        switch (type)
        {
            //tower
            case 0:

                break;
                //minion
            case 1:

                break;
            default:
                break;
        }
    }

}
