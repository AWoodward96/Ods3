﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour {

    Canvas MyCanvas;
    Text TextArea;
    Text sideTextArea;
    Image portraitArea;
    Image sidePortraitArea;
     
    public static CutsceneManager instance;
    public static bool InCutscene;
    [TextArea(1,100)]
    public string fullCutscene;


    // For texting purposes
    string currentString;
    string lastString;
    public int currentLine;
    int maxLine;
    int currentCharacter;
    int speakingSpeed = 1;
    bool waitTexting = false;

    public bool actionComplete;


    public AudioClip[] AudioClips;
    AudioSource mySource;

    PlayerScript playerS; // Because I don't want to find this mother fucker twice
    bool playerHalted; // This will be true if we've found a character to hault and we've prevented him from moving


    // Cutscene Data Storage
    CutsceneCharacters currentCutsceneCharacter;
    Dictionary<string, CutsceneCharacters> loadedCharacters = new Dictionary<string, CutsceneCharacters>();
    Dictionary<string, IPermanent> loadedPermanents = new Dictionary<string, IPermanent>();
     

    public GameObject MainTextBox;
    public GameObject SideTextBox;
    public bool ShowMain = false;
    public bool ShowSide = false;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        InCutscene = false;
        MyCanvas = GetComponentInChildren<Canvas>();
        mySource = GetComponent<AudioSource>();


        // Set up the texts
        Text[] allText = GetComponentsInChildren<Text>();
        foreach (Text t in allText)
        {
            if (t.name == "TextArea")
                TextArea = t;

            if (t.name == "SideTextArea")
                sideTextArea = t;
        }

        // Set up the images
        Image[] allImg = GetComponentsInChildren<Image>();
        foreach(Image i in allImg)
        {
            if (i.name == "portraitArea")
                portraitArea = i;

            if (i.name == "sidePortraitArea")
                sidePortraitArea = i;
        }

        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        MainTextBox.SetActive(ShowMain);
        SideTextBox.SetActive(ShowSide);

        if(InCutscene)
        {
            if(actionComplete) // If we've completed the previous action
            {
                // Check to see if we're done with the cutscene
                if(currentLine >= maxLine)
                {
                    // We're done boyos 
                    InCutscene = false;
                    TextArea.text = "";

                    ShowMain = false;
                    ShowSide = false;
                     
                    MainTextBox.SetActive(false);
                    SideTextBox.SetActive(false);

                    if (playerHalted)
                    {
                        playerHalted = false;
                        playerS.AcceptInput = true;
                    }

                    return;
                }else
                {
                    // Otherwise parse the next string
                    actionComplete = false;
                    ParseString();
                }
            }
        }
    }


    void ParseString()
    {
        // parse the next string
        string[] splitString = fullCutscene.Split('\n');
        string fullLine = splitString[currentLine];

        currentLine++;

        // EE1: There is no command, or it's a comment
        string[] lineSpit = fullLine.Split('(');
        string commandID = lineSpit[0].Trim(); 
        if(commandID == "" || fullLine[0] == '/')
        {
            actionComplete = true;
            return;
        }

        string[] parameters = lineSpit[1].Split(',');
        string Text = "";
        string CharacterID = "";
        string PortraitID = "";
        string savedName = "";
        switch (commandID.ToUpper())
        {
            case "SAY":
                // The say method is for displaying things to the textbox
                // Valid Syntax:
                // Say(CharacterID,Text) [The bear minimum. Will use the last 
                // Say(CharacterID,Text,PortraidID) [Will change the portrait and then say the words]
                CharacterID = parameters[0].Trim().ToUpper();
                Text = parameters[1].Trim().Replace(")","");
                PortraitID = "";
                if (parameters.Length > 2)
                    PortraitID = parameters[2].Trim().Replace(")", "");
 
                // If the character we want isn't dead
                if (loadedCharacters.ContainsKey(CharacterID))
                { 
                    CutsceneCharacters character = loadedCharacters[CharacterID];
                    if(character != currentCutsceneCharacter)
                    { 
                        portraitArea.sprite = character.defaultPortrait;
                    }
                     
                    if (PortraitID != "" && character.characterPortraits.Length > 0)
                    {
                        // Loop through the characters portrait and select the right portrait to send to the portrait area
                        foreach(CutscenePortraits p in character.characterPortraits)
                        {
                            if (PortraitID.ToUpper() == p.PortId.ToUpper())
                                portraitArea.sprite = p.PortImg;
                        }
                    }
                     
                    currentCutsceneCharacter = character;
                    ShowMain = true;
                    StartCoroutine(Speak(Text,0));
                }else
                {
                    Debug.Log("Couldn't find character: " + CharacterID + ", at line: " + currentLine);
                }


                //currentCutsceneCharacter = lo
                break;
            case "CONTINUE":
                // Rather then reset the line, this method continues where we left off after we hit space
                // Valid Syntax
                // Continue(Text)
                // Continue(Text,PortraitID) 
                Text = parameters[0].Trim().Replace(")", "");
                if(parameters.Length > 1)
                    PortraitID = parameters[1].Trim().Replace(")", "");
                 

                if (PortraitID != "" && currentCutsceneCharacter.characterPortraits.Length > 0)
                {
                    // Loop through the characters portrait and select the right portrait to send to the portrait area
                    foreach (CutscenePortraits p in currentCutsceneCharacter.characterPortraits)
                    {
                        if (PortraitID.ToUpper() == p.PortId.ToUpper())
                            portraitArea.sprite = p.PortImg;
                    }
                }



                ShowMain = true;
                StartCoroutine(Speak(lastString + " " + Text, lastString.Length));

                break;
            case "LOADCHAR":
                // Loads and saves a character into a dictionary
                // LoadChar(resourceName, savedName)
                string resourceName = parameters[0].Trim().Replace(")", "");
                savedName = parameters[1].Trim().Replace(")", "");

                CutsceneCharacters c = (Resources.Load("CutsceneData/Characters/" + resourceName) as GameObject).GetComponent<CutsceneCharacters>();
                if(c != null)
                { 
                    loadedCharacters.Add(savedName.ToUpper(), c);
                    actionComplete = true;
                }else
                {
                    Debug.Log("Couldn't load character: " + resourceName + ", at line " + currentLine);
                }

                break;
            case "LOADPERM":
                // Loads and saves a IPermanent into a dictionary
                // LoadPerm(ObjectInSceneName, savedName)
                string ObjectInSceneName = parameters[0].Trim().Replace(")", "");
                savedName = parameters[1].Trim().Replace(")", "");

                GameObject obj = GlobalConstants.FindGameObject(ObjectInSceneName);
 

                if (obj != null)
                {
                    loadedPermanents.Add(savedName.ToUpper(), obj.GetComponent<IPermanent>());
                    actionComplete = true;
                }
                else
                {
                    Debug.Log("Couldn't load permanent: '" + ObjectInSceneName + "', at line " + currentLine);
                }
                break;
            case "TRIGGER":
                // Toggles the trigger on a saved permanent
                // Trigger(PermName)
                savedName = parameters[0].Trim().Replace(")", "").ToUpper();
                if(loadedPermanents.ContainsKey(savedName))
                {
                    if (loadedPermanents[savedName] == null)
                        Debug.Log("Couldn't find perminant on loaded perm: " + savedName);
                    else
                        loadedPermanents[savedName].Triggered = !loadedPermanents[savedName].Triggered;
                }else
                {
                    Debug.Log("Couldn't find permanent: " + savedName + " within the loaded permanents dictionary. Make sure it's loaded first");
                }

                actionComplete = true;
                break;
            case "HALTPLAYER":
                // Stops the players inputs from effecting the scene
                // this includes clicking 
                GameObject player = GameObject.FindGameObjectWithTag("Player"); 
                if (player)
                {
                    playerS = player.GetComponent<PlayerScript>();
                    if (playerS != null)
                    {
                        playerS.AcceptInput = false;
                        playerHalted = true;
                    }
                }
                actionComplete = true;
                break;
            case "WAITTEXT":
                // Starts a waitText. While in a wait text, there are things going on in the scene that need to be done before you can progress to the next text.
                // The waitText will end when the EndWait() is called
                waitTexting = true;
                // WaitText(CharacterID,Text)
                // WaitText(CharacterID,Text,PortraitID)
                Text = parameters[0].Trim().Replace(")", "");
                if (parameters.Length > 1)
                    PortraitID = parameters[1].Trim().Replace(")", "");


                if (PortraitID != "" && currentCutsceneCharacter.characterPortraits.Length > 0)
                {
                    // Loop through the characters portrait and select the right portrait to send to the portrait area
                    foreach (CutscenePortraits p in currentCutsceneCharacter.characterPortraits)
                    {
                        if (PortraitID.ToUpper() == p.PortId.ToUpper())
                            portraitArea.sprite = p.PortImg;
                    }
                }

                ShowMain = true;
                StartCoroutine(Speak(lastString + " " + Text, lastString.Length));
                actionComplete = true;
                break;
            case "ENDWAIT":
                // Breaks us out of a waitText 
                waitTexting = false; 
                break;
            case "WAIT":
                // A simple stopage of the dialog for a set amount of time
                // Usually to show something or to delay something
                // Wait(Time)
                string s = parameters[0].Trim().Replace(")", "");
                float waitFloat;
                float.TryParse(s, out waitFloat);

                StartCoroutine(WaitCRT(waitFloat));
                break;
            case "SIDESAY":
                // Write something to the side text area. Unlike the main text area, this text comes out automatically and doesn't require you to press space to proceed

                ShowSide = true;
                // SIDESAY(CharacterID,Text) [The bear minimum. Will use the last portrait]
                // SIDESAY(CharacterID,Text,PortraidID) [Will change the portrait and then say the words] 
                CharacterID = parameters[0].Trim().ToUpper();
                Text = parameters[1].Trim().Replace(")", "");
                PortraitID = "";
                if (parameters.Length > 2)
                    PortraitID = parameters[2].Trim().Replace(")", "");

                // If the character we want isn't dead
                if (loadedCharacters.ContainsKey(CharacterID))
                {
                    CutsceneCharacters character = loadedCharacters[CharacterID];
                    if (character != currentCutsceneCharacter)
                    {
                        sidePortraitArea.sprite = character.defaultPortrait;
                    }

                    if (PortraitID != "" && character.characterPortraits.Length > 0)
                    {
                        // Loop through the characters portrait and select the right portrait to send to the portrait area
                        foreach (CutscenePortraits p in character.characterPortraits)
                        {
                            if (PortraitID.ToUpper() == p.PortId.ToUpper())
                                sidePortraitArea.sprite = p.PortImg;
                        }
                    }
                     
                    currentCutsceneCharacter = character;
                    StartCoroutine(Aside(Text, 0));
                }
                else
                {
                    Debug.Log("Couldn't find character: " + CharacterID + ", at line: " + currentLine);
                }

                actionComplete = true;
                break;
            case "ENDSIDE":
                // Ends a side say 
                actionComplete = true;
                ShowSide = false;
                StopCoroutine("Aside");
                break;
            case "CAMERATARGET":
                // Sets the main cameras target to whatever we want it to
                savedName = parameters[0].Trim().Replace(")", "");
                if(savedName.ToUpper() == "PLAYER")
                {
                    // Make it see the player again
                    Camera.main.GetComponent<CamScript>().Target = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
                    actionComplete = true;
                    return;
                }

                // Look for it in our dictionaries
                if(loadedPermanents.ContainsKey(savedName.ToUpper()))
                {
                    Camera.main.GetComponent<CamScript>().Target = loadedPermanents[savedName.ToUpper()].gameObject.transform;
                    actionComplete = true;
                    return;
                }


                // Look for it in scene
                GameObject target = GlobalConstants.FindGameObject(savedName);

                if(target != null)
                {
                    Camera.main.GetComponent<CamScript>().Target = target.transform;
                    actionComplete = true;
                    return;
                } 
                break;
            default:
                Debug.Log("Couldn't process: " + commandID);
                actionComplete = true;
                return;
        }

    }

    // The method to call when you want to start a cutscene
    public void StartCutscene(string text)
    {
        string[] splitString = text.Split('\n');
        maxLine = splitString.Length;
        currentLine = 0;
        fullCutscene = text;

        actionComplete = true;
        InCutscene = true;
        ShowSide = false;
        ShowMain = false;

        StopAllCoroutines();

  

        // Reset important variables
        loadedCharacters = new Dictionary<string, CutsceneCharacters>();
        loadedPermanents = new Dictionary<string, IPermanent>();
        currentCutsceneCharacter = null;
        sideTextArea.text = "";
        TextArea.text = "";
    }


    IEnumerator Speak(string _Line, int _startingIndex)
    {
        string fullLine = _Line;
        currentCharacter = _startingIndex;
        currentString = _Line.Substring(_Line.Length - _startingIndex);


        bool readingText = true; // Lets start an infinite loop
        while (readingText) // WEEEEEEEE
        {
             
            // If the string is less then the full line then keep writing
            if (currentString.Length < fullLine.Length)
            {
                // This will write the text out all scrolly n shit
                currentString = fullLine.Substring(0, currentCharacter);
                currentCharacter++;
                mySource.clip = currentCutsceneCharacter.talkingClip;
                mySource.Play();
                TextArea.text = currentString;

                // If we press space, before the line is complete, just complete the line. Because yeah.
                if (Input.GetKeyDown(KeyCode.Space))
                    currentCharacter = fullLine.Length;
            }

            if (Input.GetKeyDown(KeyCode.Space) && currentString.Length == fullLine.Length)// If the line is complete, then lets gtfo of here
            {
                // break out
                if(waitTexting == false) // but only if we're not waitTexting
                {
                    readingText = false;
                    actionComplete = true;
                    lastString = currentString; 
                }
            }

            // This makes it so we don't get an infinite loop error
            yield return null;
        }


        yield break;
    }

    IEnumerator Aside(string _Line, int _startingIndex)
    {
        string fullLine = _Line;
        currentCharacter = _startingIndex;
        currentString = _Line.Substring(_Line.Length - _startingIndex);

        bool readingText = true; // Lets start an infinite loop
        while (readingText) // WEEEEEEEE
        {

            // If the string is less then the full line then keep writing
            if (currentString.Length < fullLine.Length)
            {
                // This will write the text out all scrolly n shit
                currentString = fullLine.Substring(0, currentCharacter);
                currentCharacter++;
                mySource.clip = currentCutsceneCharacter.talkingClip;
                mySource.Play();
                sideTextArea.text = currentString;
            }

            if (!ShowSide)
                yield break;
            // This makes it so we don't get an infinite loop error
            yield return null;
        } 

        yield break;
    }

    IEnumerator WaitCRT(float time)
    {
        yield return new WaitForSeconds(time);
        actionComplete = true;
    }

}



 