using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script contains information about the interactable and the related ink node
 * */

public class Interactable : MonoBehaviour
{
    [Tooltip("The name of the Ink knot associated to this object")]
    public string knotName = "";

    [Tooltip("The text that appears when in range")]
    public string actionText = "Interact";

    [Tooltip("Set to true if you want the interactable to disable itself after the first interaction")]
    public bool onlyOnce = false;

    [Tooltip("Set to true if you want the dialog to start without pressing the interact key")]
    public bool onContact = false;

    private DialogueManager dialogueManager;


    void Start()
    {

        //check if there is a collider
        Collider[] cols = transform.GetComponentsInChildren<Collider>();

        //check if there is a collider
        Collider2D[] cols2D = transform.GetComponentsInChildren<Collider2D>();


        if (cols.Length == 0 && cols2D.Length == 0)
        {
            Debug.LogWarning("Warning: the interactable " + gameObject.name + " doesn't have any colliders attached");
        }

        if(onContact)
        {
            dialogueManager = GameObject.FindFirstObjectByType<DialogueManager>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if on contact check if it's the player to collide
        if (onContact && dialogueManager != null)
           if(collision.gameObject == dialogueManager.player.gameObject)
                {
                    dialogueManager.StartDialogue(knotName);
                }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //if on contact check if it's the player to collide
        if (onContact && dialogueManager != null)
            if (collider.gameObject == dialogueManager.player.gameObject)
            {
                dialogueManager.StartDialogue(knotName);
            }
    }



}
