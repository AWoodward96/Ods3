using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ssElevatorConsole : MonoBehaviour {

    public lgcGeneratorSwitch[] Switches;
    public Image[] VentImages;
    public SlidingDoor[] LockdownDoors;

    [Space(10)]
    public bool MenuOpen;
    public GameObject OverrideParent;


    [Space(10)]
    public Button StartLift;
    public Animator LiftImageAnimator;
    public Animator DoorAnimator;
    

    UsableIndicator ind_Interactable;
    PlayerScript myPlayer;
    bool startedLift = false;
    bool OnLockDown = true;


    public Canvas myCanvas;


	// Use this for initialization
	void Start () {
        // Set up other references
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
        ind_Interactable.Output = openMenuDelegate;
        myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        MenuOpen = false;


        Refresh();
    }

    // Update is called once per frame
    void Update () {
        // Escape can close the window
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWindow();
        }

        // Only show the canvas when the menu is open
        myCanvas.enabled = MenuOpen;
        LiftImageAnimator.SetBool("State", startedLift);

    }

    void openMenuDelegate()
    {
        // Open the menu
        if (!MenuOpen)
        {
            MenuOpen = true;
            MenuManager.OtherMenuOpen = MenuOpen;
            if (myPlayer.AcceptInput)
            {
                myPlayer.AcceptInput = false;
                myPlayer.GetComponent<CController>().HaltMomentum();
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
       if(myCanvas != null)
        {
            myCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            myCanvas.worldCamera = Camera.main;
            myCanvas.planeDistance = .5f;
        }
    }

    public void Refresh()
    {
        bool check = true;
        for(int i = 0; i < Switches.Length; i ++)
        {
            Animator an = VentImages[i].GetComponent<Animator>();
            an.SetBool("State", Switches[i].State);
            if (!Switches[i].State)
                check = false;
        }

        if (!startedLift)
        {
            Text t = StartLift.GetComponentInChildren<Text>();
            t.enabled = check;
            StartLift.interactable = check;
        }
    }

    public void RestartLift()
    {
        StartLift.interactable = false;
        startedLift = true;
        Text t = StartLift.GetComponentInChildren<Text>();
        t.enabled = false;

        DoorAnimator.SetBool("State", true);

    }

    public void CloseWindow()
    {
        if (MenuOpen)
        {
            myPlayer.AcceptInput = true;
            MenuOpen = false;
            MenuManager.OtherMenuOpen = false;
        }
    }

    public void OverrideLockdown()
    {
        for(int i = 0; i < LockdownDoors.Length; i ++)
        {
            LockdownDoors[i].State = true;
        }

        OverrideParent.SetActive(false); 
    }
  
}
