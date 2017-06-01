using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A NonUnit Script
/// This is a script for a console used for powering other objects.
/// When emp'd it is powered either permanently or temporarily
/// </summary>
public class EnergyProviders : MonoBehaviour, INonUnit
{

    public bool isPowered;
    public bool LockOn; // If true, when powered it will stay on forever
    public int TimeWhenPowered; // Provided lock on is false, this is how long it'll take before it is no longer powered
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

    // On Emp we power it
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
    void Start()
    {

        Timer = transform.Find("Timer").gameObject;
        myAnimator = Timer.GetComponent<Animator>();
        myEmitter = GetComponentInChildren<ParticleSystem>();
        PowerBall = transform.Find("energyOrb").gameObject; // a visual effect
    }

    // Update is called once per frame
    void Update()
    {

        if (isPowered && !LockOn)
        {
            // update the time since
            timeSince += Time.deltaTime;

            // Update the animator
            myAnimator.SetFloat("Time", TimeWhenPowered - timeSince);

            // Break out when we're done
            if (TimeWhenPowered - timeSince <= 0)
            {
                isPowered = false;
            }
        }

        
        // Enable the visual effect(s) if the object is powered
        PowerBall.gameObject.SetActive(isPowered);
        myEmitter.enableEmission = isPowered; // I'd like to stop using this but the recommended replacement doesn't work

        if (LockOn && isPowered)
            myAnimator.SetBool("LockedOn", true);
        else
            myAnimator.SetBool("LockedOn", false);

    }

    public bool Triggered
    {
        get
        { return false; }

        set
        {  // nothing 
        }
    }
}
