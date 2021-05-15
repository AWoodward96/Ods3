using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T3SpiderEncounter2 : MonoBehaviour, IPermanent {

    ZoneScript Z;

    public bool triggered;

    UsableIndicator ind_Usable;

    public BoxMover Mover;
    public SpiderBroT2Turret[] SpiderBros;


    // Use this for initialization
    void Start () {
        ind_Usable = GetComponentInChildren<UsableIndicator>();
        ind_Usable.Output = Interact;
	}
	
 
    void Interact()
    {
        StartCoroutine(Run());
        ind_Usable.Disabled = true; 
    }

    IEnumerator Run()
    {
        Mover.Triggered = true;
        yield return new WaitForSeconds(1);
        SpiderBros[0].triggered = true;
        yield return new WaitForSeconds(3);
        SpiderBros[1].triggered = true;
        yield return new WaitForSeconds(3);
        SpiderBros[2].triggered = true; 
    }
     
    public ZoneScript myZone
    {
        get
        {
            return Z;
        }

        set
        {
            Z = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return triggered;
        }

        set
        {
            triggered = value;
        }
    }

}
