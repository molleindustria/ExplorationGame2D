using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSpriteAnimation : MonoBehaviour
{
    private Transform[] frames;

    public float FPS = 5;
    public bool loop = true;
    public bool pingPong = false;
    private float timer = 0;
    public int currentFrame = 0;

    public bool ping = true;

    private void Awake()
    {
        timer = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        List<Transform> result = new List<Transform>();

        GetComponentsInChildren<Transform>(true, result);


        if (result.Contains(transform))
            result.Remove(transform);

        frames = result.ToArray();

        HideFrames();
        if (frames.Length > 0)
            frames[0].gameObject.SetActive(true);
        else
            Debug.LogWarning("There are no frames in this animation, please add children to the object "+gameObject.name);
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
                    HideFrames();
                    frames[currentFrame].gameObject.SetActive(true);
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
                    HideFrames();
                    frames[currentFrame].gameObject.SetActive(true);
                }
            }


        }
        
    }

    public void HideFrames()
    {
        foreach (Transform o in frames)
            o.gameObject.SetActive(false);
    }

}
