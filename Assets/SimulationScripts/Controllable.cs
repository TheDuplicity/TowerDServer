using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : MonoBehaviour
{
    public bool readyToGetWelcomePackage;
    private float maxMessages;
    private int id;
    public int type;
    public struct message
    {
        int id;
        Vector2 position;
        float rotation;
    }

    Queue<message> messages;
    // Start is called before the first frame update
    void Start()
    {

        maxMessages = 3;
        messages = new Queue<message>();
    }

    private void Update()
    {

        if (readyToGetWelcomePackage)
        {
            if (GameManager.Instance.gameStarted)
            {
                GameManager.Instance.sendWelcomePackage(id, type);
            }

        }

        while (messages.Count > maxMessages)
        {
            messages.Dequeue();
        }
    }

    public void setId(int setId)
    {
        id = setId;
    }

    public int getId()
    {
        return id;
    }




}
