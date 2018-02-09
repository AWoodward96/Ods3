using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBox : MonoBehaviour, IDamageable {

    UnitStruct myUnit;
    ZoneScript zone;
    public bool State;
    ParticleSystem mySystem;
    AudioSource src;
    public GameObject Unbroken;
    public GameObject Broken;

    BoxCollider myCol;

    Vector3 defaultSize = new Vector3(4, 4, 4);
    Vector3 defaultPosition = new Vector3(0, 2, -2);
    Vector3 brokenSize = new Vector3(4, .2f, 4);
    Vector3 brokenPosition = new Vector3(2, .1f, -2);

    // Use this for initialization
    void Start () {
        mySystem = GetComponentInChildren<ParticleSystem>();
        src = GetComponent<AudioSource>();
        myCol = GetComponent<BoxCollider>();
	}
	
 
    public UnitStruct MyUnit
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public ZoneScript myZone
    {
        get
        {
            return zone;
        }

        set
        {
            zone = value;
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
            if(State)
            {
                if (src != null)
                    src.Play();

                if(mySystem != null)
                    mySystem.Play(); 
            }

            myCol.size = (State) ? brokenSize : defaultSize;
            myCol.center = (State) ? brokenPosition : defaultPosition;

            Unbroken.SetActive(!State);
            Broken.SetActive(State);
        }
    }

    public void Activate()
    { 
    }

    public void OnHit(int _damage)
    {
        if(!Triggered)
            Triggered = true;  
    }

}
