using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A script to handle all forms of dialog and severl other interactions in the game
/// </summary>
public class DialogManager : MonoBehaviour {

    Canvas MyCanvas;
    Text TextArea;
    CustomButtonUI buttonLeft;
    CustomButtonUI buttonRight;

   
    enum DecisionType { Power, Pickup };
    DecisionType currentDecisionType;
    List<GameObject> buttonEffector; // A generic variable to determine what the decision is effecting

    public static DialogManager instance;
    public static bool InDialog;
    [TextArea(1, 100)]
    public string Dialog;

    bool makingADecision;
    bool breakOutDecision;

    // For texting purposes
    string currentString;
    public int currentLine;
    int currentCharacter;

    bool waiting;

    CController playerCC; // Because I don't want to find this mother fucker twice
    bool playerHalted; // This will be true if we've found a character to hault and we've prevented him from moving

    public AudioClip[] AudioClips;
    AudioSource mySource;

    IPawn currentPawn;

    // Use this for initialization 
    void Start() { 
        if (instance == null)
        {
            instance = this; 
        }
        else
        { 
            Destroy(this.gameObject);
        }

        InDialog = false;
        MyCanvas = GetComponentInChildren<Canvas>();
        mySource = GetComponent<AudioSource>();

        // Set up the texts
        Text[] allText = GetComponentsInChildren<Text>();
        foreach(Text t in allText)
        {
            if (t.name == "TextArea")
                TextArea = t;
        }

        CustomButtonUI[] buttons = GetComponentsInChildren<CustomButtonUI>();
        foreach(CustomButtonUI b in buttons)
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
        waiting = false;
        DontDestroyOnLoad(this);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        // enable and disable assets depending on booleans
        MyCanvas.enabled = InDialog && !waiting;
        buttonLeft.gameObject.SetActive(makingADecision);
        buttonRight.gameObject.SetActive(makingADecision);  
	}

