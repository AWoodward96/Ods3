using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A NonUnit Script
/// A destructable object that can take a couple of hits before destroying itself
/// </summary>
public class RubbleScript : MonoBehaviour, INonUnit
{

    public int Health;


    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
            Destroy(this.gameObject);
    }

    // It cannot be powered
    public bool Powered
    {
        get
        { return false; }

        set { }
    }

    // Emping it does nothing
    public void OnEMP()
    {

    }

    public void OnHit()
    {
        Health--;
    }

    // Cannot be triggered
    public bool Triggered
    {
        get
        { return false; }

        set
        {  // nothing 
        }
    }

}
