using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//This script detect a trigger enter and exit and plays an even specified in the inspector
public class TriggerInteraction2D : MonoBehaviour
{
    public GameObject player;

    [Serializable]
    public class MyEvent : UnityEvent { }

    public MyEvent EnterTrigger;
    public MyEvent ExitTrigger;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            //if not set assumes the template setup
            player = GameObject.Find("player");
        }

        if (player == null)
        {
            //if not set assumes the template setup
            player = GameObject.Find("Player");
        }

        if(player == null)
        {
            Debug.LogWarning(gameObject.name + " can't find the player to trigger the event, please set it manually");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("Enter trigger " + gameObject.name);

        if (other.gameObject == player)
        {
            EnterTrigger.Invoke();
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        print("Exit trigger " + gameObject.name);

        if (other.gameObject == player)
        {
            ExitTrigger.Invoke();
        }
    }

}
