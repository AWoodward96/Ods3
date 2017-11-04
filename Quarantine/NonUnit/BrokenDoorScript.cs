using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A NonUnit Script
/// Almost entirely for visual effects, this is a door that opens once shot a couple of times
/// </summary>
[RequireComponent(typeof(Animator))]
public class BrokenDoorScript : MonoBehaviour, INonUnit
{

    public int Health; // How many hits this can take before it opens
    public GameObject Top; // The top mesh of the door (They're sepearate objects)
    Animator myAnimator;
    Animator topAnimator;
    BoxCollider myCollider;


    // Use this for initialization
    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        myAnimator = GetComponent<Animator>();
        topAnimator = Top.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            myAnimator.SetBool("Open", true);
            topAnimator.SetBool("Open", true);
            myCollider.center = new Vector3(-1.6f, 0, 0.5f);
            myCollider.size = new Vector3(0.8f, 4, 1);
        }
    }

    // This cannot be powered
    public bool Powered
    {
        get
        { return false; }

        set { }
    }

    // This cannot be triggered
    public bool Triggered
    {
        get
        { return false; }

        set
        {  // nothing 
        }
    }

    // Emping it does nothing
    public void OnEMP()
    {

    }

    // Hitting it reduces it's health
    public void OnHit()
    {
        Health--;
    }

}