    void Update()
    {
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

            if(Input.GetKeyDown(KeyCode.Space))
            {
                DecisionMade((CustomButtonUI.Selected == buttonLeft));
            }
          
        }
    }

    // This is a method that can be called from anywhere to show dialog
    public void ShowDialog(string _dialogString)
    {
        // set up the variables
        Dialog = _dialogString;
        currentLine = 0;
        InDialog = true; 

        // Ok turn off the player if he can be turnt off
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerHalted = false;
        if(player)
        { 
            playerCC = player.GetComponent<CController>();
            if(playerCC != null)
            {
                playerCC.canMove = false;
                playerHalted = true;
            }
        }


        // This method doesn't really do much, but it does reset the lines and call ParseLine()
        ParseLine();
    }

    // A GIANT method that goes through and determines what we're doing with each line
    void ParseLine()
    {
        /// Function List:
        /// '$l:' - A simple line
        /// '//' - A comment, Ignored
        /// '$' - Also considered a comment, but only if nothing comes after it. Consider it a line feed for formatting
        /// '$Power?: ![ObjectName]' - States the current power of an object and asks if you want to turn it on or not
        ///     [This function requires an empty line afterwords]
        /// 
        /// '$PickUp?: ![Query]' - Item name is what you'd want to pick up. Query is what is asked of the player IE 'Would you like to pick it up?' GameObjectDestroy is the in world item to be destroyed if yes
        ///     ![ItemToAdd]
        ///     ![GameObjectDestroy]
        ///     $l: [Success Prompt]
        /// '$Trigger: [ObjectName]' - Triggers whatever object is listed 
        /// '$HasItem:[ItemID]
        ///     ![MoveToString] - MoveTo strings are *[Identifier] and will be found via a loop. Will set the current line to the line after it
        /// '$l: [Any text] **[MoveToString]
        /// '$Wait:[time]' puts the dialog menu away and waits for a bit before continuing
        /// '$SetActive:[object].[true/false]' turns an object on or off
        /// 'End:' Ends the dialog


        // Get the line
        string[] splitString = Dialog.Split('\n');
        string fullLine = GetValidLine(splitString);
       
        // Our break out. If the current line number is greater then the amount of lines in the dialog then we're finished.
        if(currentLine >= splitString.Length)
        {
            // We're done!
           InDialog = false;

           if (playerHalted)
               playerCC.canMove = true;
           return;
        }
        
        currentLine++; // Increment so we never read the same line twice

        // Alright now determine what to do with it

        // Ok since we know weather or not the line is a comment, do a default check on a simple line
        if (fullLine.Contains("$l:"))
        {
            // Send it to the coroutine 
            StartCoroutine(PrintLine(fullLine.Substring(3))); 
            return;
        }

        if (fullLine[0] == '.')
        {
            handlePawnCommands(fullLine);
        }

        if(fullLine.ToUpper().Contains("POWER?:"))
        {
            // We have a decision to make
            currentDecisionType = DecisionType.Power;

            string name = fullLine.Substring(8);
            buttonEffector.Clear();
            buttonEffector.Add(GlobalConstants.FindGameObject(name)); 
            INonUnit nonUnit = buttonEffector[0].GetComponent<INonUnit>();

            // Set up the buttons
            buttonLeft.GetComponentInChildren<Text>().text = "Yes";
            buttonRight.GetComponentInChildren<Text>().text = "No";
            CustomButtonUI.Selected = buttonLeft;


            // Set up the string concatination
            makingADecision = true;
            breakOutDecision = false;
            string currently = nonUnit.Powered ? "on" : "off";
            string notcurrently = !nonUnit.Powered ? "on" : "off";
            StartCoroutine(PrintLine("The " + name + " is currently " + currently + ". Turn it " + notcurrently + "?"));

            return;
        }


        if(fullLine.ToUpper().Contains("PICKUP?"))
        {
            // The pickup name should be the item name in the resources folder
            string line = fullLine.Substring(9);
            string secondLine = splitString[currentLine]; // Second line is the item to add
            string thirdLine = splitString[currentLine + 1]; // Third line is the game object to destroy
            secondLine = secondLine.Trim(); // Trim off any spaces
            thirdLine = thirdLine.Trim();
            secondLine = secondLine.Substring(1); // Get the names perfectly
            thirdLine = thirdLine.Substring(1);

            // Get the item to load
            GameObject ItemToLoad = Resources.Load("Prefabs/Items/" + secondLine) as GameObject;

            // Set up the buttonEffector
            buttonEffector.Clear();
            buttonEffector.Add(ItemToLoad); // 0th index should be the item to load
            buttonEffector.Add(GlobalConstants.FindGameObject(thirdLine)); // 1st index should be the object to destroy

            // Set up the buttons
            buttonLeft.GetComponentInChildren<Text>().text = "Yes";
            buttonRight.GetComponentInChildren<Text>().text = "No";
            CustomButtonUI.Selected = buttonLeft;

            currentDecisionType = DecisionType.Pickup;
            makingADecision = true;
            breakOutDecision = false;
            StartCoroutine(PrintLine(line));

            return;
        }


        if(fullLine.ToUpper().Contains("HASITEM:"))
        {
            string itemName = fullLine.Substring(9,1);
            int itemID;
            int.TryParse(itemName,out itemID);
 

            string identifier = splitString[currentLine];
            identifier = identifier.Trim();
            identifier = identifier.Substring(1);

            List<Item> list = GameManager.Inventory;
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == itemID)
                {
                    GoToLine(identifier, splitString);
                    ParseLine();
                    return;
                }
            }
        }

        if(fullLine.ToUpper().Contains("TRIGGER:"))
        {
            string objectName = fullLine.Substring(9);

            GameObject obj = GlobalConstants.FindGameObject(objectName.Trim());
            INonUnit activateMe = obj.GetComponent<INonUnit>();
            if(activateMe != null)
            {
                activateMe.Triggered = !activateMe.Triggered;
            }else
            {
                Debug.Log("Dialog manager could not find object: " + objectName);
            }

            ParseLine();
            return;
        }

        if(fullLine.ToUpper().Contains("SETACTIVE:"))
        {
            // Find a game object and turn it off/on
            // SYNTAX: SetActive:[Name].[True/False];
            string lineTrimmed = fullLine.Substring(11);
            string[] commands = lineTrimmed.Split('.');
            GameObject obj = GlobalConstants.FindGameObject(commands[0]);

            if(obj != null)
            {
                string toUp = commands[1].ToUpper();
                if(toUp == "TRUE" || toUp == "FALSE")
                {
                    obj.SetActive((toUp == "TRUE") ? true : false); 
                }
                else
                {
                    Debug.Log(":DIALOG: Error at line: " + currentLine + " in dialog. Setting an objects active boolean to " + commands[1]);
                }
            }else
            {
                Debug.Log("DIALOG: Could not find object: " + commands[0]);
            }

            ParseLine();
            return;
        }

        if(fullLine.ToUpper().Contains("WAIT:"))
        {
            string[] lineSplit = fullLine.Split(':');
            int value = 0;
            int.TryParse(lineSplit[1], out value); 
            waiting = true;
            StartCoroutine(waitCRT(value));
            return;

        }

        if (fullLine.ToUpper().Contains("END:"))
        {
            currentLine = splitString.Length;
            ParseLine();
            return;
        }
    
        // Otherwise keep parsing lines
        ParseLine();
    }

    // Returns the next valid string in the dialog
    string GetValidLine(string[] _splitString)
    {
        bool valid = false;
        while(!valid)
        {
            if(currentLine >= _splitString.Length)
            {
                // We're done return nothing
                return "";
            }

            string fullLine = _splitString[currentLine];

            if (fullLine.Length < 2)
            {
                currentLine++;
                continue;
            }

            if (fullLine[0] == '/' && fullLine[1] == '/')
            {
                currentLine++;
                continue;
            }

            if(fullLine[0] == '!' || fullLine[0] == '*')
            {
                currentLine++;
                continue;
            }

            return fullLine;
        }

        return "";

    }

    IEnumerator PrintLine(string _Line)
    {
        string fullLine = _Line;
        currentCharacter = 0;
        currentString = "";

        // Set the dialog clip to the talking clip
        mySource.clip = AudioClips[0];

        bool readingText = true; // Lets start an infinite loop
        while(readingText) // WEEEEEEEE
        {
            // If the string is less then the full line then keep writing
            if(currentString.Length < fullLine.Length)
            {
                // This will write the text out all scrolly n shit
                currentString = fullLine.Substring(0, currentCharacter);
                currentCharacter++;
                mySource.Play();
                TextArea.text = currentString;

                // If we press space, before the line is complete, just complete the line. Because yeah.
                if (Input.GetKeyDown(KeyCode.Space))
                    currentCharacter = fullLine.Length;
            }

            // If we're making a decision, our break out is dependent on if the player has made a decision yet
            if(makingADecision)
            {
                if(breakOutDecision)
                {
                    // break out 
                    makingADecision = false;
                    readingText = false;
                    ParseLine();
                }
            }else if (Input.GetKeyDown(KeyCode.Space) && currentString.Length == fullLine.Length)// If the line is complete, then lets gtfo of here
            {
                // break out
                readingText = false;
                ParseLine(); 
            }

            // This makes it so we don't get an infinite loop error
            yield return null;
        }
        

        yield break;
    }

    void GoToLine(string _Marker, string[] _splitString)
    {
        // Set up the dialog to start reading at another line
        for(int i = 0; i < _splitString.Length; i++)
        {
            string line = _splitString[i];

            if (line.Length < 1)
                continue;

            line = line.Trim();

            if (line[0] == '*')
            {
                line = line.Substring(1);
                if(line == _Marker)
                {
                    currentLine = i; 
                    return;
                }
            }
        }
    }

    // This method is called from other scripts to let the dialog manager know a decision was made
    // This could probably be done better
    public void DecisionMade(bool _decision)
    {
        switch (currentDecisionType)
        {
            case DecisionType.Power:
                INonUnit nonUnit = buttonEffector[0].GetComponent<INonUnit>();
                if (_decision)
                {
                    mySource.clip = AudioClips[1];
                    mySource.Play();
                    nonUnit.Powered = !nonUnit.Powered;
                    Dialog = modifyString(currentLine + 1, Dialog, "$l: You turned the " + buttonEffector[0].name + ((nonUnit.Powered) ? ": On" : ": Off"));
                }  

                break;
            case DecisionType.Pickup:
                if (_decision)
                {
                    // 0th index is the item to load
                    // 1st index is the object to destroy
                    Item i = buttonEffector[0].GetComponent<Item>();
                    GameManager.Inventory.Add(i);
                    DestroyImmediate(buttonEffector[1]); 
                }else
                {
                    currentLine++;
                }
                break;
        } 

       
        breakOutDecision = true; 
    }

 
    string InsertString(int _lineNumber, string _fullString, string _insertedLine)
    {
        // add the inserted line into the fullstring at linenumber and return it
        List<string> splitString = _fullString.Split('\n').ToList();
        splitString.Insert(_lineNumber, _insertedLine);
        string returnedString = "";
        for(int i = 0; i < splitString.Count; i++)
        {
            returnedString += splitString[i];
        }

        return returnedString;
        
    }

    IEnumerator waitCRT(float _time)
    {
        yield return new WaitForSeconds(_time);
        waiting = false;
        ParseLine();
    }

    // Change a line in the dialog on the fly. Mostly used in automatically generated lines (EX: "x object was turned on/off")
    string modifyString(int _lineNumber, string _fullString, string _newLine)
    {
        List<string> splitString = _fullString.Split('\n').ToList();
        splitString[_lineNumber] = _newLine;
        string returnedString = "";
        for (int i = 0; i < splitString.Count; i++)
        {
            returnedString += splitString[i];
            returnedString += "\n";
        }

        return returnedString;
        
    }

    string[] parseCommand(string _Command)
    {
        string cutCommand = _Command.Substring(_Command.IndexOf('(') + 1);
        cutCommand = cutCommand.TrimEnd(')');

        string[] returned = cutCommand.Split(',');
        for(int i = 0; i < returned.Length; i++)
        {
            returned[i] = returned[i].Trim();
        }

        return returned;
    }

    void handlePawnCommands(string _fullline)
    {
        // Pawn commands 
        // Bind sets the currentPawn variable to whatever object we've found
        if(_fullline.ToUpper().Contains(".BIND"))
        {
            string[] parameters = parseCommand(_fullline);
            GameObject Object = GlobalConstants.FindGameObject(parameters[0]);

            if(Object == null)
            { 
                Debug.Log("Could not find object: " + parameters[0]); 
                return;
            }

            IPawn pawn = Object.GetComponent<IPawn>();
            if (pawn != null)
            {
                currentPawn = pawn;
            }
            else
                Debug.Log("Could not find pawnscript on object: " + parameters[0]);

            return;
        }

        if(_fullline.ToUpper().Contains(".MOVE") || _fullline.ToUpper().Contains(".WALK") || _fullline.ToUpper().Contains(".SPRINT"))
        {
            string[] parameters = parseCommand(_fullline);

            int val1, val2;
            int.TryParse(parameters[0], out val1);
            int.TryParse(parameters[1], out val2);
            currentPawn.MyCommands.Add(new PawnCommand(PawnCommand.commandType.Move, 0, 0, _fullline.ToUpper().Contains(".SPRINT"), "", new Vector3(val1, 0, val2)));
            return;
        }

        
        if(_fullline.ToUpper().Contains(".AIM") || _fullline.ToUpper().Contains(".POINT"))
        {
            string[] parameters = parseCommand(_fullline);

            int val1, val2;
            int.TryParse(parameters[0], out val1);
            int.TryParse(parameters[1], out val2);
            currentPawn.MyCommands.Add(new PawnCommand(PawnCommand.commandType.Aim, 0, 0, _fullline.ToUpper().Contains(".AIM"), "", new Vector3(val1, 0, val2)));
            return;
        }
        
    }
}
