using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcSyncTriggerWithAnimCTRL : MonoBehaviour, IPermanent {

    
    public bool State;
    Animator myAnim;
    ZoneScript Zone;

    public ZoneScript myZone
    {
        get
        {
            return Zone;
        }

        set
        {
            Zone = value;
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
            if (myAnim == null)
                myAnim = GetComponent<Animator>();

            myAnim.SetBool("State", State);
        }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }
 
 
}
