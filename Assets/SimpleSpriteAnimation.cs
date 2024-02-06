using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSpriteAnimation : MonoBehaviour
{
    [Tooltip("Add the sprites here in sequence")]
    public Sprite[] frames;

    [Tooltip("The sprite renderer if not specified looks on this gameobject")]
    public SpriteRenderer spriteRenderer;

    public float FPS = 5;
    public bool loop = true;
    public bool pingPong = false;
    private float timer = 0;
    public int currentFrame = 0;

    public bool ping = true;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogWarning("Warning: no sprite renderer on " + gameObject.name);

    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        //next frame
        if(timer> 1/FPS)
        {
            //playing forward
            if (!pingPong)
            {
                timer = 0;
                currentFrame++;

                if (loop && currentFrame >= frames.Length)
                    currentFrame = 0;

                if (frames.Length > 0 && currentFrame <= frames.Length-1)
                {
                    spriteRenderer.sprite = frames[currentFrame];
                }
            }
            else
            {
                //forward
                if (ping)
                {
                    timer = 0;
                    currentFrame++;

                    if (currentFrame >= frames.Length)
                    {
                        currentFrame = frames.Length-2;
                        ping = false;
                    }

                }
                else
                {
                    timer = 0;
                    currentFrame--;

                    if (loop && currentFrame <= 0)
                    {
                        ping = true;
                        currentFrame = 0;
                    }

                }

                
                if (frames.Length > 0 && currentFrame <= frames.Length - 1)
                {
                    currentFrame %= frames.Length;
                    spriteRenderer.sprite = frames[currentFrame];
                }
            }


        }
        
    }


}
