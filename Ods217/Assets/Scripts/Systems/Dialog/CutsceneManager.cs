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

	enum DecisionType { Power, Pickup, Save, Breakpoint };
	DecisionType currentDecisionType;
	List<GameObject> buttonEffector; // A generic variable to determine what the decision is effecting
     
    public static CutsceneManager instance;
    public static bool InCutscene;
    [TextArea(1,100)]
    public string fullCutscene;

	bool makingADecision;
	bool breakOutDecision;
    string decisionIDTrue;
    string decisionIDFalse;
    

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
	Dictionary<string, Item> loadedItems = new Dictionary<string, Item>();
	Dictionary<string, string> loadedObjectives = new Dictionary<string, string>();
    Dictionary<string, int> loadedBreakpoints = new Dictionary<string, int>();
     

    public GameObject MainTextBox;
    public GameObject SideTextBox;
    public bool ShowMain = false;
    public bool ShowSide = false;
    public bool Skipping = false;
    bool LongSkip = false;

    private void Awake()
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

                    Skipping = false;

                    if (LongSkip)
                        Camera.main.GetComponent<CamScript>().FadeIn(1);

                    return;
                }else
                { 
                    // Otherwise parse the next string
                    actionComplete = false;
                    ParseString();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !Skipping)
            {
                Skipping = true;
                LongSkip = (maxLine > 20);
                StopAllCoroutines();

                if(!makingADecision)
                {
                    ShowMain = false;
                    ShowSide = false;

                    actionComplete = true;

                    MainTextBox.SetActive(false);
                    SideTextBox.SetActive(false);

                    if (LongSkip)
                        Camera.main.GetComponent<CamScript>().FadeOut(.5f);
                     
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

        if(fullLine[0] == '*') // Stars indicate a breakpoint/checkpoint
        {
            actionComplete = true;
            return;
        }

        string[] parameters = lineSpit[1].Split(',');
        string Text = "";
        string CharacterID = "";
        string PortraitID = "";
        string sGeneric = "";
        string sGeneric2 = "";
        GameObject goGeneric;
        GameObject playerTemp;
        IPawn pawn;
        float fGeneric;
        int iGeneric;
        Vector3 v3Generic;
         
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
                    lastString = Text;

                    if (Skipping) // If we're skipping, dialog is not important. Go onwards my child
                    {
                        actionComplete = true; 
                        return;
                    }

                    ShowMain = true;
                    StartCoroutine(Speak(Text,0));
                }else
                {
                    Debug.Log("Couldn't find character: " + CharacterID + ", at line: " + currentLine);
                } 
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

                Text = lastString + " " + Text;
                int l = lastString.Length;
                lastString = Text;

                if (Skipping) // If we're skipping, dialog is not important. Go onwards my child
                {
                    actionComplete = true;
                    return;
                }

                ShowMain = true;
                StartCoroutine(Speak(Text, l));

                break;
            case "LOADCHAR":
                // Loads and saves a character into a dictionary
                // LoadChar(resourceName, sGeneric)
                string resourceName = parameters[0].Trim().Replace(")", "");
                sGeneric = parameters[1].Trim().Replace(")", "");

                CutsceneCharacters c = (Resources.Load("CutsceneData/Characters/" + resourceName) as GameObject).GetComponent<CutsceneCharacters>();
                if(c != null)
                { 
                    loadedCharacters.Add(sGeneric.ToUpper(), c);
                    actionComplete = true;
                }else
                {
                    Debug.Log("Couldn't load character: " + resourceName + ", at line " + currentLine);
                }

                // Loading character doesn't take any real ingame time. Don't bother with skipping
                break;
            case "LOADPERM":
                // Loads and saves a IPermanent into a dictionary
                // LoadPerm(ObjectInSceneName, sGeneric)
                string ObjectInSceneName = parameters[0].Trim().Replace(")", "");
                sGeneric = parameters[1].Trim().Replace(")", "");

                GameObject obj = GlobalConstants.FindGameObject(ObjectInSceneName);
 

                if (obj != null)
                {
                    loadedPermanents.Add(sGeneric.ToUpper(), obj.GetComponent<IPermanent>());
                    actionComplete = true;
                }
                else
                {
                    Debug.Log("Couldn't load permanent: '" + ObjectInSceneName + "', at line " + currentLine);
                }
                // Loading perms doesn't take any real ingame time. Don't bother with skipping
                break;
            case "TRIGGER":
                // Toggles the trigger on a saved permanent
                // Trigger(PermName)
                sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
                if(loadedPermanents.ContainsKey(sGeneric))
                {
                    if (loadedPermanents[sGeneric] == null)
                        Debug.Log("Couldn't find perminant on loaded perm: " + sGeneric);
                    else
                        loadedPermanents[sGeneric].Triggered = !loadedPermanents[sGeneric].Triggered;
                }else
                {
                    Debug.Log("Couldn't find permanent: " + sGeneric + " within the loaded permanents dictionary. Make sure it's loaded first");
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

                if (Skipping) // If we're skipping, dialog is not important. Go onwards my child
                {
                    actionComplete = true;
                    return;
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

                if (Skipping) // If we're skipping, waiting is not important. Go onwards my child
                {
                    actionComplete = true;
                    return;
                }

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

                    if (Skipping) // If we're skipping, side dialog is not important. Go onwards my child
                    {
                        actionComplete = true;
                        return;
                    }

                    StartCoroutine(Aside(Text, 0));
                }
                else
                {
                    Debug.Log("Couldn't find character: " + CharacterID + ", at line: " + currentLine);
                }

                actionComplete = true;
                break;
            case "END":
                // End the cutscene early
                currentLine = fullCutscene.Length;

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
                sGeneric = parameters[0].Trim().Replace(")", "");
                if(sGeneric.ToUpper() == "PLAYER")
                {
                    // Make it see the player again
                    Camera.main.GetComponent<CamScript>().Target = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
                    actionComplete = true;
                    return;
                }

                // Look for it in our dictionaries
                if(loadedPermanents.ContainsKey(sGeneric.ToUpper()))
                {
                    Camera.main.GetComponent<CamScript>().Target = loadedPermanents[sGeneric.ToUpper()].gameObject.transform;
                    actionComplete = true;
                    return;
                }


                // Look for it in scene
                GameObject target = GlobalConstants.FindGameObject(sGeneric);

                if(target != null)
                {
                    Camera.main.GetComponent<CamScript>().Target = target.transform;
                    actionComplete = true;
                    return;
                } 
                break;
            case "CAMERASHAKE":
                // CAMERASHAKE(_float howlong)
                sGeneric = parameters[0].Trim().Replace(")", "");
                float.TryParse(sGeneric, out fGeneric);
                Camera.main.GetComponent<CamScript>().AddEffect(CamScript.CamEffect.Shake, fGeneric);
                actionComplete = true;
                break;
            case "CAMERASIZE":
                // Sets the main cameras orthographic size to whatever we want it to
                sGeneric = parameters[0].Trim().Replace(")", "");
                float.TryParse(sGeneric, out fGeneric);

                Camera.main.orthographicSize = fGeneric;
                actionComplete = true;
                break;
            case "LERPCAMERASIZE":
                // Sets the main cameras orthographic size to whatever we want it to (LERPED VERSION)
                sGeneric = parameters[0].Trim().Replace(")", "");
                float.TryParse(sGeneric, out fGeneric);
                sGeneric = parameters[1].Trim().Replace(")", "");
                float foo;
                float.TryParse(sGeneric, out foo);
                Camera.main.GetComponent<CamScript>().LerpSize(fGeneric, foo);
                actionComplete = true;
                break;
            case "LOCK":
                // Toggle the zones lock
                sGeneric = parameters[0].Trim().Replace(")", "");
                int.TryParse(sGeneric, out iGeneric);

                if(iGeneric < ZoneScript.ActiveZone.ZoneLocks.Length)
                {
                    ZoneScript.ActiveZone.ZoneLocks[iGeneric].Enabled = !ZoneScript.ActiveZone.ZoneLocks[iGeneric].Enabled;
                }

                actionComplete = true;
                break;
            case "LOADPAWN":
                // Save a pawn in the dictionazry for further use 
                // LoadPawn(ObjectInSceneName, pawnName)
                sGeneric = parameters[0].Trim().Replace(")", "");
                GameObject nObj = GlobalConstants.FindGameObject(sGeneric); 
               
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
                sGeneric = parameters[0].Trim().Replace(")", ""); 

                if(loadedPawns.ContainsKey(sGeneric))
                {
                    pawn = loadedPawns[sGeneric];
                    sGeneric = parameters[1].Trim().Replace(")", "");
                    float.TryParse(sGeneric, out fGeneric);
                    newVec.x = fGeneric;
                    sGeneric = parameters[2].Trim().Replace(")", "");
                    float.TryParse(sGeneric, out fGeneric);
                    newVec.y = fGeneric;
                    sGeneric = parameters[3].Trim().Replace(")", "");
                    float.TryParse(sGeneric, out fGeneric);
                    newVec.z = fGeneric;

 
                    pawn.MoveTo(newVec);
                }
                else
                    Debug.Log("No pawn by the name: " + sGeneric + " found");

                actionComplete = true;
                break;
            case "LOOKPAWN":
                // Tell a pawn to move
                // LookPawn(ID, x,z)
                Vector3 newLook = Vector3.zero;
                sGeneric = parameters[0].Trim().Replace(")", "");

                if (loadedPawns.ContainsKey(sGeneric))
                {
                    // Pull out all the coordinate locations
                    pawn = loadedPawns[sGeneric];
                    sGeneric = parameters[1].Trim().Replace(")", "");
                    float.TryParse(sGeneric, out fGeneric);
                    newLook.x = fGeneric;
                    sGeneric = parameters[2].Trim().Replace(")", "");
                    float.TryParse(sGeneric, out fGeneric);
                    newLook.z = fGeneric;

                    // Tell the pawn to look
                    pawn.Look(newLook);
                }
                else
                    Debug.Log("No pawn by the name: " + sGeneric + " found");

                actionComplete = true;
                break;
            case "SPRINTPAWN":
                // Tell a pawn to sprint or not sprint
                // SprintPawn(ID, 1 or 0)
                sGeneric = parameters[0].Trim().Replace(")", "");

                if(loadedPawns.ContainsKey(sGeneric))
                {
                    pawn = loadedPawns[sGeneric];


                    sGeneric = parameters[1].Trim().Replace(")", "");
                    int.TryParse(sGeneric, out iGeneric);

                    pawn.cc.Sprinting = (iGeneric == 1);
                }

                actionComplete = true;

                break;
            case "AGGROPAWN": 
                // Tell a pawn to aggro up
                // AggroPawn(ID, 1 or 0)
                sGeneric = parameters[0].Trim().Replace(")", "");

                if (loadedPawns.ContainsKey(sGeneric))
                {
                    pawn = loadedPawns[sGeneric];


                    sGeneric = parameters[1].Trim().Replace(")", "");
                    int.TryParse(sGeneric, out iGeneric);

                    pawn.SetAggro(iGeneric == 1);
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
			case "DECISIONSAVE":
                // There's a decision to be made
                // DecisionSave()

                // There is no skipping decisions. Don't try it. 
                // In fact a decision will stop a skip
                if(LongSkip)
                {
                    Camera.main.GetComponent<CamScript>().FadeIn(1);
                }
                Skipping = false;

                // Debug.Log("Making a decision atm");
                ShowMain = true;
                TextArea.text = lastString;
                 

                currentDecisionType = DecisionType.Save; 
				buttonLeft.GetComponentInChildren<Text>().text = "Yes";
                buttonRight.GetComponentInChildren<Text>().text = "No";

				CustomButtonUI.Selected = buttonLeft;

				makingADecision = true;
				breakOutDecision = false; 
				break;
            case "DECISIONSIMPLE":
                // There is a simple yes or no decision to be made
                // DecisionSimple(BreakpointID1,BreakpointID2)
                if (LongSkip)
                {
                    Camera.main.GetComponent<CamScript>().FadeIn(1);
                }
                Skipping = false;

                // Debug.Log("Making a decision atm");
                ShowMain = true;
                TextArea.text = lastString;

                decisionIDTrue = parameters[0].Trim().Replace(")", "");
                decisionIDFalse = parameters[1].Trim().Replace(")", "");


                currentDecisionType = DecisionType.Breakpoint;
                buttonLeft.GetComponentInChildren<Text>().text = "Yes";
                buttonRight.GetComponentInChildren<Text>().text = "No";

                CustomButtonUI.Selected = buttonLeft;

                makingADecision = true;
                breakOutDecision = false;
                break;
            case "SETTRACK":
                // Set the music managers track to a desired volume
                // SYNTAX: SetTrack(index, newvolume)
                sGeneric = parameters[0].Trim().Replace(")", "");
                int.TryParse(sGeneric, out iGeneric);
                sGeneric = parameters[1].Trim().Replace(")", "");
                float.TryParse(sGeneric, out fGeneric);

                if (NewMusicManager.instance != null)
                {
                    NewMusicManager.instance.SetTrack(iGeneric, fGeneric);
                    actionComplete = true;
                }
                break;
            case "MOVETO":
                // There is no skipping movetos. They're kinda important.
                sGeneric = parameters[0].Trim().Replace(")", "");
                GameManager.instance.PreservePlayer();

                if (GameManager.World.ContainsKey(sGeneric.ToUpper()))
				{
					GameManager.instance.LoadScene(GameManager.World[sGeneric.ToUpper()]);
				}
                else
                    SceneManager.LoadScene(sGeneric);


                SceneManager.sceneLoaded += GameManager.instance.RestorePlayer;
                break;
            case "UPDATEWORLD":
                // It's time to update what links in the world go where
                sGeneric = parameters[0].Trim().Replace(")", "");
                sGeneric2 = parameters[1].Trim().Replace(")", "");

                if (GameManager.World.ContainsKey(sGeneric.ToUpper()))
                    GameManager.World[sGeneric.ToUpper()] = sGeneric2;
                else
                    Debug.Log("Gamemanger world does not contain key for: " + sGeneric.ToUpper());

                actionComplete = true;
                
                break;
            case "TRIGGERIND":
                // Flip the disabled switch on an EIndicator script
                // Based on perminants 
                // TriggerInd(PermName)
                sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
                if (loadedPermanents.ContainsKey(sGeneric))
                {
                    if (loadedPermanents[sGeneric] == null)
                        Debug.Log("Couldn't find perminant on loaded perm: " + sGeneric);
                    else
                    {
                        UsableIndicator ind = loadedPermanents[sGeneric].gameObject.GetComponentInChildren<UsableIndicator>();
                        if(ind != null)
                        {
                            ind.Disabled = !ind.Disabled;
                        }
                    }
                }
                else
                {
                    Debug.Log("Couldn't find permanent: " + sGeneric + " within the loaded permanents dictionary. Make sure it's loaded first");
                }

                actionComplete = true;

                break;

			case "DISABLEOBJECT":
				// Disables the indicated object; does not need to be loaded in-cutscene
				// DisableObject(ObjectInSceneName) 
				sGeneric = parameters[0].Trim().Replace(")", "");
				goGeneric = GameObject.Find(sGeneric);
				if(goGeneric != null)
				{
                    goGeneric.SetActive(false);
				}
				else
				{
					Debug.Log("Could not find GameObject " + sGeneric + "!");
				}
				actionComplete = true;
				break;
            case "ENABLEOBJECT":
                // Enables the indicated object; does not need to be loaded in-cutscene. Will find prefabs, be specific in your naming conventions
                // EnableObject(ObjectInSceneName) 
                sGeneric = parameters[0].Trim().Replace(")", "");
                goGeneric = GlobalConstants.FindGameObject(sGeneric);
                if (goGeneric != null)
                {
                    goGeneric.SetActive(true);
                }
                else
                {
                    Debug.Log("Could not find GameObject " + sGeneric + "!");
                }
                actionComplete = true;
                break;
            case "LOADSCENE":
                sGeneric = parameters[0].Trim().Replace(")", "");
                GameManager.instance.LoadScene(sGeneric);
                actionComplete = true;
                break;

            case "SAVE":
			GameManager.instance.WriteSaveFile();
                actionComplete = true;
                break;

			case "CACHE":
				GameManager.instance.WriteCache();
				actionComplete = true;
				break;

			case "TOGGLEDOORLOCK":
				// Sets the door lock to the given value
				// ToggleDoorLock(ObjectInSceneName, myLockValue)
				ObjectInSceneName = parameters[0].Trim();
				string myLockValue = parameters[1].Trim().Replace(")", "").ToUpper();

				goGeneric = GameObject.Find(ObjectInSceneName);
				if(goGeneric != null)
				{
					SlidingDoor myDoor = goGeneric.GetComponent<SlidingDoor>();
					if(myDoor != null)
					{
						if(myLockValue == "TRUE")
						{
							myDoor.Locked = true;
						}
						else if(myLockValue == "FALSE")
						{
							myDoor.Locked = false;
						}
						else
						{
							Debug.Log("Could not parse door locked bool");
						}
					}

					// The fact that I have to do this is bad.
					// Do please remind me at some point to fix my own bad programming by making Logic Doors inherit from SlidingDoor, mmkay?
					//		-Ed
					else
					{
						lgcLogicDoor myLogicDoor = goGeneric.GetComponent<lgcLogicDoor>();
						if(myLogicDoor != null)
						{
							if(myLockValue == "TRUE")
							{
								myLogicDoor.Locked = true;
							}
							else if(myLockValue == "FALSE")
							{
								myLogicDoor.Locked = false;
							}
							else
							{
								Debug.Log("Could not parse door locked bool");
							}
						}
						else
						{
							Debug.Log("GameObject given is neither a logic door or a sliding door. Aborting lock toggle.");
						}
					}
				}
				else
				{
					Debug.Log("Could not find GameObject " + ObjectInSceneName + "!");
				}
				actionComplete = true;
				break;
	        
			case "FADEOUT":
                // Fade to black
                // SYNTAX: FadeOut(time)
                if(LongSkip)
                {
                    actionComplete = true;
                    return;
                }

                sGeneric = parameters[0].Trim().Replace(")", "");
				
				float.TryParse(sGeneric, out fGeneric);

				Camera.main.GetComponent<CamScript>().FadeOut(fGeneric);
				actionComplete = true;
				break;
            case "FADEIN":
                // Fade the screen back into view
                // SYNTAX: FadeIn(time)
                if(LongSkip) // if it's a long skip don't ever fade in
                {
                    actionComplete = true;
                    return;
                }

                sGeneric = parameters[0].Trim().Replace(")", ""); 
                float.TryParse(sGeneric, out fGeneric);

                Camera.main.GetComponent<CamScript>().FadeIn(fGeneric);
                actionComplete = true;
                break;

            case "SMARTPLACE": 
                // Place the player at a location that we already know and turn him on 
                sGeneric = parameters[0].Trim().Replace(")", "");
                 
                playerTemp = GlobalConstants.FindGameObject("Player");
                if(playerTemp == null)
                {
                    actionComplete = true;
                    Debug.Log("Could not find a player object to place");
                    return;
                }

                // Look for it in our dictionaries
                if (loadedPermanents.ContainsKey(sGeneric.ToUpper()))
                {
                    playerTemp.transform.position = loadedPermanents[sGeneric.ToUpper()].gameObject.transform.position;
                    actionComplete = true;
                    playerTemp.SetActive(true);
                    playerTemp.GetComponent<CController>().Velocity = Vector3.zero;
                    return;
                }


                // Look for it in scene
                goGeneric= GlobalConstants.FindGameObject(sGeneric);

                if (goGeneric != null)
                {
                    playerTemp.transform.position = goGeneric.transform.position; 
                    actionComplete = true;
                    playerTemp.SetActive(true);
                    playerTemp.GetComponent<CController>().Velocity = Vector3.zero;
                    return;
                }
                break; 
			case "LOADITEM":
				// Loads and saves an item into a dictionary
				// LoadItem(resourceName, sGeneric)
				string ItemName = parameters[0].Trim().Replace(")", "");
				sGeneric = parameters[1].Trim().Replace(")", "");

				// Load in the item
				Item myItem = (Resources.Load("Prefabs/Items/" + ItemName) as GameObject).GetComponent<Item>();


				if (myItem != null)
				{
					loadedItems.Add(sGeneric.ToUpper(), myItem);
					actionComplete = true;
				}
				else
				{
					Debug.Log("Couldn't load item: '" + ItemName + "', at line " + currentLine);
				}
				break; 
			case "DROPITEM":
				// Drops an item from the player's inventory
				// DropItem(ItemName)
				sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
				if(loadedItems.ContainsKey(sGeneric))
				{
					if (loadedItems[sGeneric] == null)
						Debug.Log("Requested item is null: " + sGeneric);
					else
					{
						// Find the item in the inventory and remove it
						for(int i = 0; i < GameManager.Inventory.Count; i++)
						{
							if(GameManager.Inventory[i].name == loadedItems[sGeneric].name)
							{
								GameManager.Inventory.RemoveAt(i);
								break;
							}
						}
					}
				}else
				{
					Debug.Log("Couldn't find item: " + sGeneric + " within the loaded items dictionary. Make sure it's loaded first");
				}

				actionComplete = true;
				break;

			case "GRABITEM":
				// Adds an item to the player's inventory
				// GrabItem(ItemName)
				sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
				if(loadedItems.ContainsKey(sGeneric))
				{
					if (loadedItems[sGeneric] == null)
						Debug.Log("Requested item is null: " + sGeneric);
					else
					{
						// Add the item to the game manager's list
						GameManager.Inventory.Add(loadedItems[sGeneric]);
					}
				}else
				{
					Debug.Log("Couldn't find item: " + sGeneric + " within the loaded items dictionary. Make sure it's loaded first");
				}

				actionComplete = true;
				break;

			case "LOADOBJECTIVE":
				// Loads and saves an objective into a dictionary
				// LoadObjective(resourceName, sGeneric)
				string ObjectiveName = parameters[0].Trim().Replace(")", "");
				sGeneric = parameters[1].Trim().Replace(")", "");

				// Load in the objective
				string myObjective = (Resources.Load("Objectives/" + ObjectiveName) as TextAsset).text;


				if (myObjective != null)
				{
					loadedObjectives.Add(sGeneric.ToUpper(), myObjective);
					actionComplete = true;
				}
				else
				{
					Debug.Log("Couldn't load objective: '" + ObjectiveName + "', at line " + currentLine);
				}
				break;

			case "REMOVEOBJECTIVE":
				// Removes an objective from the player's list
				// RemoveObjective(ObjectiveName)
				sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
				if(loadedObjectives.ContainsKey(sGeneric))
				{
					if (loadedObjectives[sGeneric] == null)
						Debug.Log("Requested objective is null: " + sGeneric);
					else
					{
						// Find the objective in the list and remove it
						for(int i = 0; i < GameManager.Objectives.Count; i++)
						{
							if(GameManager.Objectives[i] == loadedObjectives[sGeneric])
							{
								GameManager.Objectives.RemoveAt(i);
								break;
							}
						}

						MenuManager.instance.FlashObjectiveNotification();
					}
				}else
				{
					Debug.Log("Couldn't find objective: " + sGeneric + " within the loaded objectives dictionary. Make sure it's loaded first");
				}

				actionComplete = true;
				break;

			case "CLEAROBJECTIVES":
				// Clears the player's objectives list
				// ClearObjective()
				GameManager.Objectives.Clear();
				MenuManager.instance.FlashObjectiveNotification();

				actionComplete = true;
				break;

			case "ADDOBJECTIVE":
				// Adds an objective to the player's list
				// AddObjective(ObjectiveName)
				sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
				if(loadedObjectives.ContainsKey(sGeneric))
				{
					if (loadedObjectives[sGeneric] == null)
						Debug.Log("Requested objective is null: " + sGeneric);
					else
					{
						// Add the item to the game manager's list
						GameManager.Objectives.Add(loadedObjectives[sGeneric]);
						MenuManager.instance.FlashObjectiveNotification();
					}
				}else
				{
					Debug.Log("Couldn't find objective: " + sGeneric + " within the loaded objectives dictionary. Make sure it's loaded first");
				}

				actionComplete = true;
				break;

			case "SETEVENT":
				// Sets the chosen event to true
				// SetEvent(Key)
				sGeneric = parameters[0].Trim().Replace(")", "").ToUpper();
				if(GameManager.Events.ContainsKey(sGeneric))
				{
					GameManager.Events[sGeneric] = true;
				}
				else
				{
					Debug.Log("Couldn't find event: " + sGeneric + " within the Game Manager's Events dictionary. Make sure it's been added first, and check for typos.");
				}

				actionComplete = true;
				break;
            case "DECISIONEVENT":
                // There is a simple yes or no decision to be made
                // DecisionEvent(ID,BreakpointID1,BreakpointID2) 

                sGeneric = parameters[0].Trim().Replace(")", "");
                decisionIDTrue = parameters[1].Trim().Replace(")", "");
                decisionIDFalse = parameters[2].Trim().Replace(")", ""); 

                if(GameManager.Events.ContainsKey(sGeneric))
                { 
                    currentLine = loadedBreakpoints[(GameManager.Events[sGeneric]) ? decisionIDTrue.ToUpper() : decisionIDFalse.ToUpper()];
                }
                else
                {
                    Debug.Log("Event list does not contain a key for: " + sGeneric + ". Defaulting a false result"); 
                    currentLine = loadedBreakpoints[decisionIDFalse.ToUpper()];
                }

                actionComplete = true;
                break;
            case "BREAKPOINT":
                // Go to this breakpoint
                // Breakpoint(ID)

                sGeneric = parameters[0].Trim().Replace(")", "");
                currentLine = loadedBreakpoints[sGeneric.ToUpper()];

                actionComplete = true;
                break;

            case "START":
                // A quick start method.
                // Loads the Slas character into the character slot
                // Flips the camera
                // and halts player movement
                 
                // Load Slas
                loadedCharacters.Add("SLAS", (Resources.Load("CutsceneData/Characters/Slas") as GameObject).GetComponent<CutsceneCharacters>());

                // flip
                v3Generic = portraitArea.transform.localScale;
                v3Generic.x *= -1;
                portraitArea.transform.localScale = v3Generic;

                // Halt the player
                SetPlayerHalted(true);

                actionComplete = true;  
                break;
            case "TEXTSIZE":
                // sets the text size for the conversation
                // TextSize(int)
                sGeneric = parameters[0].Trim().Replace(")", ""); 
                int.TryParse(sGeneric, out iGeneric);

                // 264 is the standard text size. 
                // if we call 264 -> 8 then scale everything based on that
                // 120 <-> 300
                TextArea.fontSize = 120 + (18 * iGeneric);

                actionComplete = true;
                break;
			case "SETTIME":
				// Sets the time of day in the Game Manager
				// SetTime(string)
				sGeneric = parameters[0].Trim().Replace(")", "");
				GameManager.instance.ChangeTimeOfDay(sGeneric);

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

        InCutscene = true;
        
        ResetCutscene();
    }

    public void ResetCutscene()
    {
        actionComplete = true;
        ShowSide = false;
        ShowMain = false;
        Skipping = false;
        LongSkip = false;

        MainTextBox.SetActive(false);
        SideTextBox.SetActive(false);

        portraitArea.transform.localScale = new Vector3(-1, 1, 1);
        sidePortraitArea.transform.localScale = new Vector3(-1, 1, 1);

        // Reset important variables
        loadedCharacters = new Dictionary<string, CutsceneCharacters>();
        loadedPermanents = new Dictionary<string, IPermanent>();
        loadedPawns = new Dictionary<string, IPawn>();
		loadedItems = new Dictionary<string, Item>();
		loadedObjectives = new Dictionary<string, string>();
        loadedBreakpoints = new Dictionary<string, int>();
        currentCutsceneCharacter = null;
        sideTextArea.text = "";
        TextArea.text = "";
        TextArea.fontSize = 264;

        LoadBreakpoints();
        StopAllCoroutines();
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

    void LoadBreakpoints()
    {  
        string[] splitString = fullCutscene.Split('\n');
        for(int i = 0; i < splitString.Length;i++)
        {
            if (splitString[i].Length == 0)
                continue;


            // If the first char is a * then it's a breakpoint
            if(splitString[i][0] == '*' && splitString[i].Length > 1) 
                loadedBreakpoints.Add(splitString[i].Substring(1).ToUpper().Trim(), i);  
        }
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
				if(GameManager.instance.WriteSaveFile())
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
            case DecisionType.Breakpoint: 
                currentLine = loadedBreakpoints[(_decision) ? decisionIDTrue.ToUpper() : decisionIDFalse.ToUpper()]; 
                break;
		}
			
		breakOutDecision = true;
		makingADecision = false;
		actionComplete = true;

		if(LongSkip)
		{
			Camera.main.GetComponent<CamScript>().FadeOut(.5f);
		}
	}
}



 
