using UnityEngine;
using UnityEngine.UI;
using System;
using Ink.Runtime;
using TMPro;
using System.Collections;
using RedBlueGames.Tools.TextTyper;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/*
 This script works with Interactable.cs and integrates ink, 
 text mesh pro UI, a character avatar and a script for text effects
 the current interactable needs to be set up externally depending on the control logic eg by Raycaster.cs

    ink+unity info here
    https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md#getting-started-with-the-runtime-api
    
    text typer info here
    https://github.com/redbluegames/unity-text-typer

 * */


public class DialogueManager : MonoBehaviour
{
    public static event Action<Story> OnCreateStory;

    [Tooltip("If you want to start the game by displaying text add the passage here")]
    public string introKnot = "";

    [Tooltip("The player object")]
    public GameObject player;


    [Tooltip("The dialogue panel")]
    public GameObject panel;

    [Tooltip("The text field for the interaction")]
    public TMP_Text actionLabel;

    [Tooltip("The text field for the narration")]
    public TMP_Text line;

    [Tooltip("A button prefab for the choices")]
    [SerializeField]
    private Button buttonPrefab = null;

    [Tooltip("Remove the TextTyper script on the textfield if you don't want any effects")]
    public TextTyper typer; //the typewriter effect script
    public Transform choicesContainer;

    public bool dialogueOn = false;

    //i keep track of the state of the dialogue with an int variable
    private int dialogueState = 0;

    //the state variable can have different values that I make human-readable with constants 
    private int DIALOGUE_OFF = 0; //no dialogue
    private int WAIT_CONFIRM = 1; //line has been typed wait to advance
    private int WAIT_CHOICE = 2; //the next step will be to display the choices without advancing the story
    private int CHOICE_DISPLAYED = 3; //choices are displayed wait for player choice

    //when busy typing
    private bool typing = false;

    public float typingDelay = 0.1f;

    private bool skipFrame = false;

    public Interactable currentInteractable;

    [Tooltip("Source for the typing sound")]
    public AudioSource typingAudioSource;

    [Tooltip("The sound(s) produced when typing")]
    public AudioClip[] typeSound;


    [SerializeField]
    private TextAsset inkJSONAsset = null;
    public Story story;

    //wait x seconds after accepting choices to prevent a skip from becoming an accidental selection
    private float confirmTimeGrace = 0.5f;
    private float confirmTimer = 0;

    void Awake()
    {
        if (player == null)
        {
            print("Warning: the player object is not assigned");
        }

        LoadDialogues();
        actionLabel.text = "";


        //if null falls back to plain visualization
        typer = line.gameObject.GetComponent<TextTyper>();

        if (typer != null)
        {
            //listeners are functions called on each character and at the end of the typing
            //the actual functions are down below
            typer.PrintCompleted.AddListener(OnDoneTyping);
            typer.CharacterPrinted.AddListener(OnCharacterPrinted);
        }

        panel.SetActive(false);
        choicesContainer.gameObject.SetActive(false);
    }



    // Creates a new Story object with the compiled story which we can then play!
    void LoadDialogues()
    {
        story = new Story(inkJSONAsset.text);
        if (OnCreateStory != null) OnCreateStory(story);

        /*
        bind the function declared in the ink file to a function here
        You can have as many functions as you want with different parameters
         */

        story.BindExternalFunction("teleport", (string objectName) => { Teleport(objectName); });

        story.BindExternalFunction("activate", (string objectName) => { Activate(objectName); });

        story.BindExternalFunction("deactivate", (string objectName) => { Deactivate(objectName); });

        story.BindExternalFunction("gameEvent", (string name) => { GameEvent(name); });

        story.BindExternalFunction("callFunction", (string name, string parameter) => { CallFunction(name, parameter); });

        story.BindExternalFunction("playSound", (string fileName) => { PlaySound(fileName); });

    }


    private void Start()
    {
        //if set launch the starting dialogue
        if (introKnot != "")
            StartDialogue(introKnot);
    }



    private void Update()
    {
        //make sure the event system keyboard focus isn't lost if the user clicked out of the interface
        //firstSelectedGameObject is the first button created in displayChoices 
        if (dialogueState == CHOICE_DISPLAYED)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }


