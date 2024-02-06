using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * This script shoots a raycaster from the pointer to detect an interactable
 * and updates the dialogue manager. Use only as an alternative to a character based raycast
 */

public class RaycasterPointer : MonoBehaviour
{
    public DialogueManager dialogueManager;
    private EventSystem eventSystem;

    void Start()
    {
        if (dialogueManager == null)
            dialogueManager = GameObject.FindAnyObjectByType<DialogueManager>();

        if(eventSystem == null)
            eventSystem = GameObject.FindAnyObjectByType<EventSystem>();

    }

    //every frame send a raycast
    void Update()
    {
        dialogueManager.currentInteractable = null;

        //esoteric way to check is the pointer is not on UI
        if (!EventSystem.current.IsPointerOverGameObject())
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);


            if (hit.collider != null)
            {
                //Debug.Log("Pointer over " + hit.collider.gameObject.name);

                //see if the object has an interactable on it
                Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();

                if (interactable != null)
                {
                    dialogueManager.currentInteractable = interactable;

                }
            }

        }



    }
}
