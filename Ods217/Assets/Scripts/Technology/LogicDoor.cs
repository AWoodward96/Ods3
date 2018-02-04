using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicDoor : MonoBehaviour, IPermanent {

    public GameObject Door1;
    public GameObject Door2;
    public Vector3 DoorPos1True;
    public Vector3 DoorPos2True;

    public Vector3 DoorPos1False;
    public Vector3 DoorPos2False;

    public float Speed = 5;
    public bool State;
    public bool Locked;

    ZoneScript Zone;

	// This list will contain the scripts of every object that's needed to trigger the door (Except for things in other lists).
	public List<IPermanent> triggers;

	// This list will consist of things that are killed to open the door (everything right now)
	public List<IDamageable> harmTriggers;
      
    public ZoneScript myZone
    {
        get
        {
            return Zone;
        }

        set
        {
            Zone = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return State;
        }

        set
        {
            State = value;
        }
    }
 
	void Awake()
	{
		harmTriggers = new List<IDamageable>();
		GetComponentsInChildren<IDamageable>(harmTriggers);

		triggers = new List<IPermanent>();
		GetComponentsInChildren<IPermanent>(triggers);

		// Goofy function includes the parent itself for some reason
		triggers.RemoveAt(0);

		for(int i = 0; i < triggers.Count; i++)
		{
			// Don't want the same object being in multiple lists
			if(harmTriggers.Contains(triggers[i].gameObject.GetComponent<IDamageable>()))
			{
				triggers.RemoveAt(i);
				i--;
			}
		}
	}

    // Update is called once per frame
    void Update()
    { 
        Door1.transform.localPosition = Vector3.Lerp(Door1.transform.localPosition, (State && !Locked) ? DoorPos1True : DoorPos1False, Speed * Time.deltaTime);
        Door2.transform.localPosition = Vector3.Lerp(Door2.transform.localPosition, (State && !Locked) ? DoorPos2True : DoorPos2False, Speed * Time.deltaTime);

		// First, check and see if any harmables are dead. If so, remove them from the list!
		for(int i = 0; i < harmTriggers.Count; i++)
		{
			if(harmTriggers[i].MyUnit.CurrentHealth == 0)
			{
				harmTriggers.RemoveAt(i);
				i--;
			}
		}

		// I feel like this is bad; the old code with the generators was far more efficient, only checking the list when a generator was activated
		// However, without modifying iPermanent or making a new interface it's the best solution if the code's to be adaptable
		// (at least, that I know of)
		bool allTriggered = true;
		for(int i = 0; i < triggers.Count; i++)
		{
			// If any of the triggers aren't set off, then the door won't open.
			if(!triggers[i].Triggered)
			{
				allTriggered = false;
				break;
			}
		}

		// If all triggerables are triggered and all harmables are dead, open the door!
		if(allTriggered && harmTriggers.Count == 0)
		{
			State = true;
		}
    }
 

    public void Activate()
    {
		
    }
}
