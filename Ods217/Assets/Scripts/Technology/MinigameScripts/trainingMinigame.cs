using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is for running the target practice minigame
/// The levels are hardcoded but could be turned into an algorithm if I really wanted to
/// </summary>
public class trainingMinigame : MonoBehaviour {

    public GameObject Prison1x1; // An invisible prison we throw around to make sure that the player can't leave the active tile
    public traningMinigameTile trueTile; // The Red X tile that really does nothing except for move around
    public traningMinigameTile falseTile; // The Blue Square tile that when stepped on activates a new set of targets

    public List<GameObject> Targets; // A list of target objects that we throw up randomly throughout the minigame

    List<lgcMoveToOnTrigger> Triggers; // Each target should have a lgcMoveToOnTriggerScript. We get these during initialization so we don't have to get them again later
    List<IArmed> Units; // Each target has a IUnit interface script on them. We get these during initialization so we don't have to get them again

    Vector3[] Positions = { new Vector3(0, 0, 1), new Vector3(0.951f, 0, 0.3089f), new Vector3(-0.951f, 0, 0.3089f), new Vector3(0.589f, 0, -0.8078f), new Vector3(-0.589f, 0, -0.8078f) }; // Hard coded unit positions. We add these values to the root tile when setting the targets position

    int levelNum = 0; // 
    int cycleNum = 0; // We run in circles using this

    bool levelActive;

	// Use this for initialization
	void Start () {
        Prison1x1.SetActive(false);

        // Populate the triggers array so we don't have to get it every time we want to modify the trigger values
        Triggers = new List<lgcMoveToOnTrigger>();
        Units = new List<IArmed>();
        foreach(GameObject o in Targets)
        {
            lgcMoveToOnTrigger trigger = o.GetComponent<lgcMoveToOnTrigger>();
            if (trigger)
                Triggers.Add(trigger);

            IArmed unit = o.GetComponent<IArmed>();
            if (unit != null)
                Units.Add(unit);
        }

	}
	
	// Update is called once per frame
	void Update () {
        if(levelActive)
        {
            // Check to see if the player has 'killed' all the targets
            bool isFinished = true;
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].activeInHierarchy)
                    isFinished = false;
            }

            // If they have killed all the targets
            if (isFinished)
            {
                // We have no more levels after 10
                if (levelNum > 10)
                    return;

                Vector3 _newDirection = Vector3.zero;

                // Figure out where the next tile will be placed
                switch (cycleNum)
                {
                    case 0:
                        _newDirection = Vector3.forward;
                        MenuManager.instance.ShowDirectional(MenuManager.Direction.Up);
                        break;
                    case 1:
                        _newDirection = Vector3.left;
                        MenuManager.instance.ShowDirectional(MenuManager.Direction.Left);
                        break;
                    case 2:
                        _newDirection = Vector3.back;
                        MenuManager.instance.ShowDirectional(MenuManager.Direction.Down);
                        break; 
                    case 3:
                        _newDirection = Vector3.right;
                        MenuManager.instance.ShowDirectional(MenuManager.Direction.Right);
                        break;
                    default:
                        _newDirection = Vector3.right;
                        MenuManager.instance.ShowDirectional(MenuManager.Direction.Right);
                        break;
                }

                // Move the tiles and turn the barrier off so we can run to the next tile
                falseTile.transform.position = falseTile.transform.position + (_newDirection * 30);
                falseTile.GetComponent<SpriteRenderer>().enabled = true;
                Prison1x1.SetActive(false);
                levelActive = false;
            }
        }

	}

    public void SteppedOn()
    {
        // Set everything up visually
        // Restrict the players movement
        trueTile.transform.position = falseTile.transform.position;
        falseTile.GetComponent<SpriteRenderer>().enabled = false;
        Prison1x1.SetActive(true);
        Prison1x1.transform.position = trueTile.transform.position;

        
        for(int i = 0; i < Targets.Count;i++)
        {
            Targets[0].SetActive(false);
        }

        // Depending on the level number fix up some of the targets
        switch(levelNum)
        {
            // 1 target, straight up, 10 health
            case 0:
                PrepTarget(0, 0, 10);
                break;
            case 1:
                PrepTarget(0, 1, 10);
                PrepTarget(1, 2, 10);
                break;
            case 2:
                PrepTarget(0, 0, 10);
                PrepTarget(1, 1, 10);
                PrepTarget(2, 2, 10);
                break;
            case 3:
                PrepTarget(0, 0, 20);
                PrepTarget(1, 1, 10);
                PrepTarget(2, 2, 10);
                break;
            case 4:
                PrepTarget(0, 0, 10);
                PrepTarget(1, 1, 20);
                PrepTarget(2, 2, 20);
                break;
            case 5:
                PrepTarget(0, 0, 20);
                PrepTarget(1, 1, 10);
                PrepTarget(2, 2, 20);
                PrepTarget(3, 3, 20);
                break;
            case 6:
                PrepTarget(0, 0, 30);
                PrepTarget(1, 1, 20);
                PrepTarget(2, 2, 10);
                PrepTarget(3, 3, 20);
                break;
            case 7:
                PrepTarget(0, 0, 10);
                PrepTarget(1, 1, 20);
                PrepTarget(2, 2, 20);
                PrepTarget(3, 3, 30);
                PrepTarget(4, 4, 20);
                break;
            case 8:
                PrepTarget(0, 0, 30);
                PrepTarget(1, 1, 20);
                PrepTarget(2, 2, 30);
                PrepTarget(3, 3, 20);
                break;
            case 9:
                PrepTarget(0, 0, 40);
                PrepTarget(1, 1, 20);
                PrepTarget(2, 2, 20);
                PrepTarget(3, 4, 20);
                break;
            case 10:
                PrepTarget(0, 0, 40);
                PrepTarget(1, 1, 30);
                PrepTarget(2, 2, 10);
                PrepTarget(3, 4, 10); // o mix it up
                PrepTarget(4, 3, 10);
                break;
            default:
                PrepTarget(0, 0, 10);
                break;
        }

        
        levelActive = true;

        // The cycle is just to ensure we never run off the stage
        cycleNum += 1;
        if (cycleNum > 3)
            cycleNum = 0;

        levelNum++;
    }

    // The method called when we want to set up a target
    void PrepTarget(int _targetIndex, int _targetPositionIndex,int _targetHealth)
    {
        // Set the Targets position based on the position index relative to the X tile the player is now standing on
        Vector3 rootPosition = trueTile.transform.position; 
        Targets[_targetIndex].SetActive(true); // Turn the target object on
        Targets[_targetIndex].transform.position = rootPosition + (Positions[_targetPositionIndex] * 7) + Vector3.down * 2; // Move the target to the right position

        // When summoned we want the Targets to start beneath the ground and move upwards. We use the moveToOntrigger script here
        // Calculate the position we want the target to move up to
        Vector3 truePosition = Targets[_targetIndex].transform.position;
        truePosition.y = 5;

        Triggers[_targetIndex].StatePositionFalse = Targets[0].transform.position; // The state position false really isn't used but just in case we set it up here
        Triggers[_targetIndex].StatePositionTrue = truePosition;
        Triggers[_targetIndex].triggered = true; // Trigger them so they're garunteed to start moving
        Triggers[_targetIndex].State = true; // Set their state to true as well

        // Heal up the target and set it's health
        Units[_targetIndex].MyUnit.CurrentHealth = _targetHealth;
        Units[_targetIndex].MyUnit.MaxHealth = _targetHealth;
    }
}
