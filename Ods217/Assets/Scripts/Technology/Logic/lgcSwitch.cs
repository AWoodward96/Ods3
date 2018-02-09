using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A Logic Script
/// A Simple Boolean switch system for other objects to build off of
/// </summary>
public class lgcSwitch : MonoBehaviour, IPermanent {

    ZoneScript z;
    public bool State;
    [HideInInspector]
    public UsableIndicator ind;

    public ZoneScript myZone
    {
        get
        {
            return z;
        }

        set
        {
            z = value;
        }
    }

    public virtual bool Triggered
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

    public void Activate()
    {
         
    }

    public virtual void Start()
    {
        ind = GetComponentInChildren<UsableIndicator>();
        ind.Output = () => { State = !State; };
    }
  
}
