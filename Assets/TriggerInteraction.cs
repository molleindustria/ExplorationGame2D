using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//This script detect a trigger enter and exit and plays an even specified in the inspector
public class TriggerInteraction : MonoBehaviour
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

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            EnterTrigger.Invoke();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            ExitTrigger.Invoke();
        }
    }

}
