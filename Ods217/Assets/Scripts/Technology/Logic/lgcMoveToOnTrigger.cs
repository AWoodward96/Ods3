﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// A Special Effect
/// Move any object between two positions based on the scripts state boolean
/// </summary>
public class lgcMoveToOnTrigger : MonoBehaviour, IPermanent, ISavable{

    public bool State;

    public Vector3 StatePositionTrue; // The position this object should be at when State is true
    public Vector3 StatePositionFalse; // This position this object should be at when State is false
    public float Speed;
    bool statePrev; // To check to see if we should move 
    public bool triggered; // If true, we're moving atm

	[Header("ISavable Variables")]
	public int saveID = -1;

	[HideInInspector]
	public bool saveIDSet = false;

	public int SaveID
	{
		get
		{
			return saveID;
		}
		set
		{
			saveID = value;
		}
	}

	public bool SaveIDSet
	{
		get
		{
			return saveIDSet;
		}
		set
		{
			saveIDSet = value;
		}
	}

    ZoneScript Zone;
    // Use this for initialization
    void Start () {

        if (Speed == 0)
            Speed = 4;
	}
	
	// We used FixedUpdate because it pertains to moving objects
	void FixedUpdate () {
        if(statePrev != State) 
            triggered = true;

        if(triggered)
        {
            Vector3 moveToPos = (State) ? StatePositionTrue : StatePositionFalse;
            transform.position = Vector3.Lerp(transform.position, moveToPos, Speed * Time.deltaTime);
            if(Vector3.Distance(transform.position, moveToPos) < .01)
            {
                // I don't want to continuously move every frame unless we've been triggered so reset this
                triggered = false;
            }
        }

        statePrev = State;
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
            triggered = true;
        }
    }

    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(State);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		State = bool.Parse(data[0].Trim());
	}
}
