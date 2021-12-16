using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    float serverAliveTimer;
    float stopconnectionstimer;
    [SerializeField] private int maxPlayers;
    bool wantdisconnect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("instance already exists");
            Destroy(this);
        }
    }

    private void Start()
    {
       // QualitySettings.vSyncCount = 0;
      //  Application.targetFrameRate = 30;
        Server.Start(maxPlayers, 5000);
        serverAliveTimer = 0;
       stopconnectionstimer = 0;
        wantdisconnect = false;
    }


    private void FixedUpdate()
    {
        serverAliveTimer += Time.fixedDeltaTime;
        stopconnectionstimer += Time.fixedDeltaTime;
        
        if (serverAliveTimer > 2.0f)
        {
            serverAliveTimer = 0.0f;
            Server.pingAliveToClients();
        }
        if (stopconnectionstimer > 7.0f && wantdisconnect)
        {
            wantdisconnect = false;
            Server.disconnectAllClients();
        }
        Server.updateClientTimers();
        Server.handleServerReadWrites();
    }

}
