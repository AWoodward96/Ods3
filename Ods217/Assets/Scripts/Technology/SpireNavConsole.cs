using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpireNavConsole : MonoBehaviour
{
	public float Range;
	public bool Interactable;
	public bool MenuOpen;
	public bool DescriptionOpen;

	public GameObject myConsole;
	UsableIndicator ind_Interactable;

	PlayerScript myPlayer;

	public GameObject DescriptionSection;
	public GameObject Destinations;
	public LineRenderer line;

	public int circleVertices = 3;
	public int circleRadius = 10;
	Vector3 circleCenter;

	Animator descriptionAnimator;
	RectTransform[] buttons;
	string[] descriptions;
	Vector3[] destLocations;
	Vector3[] linePositions;

	// For the line draw animation
	Vector3[] actualLinePositions;

	// Time to draw the entire line
	public float LineDrawTime;
	float AdjustedLDT;

	int curPosition;
	float timer;

	// Use this for initialization
	void Start ()
	{
		// Set up other references
		ind_Interactable = myConsole.GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
		ind_Interactable.Output = openMenuDelegate; 
		myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		MenuOpen = false;
		DescriptionOpen = false;

		descriptionAnimator = DescriptionSection.GetComponent<Animator>();
		buttons = new RectTransform[Destinations.transform.childCount - 2];
		descriptions = new string[Destinations.transform.childCount - 2];

		// 1 for the line, circleVertices for the circle, +1 to close the circle
		line.positionCount = 1 + circleVertices + 1;
		linePositions = new Vector3[line.positionCount];
		line.GetPositions(linePositions);

		actualLinePositions = new Vector3[line.positionCount];
		curPosition = 1;

		timer = 0.0f;

		for(int i = 0; i < descriptions.Length; i++)
		{
			buttons[i] = Destinations.transform.GetChild(i).GetComponent<RectTransform>();
			descriptions[i] = buttons[i].Find("Description").GetComponent<Text>().text;
		}
			
		Transform destPointContainer = Destinations.transform.Find("DestPoints");
		destLocations = new Vector3[destPointContainer.childCount];
		for(int i = 0; i < destLocations.Length; i++)
		{
			destLocations[i] = destPointContainer.GetChild(i).localPosition;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
       
        if (MenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            myPlayer.AcceptInput = true;
            MenuOpen = false;
        }

		gameObject.SetActive(MenuOpen);
        if (!MenuOpen)
		{
			DescriptionOpen = false;
			line.enabled = false;
		}

		descriptionAnimator.SetBool("Open", DescriptionOpen);

		// Update actual line positions
		for(int i = curPosition; i < line.positionCount; i++)
		{
			actualLinePositions[i] = (linePositions[curPosition - 1] * (1 - (timer / AdjustedLDT))) + (linePositions[curPosition] * (timer / AdjustedLDT));
		}


		line.SetPositions(actualLinePositions);

		timer += Time.deltaTime;
		if(timer >= AdjustedLDT)
		{
			timer = 0.0f;

			for(int i = curPosition; i < line.positionCount; i++)
			{
				actualLinePositions[i] = linePositions[curPosition];
			}

			curPosition++;

			if(curPosition < line.positionCount)
			{
				AdjustedLDT = (LineDrawTime / line.positionCount) * (linePositions[curPosition] - linePositions[curPosition - 1]).magnitude;
			}
		}
	}

	void openMenuDelegate()
    {
        if(!MenuOpen)
        {
            MenuOpen = true;
            MenuManager.OtherMenuOpen = MenuOpen;
            if (myPlayer.AcceptInput)
            {
                myPlayer.AcceptInput = false;
                myPlayer.GetComponent<CController>().HaltMomentum();
            }
				
			// Adjust based on game events
			string[] myEvents = {"OUTPOST UNLOCKED", "PIRATE SHIP UNLOCKED", "THE SKYBORN SPIRIT UNLOCKED", "LEVEL 3 UNLOCKED", "LEVEL 4 UNLOCKED"};
			for(int i = 0; i < myEvents.Length; i++)
			{
				buttons[i].GetComponent<Button>().interactable = GameManager.Events[myEvents[i]];

				if(!GameManager.Events[myEvents[i]])
				{
                    buttons[i].gameObject.SetActive(false);
				}
				else
				{
					buttons[i].transform.Find("Name").GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f);
				}
			}

			gameObject.SetActive(MenuOpen);
        }
    }

	public void HoverOverDest(int index)
	{
		// No sense in doing *anything* if we can't go there yet
		if(!buttons[index].GetComponent<Button>().interactable)
		{
			return;
		}

		DescriptionSection.transform.Find("Text").GetComponent<Text>().text = descriptions[index];
		DescriptionOpen = true;

		// Start the line draw animation! 
		line.enabled = true;
		timer = 0.0f;
		curPosition = 1;

		linePositions[0] = destLocations[destLocations.Length - 1];
		linePositions[0].y -= Destinations.transform.localPosition.y;

		circleCenter = destLocations[index];
		circleCenter.y -= Destinations.transform.localPosition.y;


        // Draw a line to indicate where in space this thing is
		for(int i = 1; i < linePositions.Length; i++)
		{
			linePositions[i] = circleCenter;
			linePositions[i].x += Mathf.Cos((i / (float)(circleVertices)) * (2 * Mathf.PI)) * circleRadius;
			linePositions[i].y += Mathf.Sin((i / (float)(circleVertices)) * (2 * Mathf.PI)) * circleRadius; 
		}

		for(int i = 0; i < actualLinePositions.Length; i++)
		{
			actualLinePositions[i] = linePositions[0];
		}

		AdjustedLDT = (LineDrawTime / line.positionCount) * (linePositions[1] - linePositions[0]).magnitude;
	}

	public void HoverOffDest()
	{
		DescriptionOpen = false;
		line.enabled = false;
	}

	public void Click(string _dest)
	{
        if (!MenuOpen)
            return;

        MenuManager.OtherMenuOpen = false;
        MenuOpen = false;

        string cutscene = "HaltPlayer()\n" +
			"Cache()\n" +
			"LoadPawn(Player,Slas)\n" +
            "LoadPerm(DockElevator,E)\n" +
            "Trigger(E)\n" +
            "Wait(3)\n" +
            "DisableObject(DockBlocker)\n" +
            "SprintPawn(Slas,0)\n" +
            "MovePawn(Slas,186,4,-10)\n" +
            "Wait(2)\n" +
            "Trigger(E)\n" +
            "FadeOut(2)\n" +
            "Wait(3)\n" +
            "MoveTo(" + _dest + ")\n" + 
            "Wait(10)";
        CutsceneManager.instance.StartCutscene(cutscene); 
	}
}
