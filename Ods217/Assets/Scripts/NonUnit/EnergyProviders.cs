using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyProviders : MonoBehaviour, INonUnit {

    public bool isPowered;
    public bool LockOn;
    public int TimeWhenPowered;
    float timeSince;

    Animator myAnimator;

    public bool Powered
    {
        get
        {
            return isPowered;
        }

        set
        {
            isPowered = value;
        }
    }

    public void OnEMP()
    {
        timeSince = 0;
        isPowered = true;
    }

    public void OnHit()
    {
       // Nothing
    }

    // Use this for initialization
    void Start () {
        myAnimator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

        if(isPowered && !LockOn)
        { 
            // update the time since
            timeSince += Time.deltaTime;

            // Update the animator
            myAnimator.SetFloat("Time",  TimeWhenPowered - timeSince);

            // Break out when we're done
            if (TimeWhenPowered - timeSince <= 0)
            {
                isPowered = false;
            }
        }

	}
}
