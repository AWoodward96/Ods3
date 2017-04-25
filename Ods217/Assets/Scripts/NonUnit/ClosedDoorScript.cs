﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosedDoorScript : MonoBehaviour, INonUnit {

    public bool Open;
    public bool LockShut;
    public GameObject TopPart;
    public List<GameObject> PowerSystems;

    Animator myAnimator;
    Animator topAnimator;
    BoxCollider myCollider;

	// Use this for initialization
	void Start () {
        myAnimator = GetComponent<Animator>();
        topAnimator = TopPart.GetComponent<Animator>();
        myCollider = GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {
        myCollider.enabled = !Open;
        myAnimator.SetBool("Open", Open);
        topAnimator.SetBool("Open", Open);

        Open = Powered;

	}

    public void OnHit()
    {
        // Do nothing
    }

    public void OnEMP()
    {
        // Open!
        Open = true;
    }

    public bool Powered
    {
        get {
            
            // Go through the entire PowerSystems list and check to see if anything isn't powered. If there is one that isn't powered, return false
            foreach(GameObject o in PowerSystems)
            {
                INonUnit nonUnit = o.GetComponent<INonUnit>();
                if(nonUnit != null)
                {
                    if (!nonUnit.Powered)
                        return false;
                }
            }

            if (LockShut) // Just in case we want a door that can't open
                return false;

            return true; }
        set {
            // We actually do nothing with the set.
            // We want to only open when conditions are met. This will never be used in another script.
        }
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
