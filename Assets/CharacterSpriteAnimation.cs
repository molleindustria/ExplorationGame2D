using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteAnimation : MonoBehaviour
{

    [Tooltip("Add the sprites here in sequence")]
    public Sprite[] idleSprites;

    [Tooltip("Add the sprites here in sequence")]
    public Sprite[] walkingSprites;

    [Tooltip("Frames per second")]
    public float FPS = 5;

    [Tooltip("mirrors on the x based on the movement")]
    public bool flipX = true;

    [Tooltip("The sprite renderer if not specified looks on this gameobject")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("The character rigidbody to detect the direction")]
    public Rigidbody2D rb;

    public int currentFrame = 0;

    private float timer = 0;

    private Sprite[] currentSprites;

    //saving the last non zero direction
    private Vector2 lastDirection = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogWarning("Warning: no sprite renderer on " + gameObject.name);

        if (rb == null)
            rb = transform.GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = transform.parent.GetComponent<Rigidbody2D>();

        if (rb == null)
            Debug.LogWarning("Warning I can't find rigidbody to infer the direction. Assign manually");

        currentSprites = walkingSprites;
    }




    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            lastDirection = rb.linearVelocity.normalized;
            currentSprites = walkingSprites;
        }
        else
        {
            currentSprites = idleSprites;
        }

        //assumes a sprite facing right 
        if (flipX) {

            if (lastDirection.x < 0)
                spriteRenderer.flipX = true;
            else if (lastDirection.x > 0)
                spriteRenderer.flipX = false;

        }


        //next frame
        if (timer > 1 / FPS)
        {
            timer = 0;
            currentFrame++;

            if (currentFrame >= currentSprites.Length)
                currentFrame = 0;

            if (currentSprites.Length > 0 && currentFrame <= currentSprites.Length - 1)
            {
                spriteRenderer.sprite = currentSprites[currentFrame];
            }

        }

    }
}
