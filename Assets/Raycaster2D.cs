using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Shoots raycasters around the player and sets the current Interactable if any
    interfaces specifically with DialogueManager and PlayerMovement2D
 * */

public class Raycaster2D : MonoBehaviour
{
    DialogueManager dialogueManager;
    PlayerMovement2D playerMovement;

    [Tooltip("The minimum distance to interact with an interactable")]
    public float interactionDistance = 3f;

    [Tooltip("The offset of the ray from the position of the object")]
    public Vector2 rayOffset = Vector2.zero;

    //saving the last non zero direction
    private Vector2 lastDirection = Vector2.zero;


    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = GameObject.FindObjectOfType<DialogueManager>();

        if (dialogueManager == null)
            Debug.LogWarning("Warning I can't find a dialogue manager in the scene");

        playerMovement = GameObject.FindObjectOfType<PlayerMovement2D>();

        if (playerMovement == null)
            Debug.LogWarning("Warning I can't find a PlayerMovement2D in the scene");

    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement.movementInput.magnitude > 0.1f)
            lastDirection = playerMovement.movementInput;

        Vector2 start = new Vector2(playerMovement.transform.position.x, playerMovement.transform.position.y) + rayOffset;
        
        // Cast a ray in the direction of the input
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, lastDirection, interactionDistance);
        
        Debug.DrawLine(start, start + lastDirection * interactionDistance,Color.yellow);

        dialogueManager.currentInteractable = null;

        foreach(RaycastHit2D hit in hits) {
            // If it hits something...
            if (hit.collider != null)
            {
                //if hit something see if there is an interactable
                Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();

                //check if the interactable is enabled
                if (interactable != null && interactable.enabled)
                {
                    //if so we hit a valid interactable
                    dialogueManager.currentInteractable = interactable;
                }
            }
        }


    }
}
