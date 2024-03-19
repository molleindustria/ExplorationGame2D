using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Quits the player when the user hits escape

public class RestartQuit : MonoBehaviour
{
    public KeyCode quitKey = KeyCode.Escape;
    public KeyCode restartKey = KeyCode.R;



    void Update()
    {
        if (Input.GetKeyDown(quitKey))
        {
            Application.Quit();
        }

        //you must add using UnityEngine.SceneManagement; at the top
        //restart the current scene whatever scene is, you can also specify the name
        if (Input.GetKeyDown(restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


    }
}