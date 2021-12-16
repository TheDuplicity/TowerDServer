using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    List<GameObject> minions;
    List<GameObject> towers;
    private GameObject tileSet;
     public GameObject minionPrefab;
     public GameObject towerPrefab;

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
            if (minions.Count + towers.Count > 1) {
                gameStarted = true;
                startGame();
            }
        }




    }

    public void sendWelcomePackage(int sendToId, int type)
    {
        int numOtherPlayers = minions.Count + towers.Count;
        Vector2[] positions = new Vector2[numOtherPlayers];
        int[] ids = new int[numOtherPlayers];
        int[] types = new int[numOtherPlayers];
        float[] zRotations = new float[numOtherPlayers];
        //num players in total but we skip
        for (int i = 0; i < minions.Count; i++)
        {
            if (ids[i] == sendToId)
            {
                //skip if the object we're looking at is us
                continue;
            }
            positions[i] = new Vector2(minions[i].transform.position.x, minions[i].transform.position.y);
            ids[i] = minions[i].GetComponent<Controllable>().getId();
            types[i] = 1;
            zRotations[i] = 0;
            
        }
        for (int i = 0; i < towers.Count; i++)
        {
            if (ids[i] == sendToId)
            {
                //skip if the object we're looking at is us
                continue;
            }
            positions[i] = new Vector2(towers[i].transform.position.x, towers[i].transform.position.y);
            ids[i] = towers[i].GetComponent<Controllable>().getId();
            types[i] = 1;
            zRotations[i] = 0;
        }

        ServerSend.JoinGameData(sendToId, type, numOtherPlayers, positions, ids, types, zRotations);
    }

    private void startGame()
    {
        for (int i = 0; i < minions.Count; i++)
        {
            sendWelcomePackage(minions[i].GetComponent<Controllable>().getId(), 1);
        }
        for (int i = 0; i < towers.Count; i++)
        {
            sendWelcomePackage(towers[i].GetComponent<Controllable>().getId(), 0);
        }
    }

    public GameObject addMinion(int clientId)
    {
        GameObject newMinion = Instantiate(minionPrefab);
        newMinion.transform.position = PathStart;
        newMinion.GetComponent<Controllable>().setId(clientId);
        newMinion.GetComponent<Controllable>().type = 1;
        minions.Add(newMinion);
        return newMinion;
    }
    public GameObject addTower(int clientId)
    {
        GameObject newTower = Instantiate(towerPrefab);
        newTower.GetComponent<Controllable>().setId(clientId);
        newTower.GetComponent<Controllable>().type = 0;
        towers.Add(newTower);
        return newTower;
    }

    public bool checkIfTowerSpawnPosValid(Vector3 mouseCheckPos)
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseCheckPos);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(mousePos, new Vector3(0, 0, 1), 100);
        if (hit) {
            GameObject hitObj = hit.transform.gameObject;
            if (hitObj.tag == "TowerTile")
            {
                return true;
            }

        }
        return false;
    }


}
