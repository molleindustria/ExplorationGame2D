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

    [Tooltip("The minimum distance to interact with an interactable")]
    public float interactionDistance = 3f;

    [Tooltip("The offset of the ray from the position of the object")]
    public Vector2 rayOffset = Vector2.zero;

    //saving the last non zero direction
    private Vector2 lastDirection = Vector2.zero;

    private Rigidbody2D playerRigidbody;

    [Tooltip("If true force the raycast to left or right (side scroller)")]
    public bool twoDirections = false;

    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = GameObject.FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
            Debug.LogWarning("Warning: I can't find a dialogue manager in the scene");

        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody2D>();
        
        if(playerRigidbody == null)
            Debug.LogWarning("Warning: I can't find a Rigidbody2D on the player object");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        //if not moving keep the last direction
        if (movementInput.magnitude > 0.1f)
        {
            //infer the direction of the player from the rigid body velocity
            lastDirection = movementInput.normalized;

            if (twoDirections)
            {
                
                if (lastDirection.x > 0)
                    lastDirection = new Vector2(1, 0);
                else
                    lastDirection = new Vector2(-1, 0);
            }
        }

        Vector2 start = new Vector2(playerRigidbody.transform.position.x, playerRigidbody.transform.position.y) + rayOffset;
        
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
