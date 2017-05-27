using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trainingMinigame : MonoBehaviour {

    public GameObject Prison1x1;
    public traningMinigameTile trueTile;
    public traningMinigameTile falseTile;
    public List<GameObject> Targets;

    List<moveToOnTrigger> Triggers;
    List<IUnit> Units;

    Vector3[] Positions = { new Vector3(0, 0, 1), new Vector3(0.951f, 0, 0.3089f), new Vector3(-0.951f, 0, 0.3089f), new Vector3(0.589f, 0, -0.8078f), new Vector3(-0.589f, 0, -0.8078f) };

    int levelNum = 0;
    int cycleNum = 0;

    bool levelActive;

	// Use this for initialization
	void Start () {
        Prison1x1.SetActive(false);

        // Populate the triggers array so we don't have to get it every time we want to modify the trigger values
        Triggers = new List<moveToOnTrigger>();
        Units = new List<IUnit>();
        foreach(GameObject o in Targets)
        {
            moveToOnTrigger trigger = o.GetComponent<moveToOnTrigger>();
            if (trigger)
                Triggers.Add(trigger);

            IUnit unit = o.GetComponent<IUnit>();
            if (unit != null)
                Units.Add(unit);
        }

	}
	
	// Update is called once per frame
	void Update () {
        if(levelActive)
        {
            bool isFinished = true;
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].activeInHierarchy)
                    isFinished = false;
            }

            if (isFinished)
            {

                if (levelNum > 10)
                    return;

                Vector3 _newDirection = Vector3.zero;

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
                PrepTarget(3, 4, 10);
                PrepTarget(4, 3, 10);
                break;
            default:
                PrepTarget(0, 0, 10);
                break;
        }

        levelActive = true;
        cycleNum += 1;
        if (cycleNum > 3)
            cycleNum = 0;

        levelNum++;

        
        
    }


    void PrepTarget(int _targetIndex, int _targetPositionIndex,int _targetHealth)
    {
        Vector3 rootPosition = trueTile.transform.position;
        Targets[_targetIndex].SetActive(true);
        Targets[_targetIndex].transform.position = rootPosition + (Positions[_targetPositionIndex] * 7) + Vector3.down * 2;
        Triggers[_targetIndex].StatePositionFalse = Targets[0].transform.position;
        Vector3 truePosition = Targets[_targetIndex].transform.position;
        truePosition.y = 5;
        Triggers[_targetIndex].StatePositionTrue = truePosition;
        Triggers[_targetIndex].triggered = true;
        Triggers[_targetIndex].State = true;
        Units[_targetIndex].MyUnit.CurrentHealth = _targetHealth;
        Units[_targetIndex].MyUnit.MaxHealth = _targetHealth;
    }
}
