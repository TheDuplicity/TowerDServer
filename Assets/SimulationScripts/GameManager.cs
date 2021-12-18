﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public struct minionDefaultMessage
    {
        public int clientId;
        public Vector2 position;
        public float time;
    }
    public struct towerDefaultMessage
    {
        public int clientId;
        public float zRotation;
        public float time;
    }
    //list of players that want to join and their tower types
    Dictionary<int, int> clientsWaitingToJoinAsType;

    List<GameObject> minions;
    List<GameObject> towers;
    public GameObject tileSet;
     public GameObject minionPrefab;
     public GameObject towerPrefab;

    public float gameTime { get; private set; }

    public bool gameStarted;

    Vector3 PathStart;

    public int minionScore;
    public int towerScore;

    private float sendPlayerUpdatesTimer;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(this);
        }

    }

    // Start is called before the first frame update
    void Start()
    {

        gameStarted = false;
        sendPlayerUpdatesTimer = 0;
        gameTime = 0;
        minions = new List<GameObject>();
        towers = new List<GameObject>();

        PathStart = tileSet.GetComponent<CustomTileMap>().startTiles[0].transform.position;

        minionScore = 0;
        towerScore = 0;

        

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            //if we have 2 players
            if (minions.Count + towers.Count >= 1) {
                startGame();
            }
            else
            {
                return;
            }
        }
        gameTime += Time.deltaTime;
        sendPlayerUpdatesTimer += Time.deltaTime;
        if (sendPlayerUpdatesTimer > 0.05)
        {
            sendPlayerUpdatesTimer = 0;
            sendDefaultUpdatesToEveryone();
        }


    }

    public void KillPlayerAndUpdateClients(int deadPlayerId)
    {
        //first send message to everyone saying the player died

        foreach (GameObject minion in minions)
        {
            ServerSend.PlayerDied(minion.GetComponent<Controllable>().getId(), deadPlayerId);
        }
        foreach (GameObject tower in towers)
        {
            ServerSend.PlayerDied(tower.GetComponent<Controllable>().getId(), deadPlayerId);
        }


        for (int i = 0; i < minions.Count; i++)
        {
            if (minions[i].GetComponent<Controllable>().getId() == deadPlayerId)
            {
                Destroy(minions[i]);
                minions.RemoveAt(i);
                return;
            }
            
        }

            for (int i = 0; i < towers.Count; i++)
            {
                if (towers[i].GetComponent<Controllable>().getId() == deadPlayerId)
                {
                    Destroy(towers[i]);
                    towers.RemoveAt(i);
                return;
                }

            }
        

    }

    private GameObject returnObjectWithThisClientId(int clientId)
    {
        GameObject returnObj = returnMinionWithThisClientId(clientId);
        if (returnObj == null)
        {
            returnObj = returnTowerWithThisClientId(clientId);
        }
        return returnObj;
    }

    public void shootBulletFromTower(int towerId)
    {
        returnTowerWithThisClientId(towerId).GetComponent<Tower>().Shoot();
    }
    public void SendTowerShotToAllPlayers(int shooterId)
    {
        foreach (GameObject player in minions)
        {
            ServerSend.TowerShot(player.GetComponent<Controllable>().getId(), shooterId);
        }
        foreach (GameObject player in towers)
        {
            ServerSend.TowerShot(player.GetComponent<Controllable>().getId(), shooterId);
        }
    }


    public void UpdateMinion(int clientId, minionDefaultMessage message)
    {
        GameObject minion = returnMinionWithThisClientId(clientId);
        minion.GetComponent<Minion>().AddMessage(message);
    }

    public void updateTower(int clientId, towerDefaultMessage message)
    {
        GameObject tower = returnTowerWithThisClientId(clientId);
        tower.GetComponent<Tower>().AddMessage(message);
    }

    private GameObject returnMinionWithThisClientId(int clientId)
    {
        foreach (GameObject minion in minions)
        {
            if (minion.GetComponent<Controllable>().getId() == clientId)
            {
                return minion;
            }
        }
        return null;
    }
    private GameObject returnTowerWithThisClientId(int clientId)
    {
        foreach (GameObject tower in towers)
        {
            if (tower.GetComponent<Controllable>().getId() == clientId)
            {
                return tower;
            }
        }
        return null;
    }

    public void sendDefaultUpdatesToEveryone()
    {
        minionDefaultMessage[] minionMessages = fillAllMinionMessages();
        towerDefaultMessage[] towerMessages = fillAllTowerMessages();

        for (int i = 0; i < minions.Count; i++)
        {
            ServerSend.SendWorldUpdate(minions[i].GetComponent<Controllable>().getId(), gameTime, minionScore, towerScore, minionMessages, towerMessages);
        }
        for (int i = 0; i < towers.Count; i++)
        {
            ServerSend.SendWorldUpdate(towers[i].GetComponent<Controllable>().getId(), gameTime, minionScore, towerScore, minionMessages, towerMessages);
        }

    }




    private towerDefaultMessage[] fillAllTowerMessages()
    {
        int numTowers = towers.Count;
        towerDefaultMessage[] messages = new towerDefaultMessage[numTowers];
        for (int i = 0; i < numTowers; i++)
        {
            GameObject tower = towers[i];
            messages[i].zRotation = tower.transform.rotation.eulerAngles.z;
            messages[i].clientId = tower.GetComponent<Controllable>().getId();

        }
        return messages;
    }

    private minionDefaultMessage[] fillAllMinionMessages()
    {
        int numMinions = minions.Count;
        minionDefaultMessage[] messages = new minionDefaultMessage[numMinions];
        for (int i = 0; i < numMinions; i++)
        {
            GameObject minion = minions[i];
            messages[i].position = new Vector2(minion.transform.position.x, minion.transform.position.y);
            messages[i].clientId = minion.GetComponent<Controllable>().getId();

        }
        return messages;
    }

    public void tellOtherPlayersIExist(int clientId)
    {
        GameObject newPlayerObject = returnMinionWithThisClientId(clientId);
        if (newPlayerObject == null)
        {
            newPlayerObject = returnTowerWithThisClientId(clientId);
        }

        Vector2 pos = new Vector2(newPlayerObject.transform.position.x, newPlayerObject.transform.position.y);
        int newPlayerType = newPlayerObject.GetComponent<Controllable>().type;
        float newPlayerZRot = newPlayerObject.transform.rotation.eulerAngles.z;
        int otherPlayerId = 0;
        for (int i = 0; i < minions.Count; i++)
        {
            otherPlayerId = minions[i].GetComponent<Controllable>().getId();


            //send a package to the oother players givine them this new object
            ServerSend.SendNewConnectedPlayerInit(otherPlayerId, pos, clientId, newPlayerType, newPlayerZRot);
        }
        for (int i = 0; i < towers.Count; i++)
        {
            otherPlayerId = towers[i].GetComponent<Controllable>().getId();

            ServerSend.SendNewConnectedPlayerInit(otherPlayerId, pos, clientId, newPlayerType, newPlayerZRot);
        }
    }

    public void sendWelcomePackage(int sendToId)
    {
        if (gameStarted)
        {
            tellOtherPlayersIExist(sendToId);
        }
        int numPlayers= minions.Count + towers.Count;
        Vector2[] positions = new Vector2[numPlayers];
        int[] ids = new int[numPlayers];
        int[] types = new int[numPlayers];
        float[] zRotations = new float[numPlayers];
        //num players in total but we skip

        for (int i = 0; i < minions.Count; i++)
        {
            
            positions[i] = new Vector2(minions[i].transform.position.x, minions[i].transform.position.y);
            ids[i] = minions[i].GetComponent<Controllable>().getId();
            types[i] = 1;
            zRotations[i] = 0;
            
        }
        for (int i = 0; i < towers.Count; i++)
        {
            int fillValArrayPos = i + minions.Count;
            positions[fillValArrayPos] = new Vector2(towers[i].transform.position.x, towers[i].transform.position.y);
            ids[fillValArrayPos] = towers[i].GetComponent<Controllable>().getId();
            types[fillValArrayPos] = 0;
            zRotations[fillValArrayPos] = 0;
        }

        ServerSend.JoinGameData(sendToId, gameTime, numPlayers, positions, ids, types, zRotations);
    }

    private void startGame()
    {
        for (int i = 0; i < minions.Count; i++)
        {
            sendWelcomePackage(minions[i].GetComponent<Controllable>().getId());
        }
        for (int i = 0; i < towers.Count; i++)
        {
            sendWelcomePackage(towers[i].GetComponent<Controllable>().getId());
        }
        gameStarted = true;
    }

    public bool addMinion(int clientId)
    {
        if (clientAlreadyHasObject(clientId))
        {
            return false;
        }
        GameObject newMinion = Instantiate(minionPrefab);
        newMinion.transform.position = PathStart;
        newMinion.GetComponent<Controllable>().setId(clientId);
        newMinion.GetComponent<Controllable>().type = 1;
        minions.Add(newMinion);
        return true;
    }
    public bool addTower(int clientId, Vector3 spawnPos)
    {
        if (clientAlreadyHasObject(clientId))
        {
            return false;
        }
        Debug.Log($"spawning tower for client {clientId} at position {spawnPos}");
        GameObject newTower = Instantiate(towerPrefab);
        newTower.GetComponent<Controllable>().setId(clientId);
        newTower.transform.position = spawnPos;
        newTower.GetComponent<Controllable>().type = 0;
        towers.Add(newTower);
        return true;
    }

    private bool clientAlreadyHasObject(int clientId)
    {

        if (minions.Count > 0)
        {
            foreach (GameObject minion in minions)
            {
                if (minion.GetComponent<Controllable>().getId() == clientId)
                {
                    return true;
                }
            }
        }
        if (towers.Count > 0)
        {
            foreach (GameObject tower in towers)
            {
                if (tower.GetComponent<Controllable>().getId() == clientId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool trySpawnClientAsTower(int clientId, Vector3 mouseCheckPos)
    {

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseCheckPos);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(mouseWorldPos, new Vector3(0, 0, 1), 100);
        if (hit)
        {
            GameObject hitObj = hit.transform.gameObject;
            if (hitObj.tag == "TowerTile")
            {
                if (!isTowerTileTaken(hitObj.transform.position))
                {
                    if(addTower(clientId, hitObj.transform.position))
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Log($"couldnt spawn client {clientId}, tile occupied");
                }
                Debug.Log($"couldnt spawn client {clientId}, not on a valid tile");


            }

        }
        return false;
    }
    private bool isTowerTileTaken(Vector3 pos)
    {
        if (towers.Count <= 0)
        {
            return false;
        }
        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i].gameObject.transform.position == pos)
            {
                return true;
            }
        }
        return false;
    }


}