        //visualize
        if (currentInteractable == null)
        {
            currentInteractable = null;
            actionLabel.text = "";
        }
        else
        {
            actionLabel.text = currentInteractable.actionText;
        }


        if (confirmTimer > 0)
            confirmTimer -= Time.deltaTime;


        //prevents keyboard input from being registered right after submitting a choice
        if (skipFrame)
        {
            skipFrame = false;
        }
        //action/click - go to settings > input to configure what "Fire1" is
        else if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            skipFrame = true;

            //if typing skip - justPressedUI is an inelegant way to avoid registering a choice event 
            //that just happened as a skip action
            if (typing)
            {
                typer.Skip();
                typing = false;
            }
            //dialogue is on but not on choices and not while typing: go to the next state
            else if (story.canContinue && dialogueState != DIALOGUE_OFF && dialogueState != CHOICE_DISPLAYED && !typing)
            {
                ContinueDialogue();
            }
            //colliding with interactable and not in dialogue mode: start the dialogue
            else if (currentInteractable != null && dialogueState == DIALOGUE_OFF)
            {


                //start dialogue
                if (currentInteractable.knotName != "")
                {
                    if (story.KnotContainerWithName(currentInteractable.knotName) != null)
                    {
                        StartDialogue(currentInteractable.knotName);
                    }
                    else
                    {
                        Debug.LogWarning("Warning: there is no ink node named " + currentInteractable.knotName);
                    }
                }

            }
            else if (dialogueState != DIALOGUE_OFF && !story.canContinue && story.currentChoices.Count == 0)
            {
                EndDialogue();
            }

        }

    }

    public void StartDialogue(string id)
    {
        dialogueOn = true;

        //freeze the controller by sending a message so I don't have to know the specific class
        //what "freezing means" depends on the control system
        if (player != null)
            player.SendMessage("Freeze", SendMessageOptions.DontRequireReceiver);

        //set the story at the knot
        story.ChoosePathString(id);

        panel.SetActive(true);

        ContinueDialogue();
    }


    // This is the main function called every time the story changes. It does a few things:
    // Destroys all the old content and choices.
    // Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
    void ContinueDialogue()
    {
        // Remove all choices and text from UI
        RemoveChoices();
        actionLabel.text = "";

        //if the option wait for choices is on add a dedicated step for displaying the choices
        if (dialogueState == WAIT_CHOICE)
        {
            //print("Display choices");
            //display choice
            DisplayChoices();
        }
        else if (story.canContinue || story.currentChoices.Count > 0)
        {
            //print("Story continue");
            bool lineDisplayed = false;

            // go to the next state
            story.Continue();

            //display a line if any
            if (story.currentText != "")
            {
                //print("Display line");
                DisplayLine(story.currentText);
                lineDisplayed = true;
            }

            //if there are choices attached to this block
            if (story.currentChoices.Count > 0 && story.currentText != "")
            {
                //print("Wait choice");
                //display choice next time
                dialogueState = WAIT_CHOICE;
                lineDisplayed = true;
            }

            if (!lineDisplayed && !story.canContinue && story.currentChoices.Count == 0)
                EndDialogue();

        }// story can't continue
        else
        {

            EndDialogue();
        }

    }


    void EndDialogue()
    {
        dialogueOn = false;
        print("Dialogue has ended");
        RemoveChoices();

        if (typer != null)
            typer.StopAllCoroutines();


        //presumably still in range?
        line.maxVisibleCharacters = int.MaxValue;


        dialogueState = DIALOGUE_OFF;

        typing = false;

        //freeze the controller by sending a message so I don't have to know the specific class
        if (player != null)
            player.SendMessage("UnFreeze", SendMessageOptions.DontRequireReceiver);

        if (currentInteractable != null)
        {

            if (currentInteractable.onlyOnce)
                currentInteractable.enabled = false;
            else if (currentInteractable != null)
                actionLabel.text = currentInteractable.actionText;
        }

        panel.SetActive(false);
    }

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice)
    {
        if (confirmTimer <= 0)
        {
            skipFrame = true;
            story.ChooseChoiceIndex(choice.index);
            ContinueDialogue();
        }

    }

    // Creates a textbox showing the the line of text
    void DisplayLine(string txt)
    {
        txt = txt.Trim();

        //if the typer isn't specified just display the line
        if (typer == null)
        {
            line.text = txt;
            dialogueState = WAIT_CONFIRM;
        }
        else
        {
            typing = true;
            typer.TypeText(txt, typingDelay);
        }

    }


    void DisplayChoices()
    {

        //clear selected ui element (unity UI quirk)
        EventSystem.current.SetSelectedGameObject(null);

        dialogueState = CHOICE_DISPLAYED;

        confirmTimer = confirmTimeGrace;

        choicesContainer.gameObject.SetActive(true);

        if (story.currentChoices.Count > 0)
        {

            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                Button button = CreateChoice(choice.text.Trim());

                // Tell the button what to do when we press it
                button.onClick.AddListener(delegate
                {
                    OnClickChoiceButton(choice);
                });


                //select the first button
                if (i == 0)
                {
                    EventSystem.current.firstSelectedGameObject = button.gameObject;
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
            }
        }


    }

    //listeners for the typer script

    //when a line has been typed
    private void OnDoneTyping()
    {
        //Debug.Log("TypeText Complete");

        //wait for advance confirmation unless there are choices at the end of this block 
        if (dialogueState != WAIT_CHOICE)
            dialogueState = WAIT_CONFIRM;

        if (dialogueState == WAIT_CHOICE)
            DisplayChoices();

        typing = false;



    }

    private void OnCharacterPrinted(string printedCharacter)
    {
        //sound and audiosources are set
        if (currentInteractable != null)
            if (typingAudioSource != null && typeSound != null)
            {

                // Do not play a sound for whitespace
                if (printedCharacter == " " || printedCharacter == "\n")
                {
                    return;
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, typeSound.Length);
                    AudioClip clip = typeSound[index];
                    typingAudioSource.PlayOneShot(clip);
                }
            }
    }


    // Creates a button showing the choice text
    Button CreateChoice(string text)
    {
        // Creates the button from a prefab
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(choicesContainer, false);

        // Gets the text from the button prefab
        TMP_Text choiceText = choice.GetComponentInChildren<TMP_Text>();
        choiceText.text = text;

        return choice;
    }

    public void CustomSubmit(BaseEventData eventData)
    {
        //Output that the Button is in the submit stage
        Debug.Log("Submitted!");
    }


    // Destroys all the children of this gameobject (all the UI)
    void RemoveChoices()
    {

        int childCount = choicesContainer.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(choicesContainer.GetChild(i).gameObject);
        }

        choicesContainer.gameObject.SetActive(false);
    }

    //teleports player to an object named id
    public void Teleport(string id)
    {
        GameObject obj = GameObject.Find(id);

        if (obj == null)
        {
            Debug.LogWarning("Warning: Teleport failed. I couldn't find a game object named " + id);
        }
        else if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            player.transform.position = obj.transform.position;

            if (cc != null)
                cc.enabled = true;


        }
    }

    public void Activate(string id)
    {
        Transform[] transforms = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform t in transforms)
        {
            if (t.name == id)
            {
                t.gameObject.SetActive(true);
            }
        }
    }

    public void Deactivate(string id)
    {
        GameObject obj = GameObject.Find(id);

        if (obj == null)
        {
            Debug.LogWarning("Warning: Dectivation failed. I couldn't find a game object named " + id);
        }
        else
        {
            obj.SetActive(false);
        }
    }

    //plays a one shot sound
    //for looping sound it's easier to activate and deactivate and object with a looping source
    public void PlaySound(string fileName)
    {
        //sound has to be in Resources and have exactly the same name (minus extension) passed by ink
        AudioClip clip = Resources.Load(fileName) as AudioClip;

        if (clip == null)
        {
            Debug.LogWarning("Warning: I couldn't find any sound file named " + fileName + " in Assets/Resources/");

        }
        else
        {
            //there has to be an audio source component on this object or it will create one from scratch
            AudioSource source = GetComponent<AudioSource>();

            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.PlayOneShot(clip);
        }
    }

    //this allows you to call functions from ink
    public void GameEvent(string id)
    {
        print("Ink calls the game event " + id);

        //example of function
        if (id == "restart")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    //same as above but with two parameters
    public void CallFunction(string functionName, string parameter)
    {
        print("Ink calls the game event " + functionName + " with parameter " + parameter);
    }

}
