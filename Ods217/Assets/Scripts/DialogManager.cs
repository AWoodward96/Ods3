﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    CController playerCC; // Because I don't want to find this mother fucker twice
    bool playerHalted; // This will be true if we've found a character to hault and we've prevented him from moving


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

        DontDestroyOnLoad(this);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        MyCanvas.enabled = InDialog;
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

    void ParseLine()
    {
        /// Function List:
        /// '$l:' - A simple line
        /// '//' - A comment, Ignored
        /// '$' - Also considered a comment, but only if nothing comes after it. Consider it a line feed for formatting
        /// '$Power?: ![ObjectName]' - States the current power of an object and asks if you want to turn it on or not
        /// '$PickUp?: ![Query]' - Item name is what you'd want to pick up. Query is what is asked of the player IE 'Would you like to pick it up?' GameObjectDestroy is the in world item to be destroyed if yes
        ///     ![ItemToAdd]
        ///     ![GameObjectDestroy]
        /// '$Face: [name]' - Changes what face is currently being shown. Will be a folder in the resources 
        /// '$HasItem:[ItemID]
        ///     ![MoveToString] - MoveTo strings are *[Identifier] and will be found via a loop. Will set the current line to the line after it

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
            Debug.Log(itemName);

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

        bool readingText = true; // Lets start an infinite loop
        while(readingText) // WEEEEEEEE
        {
            // If the string is less then the full line then keep writing
            if(currentString.Length < fullLine.Length)
            {
                // This will write the text out all scrolly n shit
                currentString = fullLine.Substring(0, currentCharacter);
                currentCharacter++;
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
        // Bleh 
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

    public void DecisionMade(bool _decision)
    {
        switch (currentDecisionType)
        {
            case DecisionType.Power:
                INonUnit nonUnit = buttonEffector[0].GetComponent<INonUnit>();
                if (_decision)
                    nonUnit.Powered = !nonUnit.Powered;

                break;
            case DecisionType.Pickup:
                if (_decision)
                {
                    // 0th index is the item to load
                    // 1st index is the object to destroy
                    GameManager.Inventory.Add(buttonEffector[0].GetComponent<Item>());
                    DestroyImmediate(buttonEffector[1]);
                }
                break;
        } 

       
        breakOutDecision = true; 
    }
}