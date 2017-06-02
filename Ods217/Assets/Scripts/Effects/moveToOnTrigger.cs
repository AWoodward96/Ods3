using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Special Effect
/// Move any object between two positions based on the scripts state boolean
/// </summary>
public class moveToOnTrigger : MonoBehaviour, INonUnit{

    public bool State;

    public Vector3 StatePositionTrue; // The position this object should be at when State is true
    public Vector3 StatePositionFalse; // This position this object should be at when State is false
    public float Speed;
    bool statePrev; // To check to see if we should move
    [HideInInspector]
    public bool triggered; // If true, we're moving atm

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

    // Required dead Methods for interfaces
    bool INonUnit.Powered
    {
        get
        {
            return false;
        }

        set
        { 
        }
    }

    bool INonUnit.Triggered
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

    void INonUnit.OnEMP()
    {
        
    }

    void INonUnit.OnHit()
    {
        
    }
}
