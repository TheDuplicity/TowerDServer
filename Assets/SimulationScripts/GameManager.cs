using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    struct minionDefaultMessage
    {
        Vector2 position;

    }

    //list of players that want to join and their tower types
    Dictionary<int, int> clientsWaitingToJoinAsType;

    List<GameObject> minions;
    List<GameObject> towers;
    private GameObject tileSet;
     public GameObject minionPrefab;
     public GameObject towerPrefab;

    public float gameTime { get; private set; }

    public bool gameStarted;

    Vector3 PathStart;

    public int minionScore;
    public int towerScore;

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
        //
        //
        //
        //
        //
        gameTime = 0;
        minions = new List<GameObject>();
        towers = new List<GameObject>();
        tileSet = GameObject.Find("Tiles");

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




    }

    public void kickPlayer(int clientId)
    {
        for (int i = 0; i < minions.Count; i++)
        {
            if (minions[i].GetComponent<Controllable>().getId() == clientId)
            {
                Destroy(minions[i].gameObject);
                minions.RemoveAt(i);
                return;
            }
        }
        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i].GetComponent<Controllable>().getId() == clientId)
            {
                Destroy(towers[i].gameObject);
                towers.RemoveAt(i);
                return;
            }
        }

        //kick player with this id


    }
    private void getAllBasicClientData()
    {

    }
    public void sendWelcomePackage(int sendToId)
    {
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
        gameStarted = true;
        for (int i = 0; i < minions.Count; i++)
        {
            sendWelcomePackage(minions[i].GetComponent<Controllable>().getId());
        }
        for (int i = 0; i < towers.Count; i++)
        {
            sendWelcomePackage(towers[i].GetComponent<Controllable>().getId());
        }
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
