using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyProviders : MonoBehaviour, INonUnit {

    public bool isPowered;
    public bool LockOn;
    public int TimeWhenPowered;
    float timeSince;

    GameObject Timer;
    GameObject PowerBall;
    ParticleSystem myEmitter;
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
       
        Timer = transform.FindChild("Timer").gameObject;
        myAnimator = Timer.GetComponent<Animator>();
        myEmitter = GetComponentInChildren<ParticleSystem>();
        PowerBall = transform.FindChild("energyOrb").gameObject;
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

        PowerBall.gameObject.SetActive(isPowered);
        myEmitter.enableEmission = isPowered;

        if (LockOn && isPowered)
            myAnimator.SetBool("LockedOn", true);
        else
            myAnimator.SetBool("LockedOn", false);

	}
}
