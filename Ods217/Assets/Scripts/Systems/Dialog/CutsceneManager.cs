using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour {
    
    Text TextArea;
    Text sideTextArea;
    Image portraitArea;
    Image sidePortraitArea;

	CustomButtonUI buttonLeft;
	CustomButtonUI buttonRight;

	enum DecisionType { Power, Pickup, Save };
	DecisionType currentDecisionType;
	List<GameObject> buttonEffector; // A generic variable to determine what the decision is effecting
     
    public static CutsceneManager instance;
    public static bool InCutscene;
    [TextArea(1,100)]
    public string fullCutscene;

	bool makingADecision;
	bool breakOutDecision;


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
    Dictionary<string, IPawn> loadedPawns = new Dictionary<string, IPawn>();
     

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

		CustomButtonUI[] buttons = GetComponentsInChildren<CustomButtonUI>();
		foreach (CustomButtonUI b in buttons)
		{
			if (b.name == "DecisionL")
				buttonLeft = b;
			if (b.name == "DecisionR")
				buttonRight = b;
		}

		buttonRight.gameObject.SetActive(false);
		buttonLeft.gameObject.SetActive(false);
		makingADecision = false;

		buttonEffector = new List<GameObject>();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        MainTextBox.SetActive(ShowMain);
        SideTextBox.SetActive(ShowSide);

		buttonLeft.gameObject.SetActive(makingADecision);
		buttonRight.gameObject.SetActive(makingADecision);

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

		// Handle button nav because we have custom buttons
		if (makingADecision)
		{
			if (Input.GetKeyDown(KeyCode.D))
			{
				if (CustomButtonUI.Selected.NavRight != null)
					CustomButtonUI.Selected = CustomButtonUI.Selected.NavRight;
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				if (CustomButtonUI.Selected.NavLeft != null)
					CustomButtonUI.Selected = CustomButtonUI.Selected.NavLeft;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				DecisionMade((CustomButtonUI.Selected == buttonLeft));
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
        IPawn pawn;
        float fGeneric;
        int iGeneric;
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
                SetPlayerHalted(true);
                actionComplete = true;
                break;
            case "RESUME":
                SetPlayerHalted(false);
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
            case "ENDSAY": 
                ShowMain = false;
                actionComplete = true;
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
            case "CAMERASIZE":
                // Sets the main cameras orthographic size to whatever we want it to
                savedName = parameters[0].Trim().Replace(")", "");
                float.TryParse(savedName, out fGeneric);

                Camera.main.orthographicSize = fGeneric;
                actionComplete = true;
                break;
            case "LERPCAMERASIZE":
                // Sets the main cameras orthographic size to whatever we want it to (LERPED VERSION)
                savedName = parameters[0].Trim().Replace(")", "");
                float.TryParse(savedName, out fGeneric);
                savedName = parameters[1].Trim().Replace(")", "");
                float foo;
                float.TryParse(savedName, out foo);
                Camera.main.GetComponent<CamScript>().LerpSize(fGeneric, foo);
                actionComplete = true;
                break;
            case "LOCK":
                // Toggle the zones lock
                savedName = parameters[0].Trim().Replace(")", "");
                int.TryParse(savedName, out iGeneric);

                if(iGeneric < ZoneScript.ActiveZone.ZoneLocks.Length)
                {
                    ZoneScript.ActiveZone.ZoneLocks[iGeneric].Enabled = !ZoneScript.ActiveZone.ZoneLocks[iGeneric].Enabled;
                }

                actionComplete = true;
                break;
            case "LOADPAWN":
                // Save a pawn in the dictionazry for further use 
                // LoadPawn(ObjectInSceneName, pawnName)
                savedName = parameters[0].Trim().Replace(")", "");
                GameObject nObj = GlobalConstants.FindGameObject(savedName); 
               
                pawn = nObj.GetComponent<IPawn>();
                if(pawn != null)
                {
                    loadedPawns.Add(parameters[1].Trim().Replace(")", ""), pawn);
                }

                actionComplete = true;
                break;
            case "MOVEPAWN":
                // Tell a pawn to move
                // MovePawn(ID, x,y,z)
                Vector3 newVec = Vector3.zero;
                savedName = parameters[0].Trim().Replace(")", ""); 

                if(loadedPawns.ContainsKey(savedName))
                {
                    pawn = loadedPawns[savedName];
                    savedName = parameters[1].Trim().Replace(")", "");
                    float.TryParse(savedName, out fGeneric);
                    newVec.x = fGeneric;
                    savedName = parameters[2].Trim().Replace(")", "");
                    float.TryParse(savedName, out fGeneric);
                    newVec.y = fGeneric;
                    savedName = parameters[3].Trim().Replace(")", "");
                    float.TryParse(savedName, out fGeneric);
                    newVec.z = fGeneric;

 
                    pawn.MoveTo(newVec);
                }

                actionComplete = true;
                break;
            case "FLIP":
                // Flip the portrait areas x axis so they look the other way now
                Vector3 currentScale = portraitArea.transform.localScale;
                currentScale.x *= -1;
                portraitArea.transform.localScale = currentScale;
                actionComplete = true;

                break;
			case "DECISION":
				// There's a decision to be made
				// Decision(Type,TextLeft,TextRight)

				string Type = parameters[0].Trim().ToUpper();

				switch (Type)
				{
					case "SAVE":
						currentDecisionType = DecisionType.Save;
						break;
					default:
						Debug.Log("Couldn't determine decision type");
						actionComplete = true;
						break;
				}

				buttonLeft.GetComponentInChildren<Text>().text = parameters[1].Trim();
				buttonRight.GetComponentInChildren<Text>().text = parameters[2].Trim().Replace(")", "");

				CustomButtonUI.Selected = buttonLeft;

				makingADecision = true;
				breakOutDecision = false;

				break;
            case "SETTRACK":
                // Set the music managers track to a desired volume
                // SYNTAX: SetTrack(index, newvolume)
                savedName = parameters[0].Trim().Replace(")", "");
                int.TryParse(savedName, out iGeneric);
                savedName = parameters[1].Trim().Replace(")", "");
                float.TryParse(savedName, out fGeneric);

                if (NewMusicManager.instance != null)
                {
                    NewMusicManager.instance.SetTrack(iGeneric, fGeneric);
                    actionComplete = true;
                }
                break;
            case "MOVETO":

                savedName = parameters[0].Trim().Replace(")", "");
                SceneManager.LoadScene(savedName);
                break;
            case "TRIGGERIND":
                // Flip the disabled switch on an EIndicator script
                // Based on perminants 
                // TriggerInd(PermName)
                savedName = parameters[0].Trim().Replace(")", "").ToUpper();
                if (loadedPermanents.ContainsKey(savedName))
                {
                    if (loadedPermanents[savedName] == null)
                        Debug.Log("Couldn't find perminant on loaded perm: " + savedName);
                    else
                    {
                        UsableIndicator ind = loadedPermanents[savedName].gameObject.GetComponentInChildren<UsableIndicator>();
                        if(ind != null)
                        {
                            ind.Disabled = !ind.Disabled;
                        }
                    }
                }
                else
                {
                    Debug.Log("Couldn't find permanent: " + savedName + " within the loaded permanents dictionary. Make sure it's loaded first");
                }

                actionComplete = true;

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

        portraitArea.transform.localScale = new Vector3(-1, 1, 1);
        sidePortraitArea.transform.localScale = new Vector3(-1, 1, 1);

  

        // Reset important variables
        loadedCharacters = new Dictionary<string, CutsceneCharacters>();
        loadedPermanents = new Dictionary<string, IPermanent>();
        loadedPawns = new Dictionary<string, IPawn>();
        currentCutsceneCharacter = null;
        sideTextArea.text = "";
        TextArea.text = "";
    }

    void SetPlayerHalted(bool _isHalted)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerS = player.GetComponent<PlayerScript>();
            if (playerS != null)
            {
                if (_isHalted)
                    playerS.GetComponent<CController>().HaltMomentum();

                playerS.AcceptInput = !_isHalted;
                playerHalted = _isHalted;
            }
        }
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

	public void DecisionMade(bool _decision)
	{
		switch (currentDecisionType)
		{
		case DecisionType.Save:
			if(_decision)
			{
				// Save the game!
				if(GameManager.instance.WriteToCurrentSave())
				{
					fullCutscene += "\nSay(Console,Game Saved!,Happy)";
				}
				else
				{
					fullCutscene += "\nSay(Console,Sorry... There was a problem saving your data.,Sad)";
				}

				maxLine++;
			}
			break;
		}
			
		breakOutDecision = true;
		makingADecision = false;
		actionComplete = true;
	}
}



 
