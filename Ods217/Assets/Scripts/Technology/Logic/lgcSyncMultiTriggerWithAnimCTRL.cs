using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcSyncMultiTriggerWithAnimCTRL : MonoBehaviour, IPermanent {

    
    public int State = 0;
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
            return false;
        }

        set
        {
            State++;
            if (myAnim == null)
                myAnim = GetComponent<Animator>();

            myAnim.SetInteger("State", State);
        }
    }
}
