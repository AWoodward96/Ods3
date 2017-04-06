using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {

    Canvas MyCanvas;
    Text TextArea;
    //Button DecisionButtonL;
    //Button DecisionButtonR;


    public static DialogManager instance;
    public static bool InDialog;
    [TextArea(1, 100)]
    public string Dialog;

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

        DontDestroyOnLoad(this);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        MyCanvas.enabled = InDialog;
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
                Debug.Log("Hm");
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
        /// '$Decision: [Query]' - Starts a decision chain
        ///     '!Option1'
        ///     '!Option2'
        ///     '!Effects'
        /// '$Face: [name]' - Changes what face is currently being shown. Will be a folder in the resources 

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
        }

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

            // If the line is complete, then lets gtfo of here
            if (Input.GetKeyDown(KeyCode.Space) && currentString.Length == fullLine.Length)
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
}
