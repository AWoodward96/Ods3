using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sittingPirates : MonoBehaviour, IPermanent {

    public GameObject[] Units;
    public lgcMoveToOnTrigger TriggerMe;
    public csStartOnCollision CutsceneTrigger;
    bool broken = false;

    bool t = false;
    ZoneScript z;

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

    public bool Triggered
    {
        get
        {
            return t;
        }

        set
        {
            t = value;
        }
    }

    // Update is called once per frame
    void Update () {
        if(!broken)
        {
            for (int i = 0; i < Units.Length; i++)
            {
                mobCowardUnit c = Units[i].GetComponent<mobCowardUnit>();
                if (c.AIState == AIStandardUnit.EnemyAIState.Vulnerable || c.AIState == AIStandardUnit.EnemyAIState.Aggro)
                {
                    DisableCutscene();
                    return;
                }
            } 
        }
		
	}

    void DisableCutscene()
    {
        if (t)
            return;

        broken = true;
        TriggerMe.triggered = true;
        CutsceneTrigger.gameObject.SetActive(false);
        for (int i = 0; i < Units.Length; i++)
        {
            mobCowardUnit c = Units[i].GetComponent<mobCowardUnit>();
            c.AIState = AIStandardUnit.EnemyAIState.Vulnerable;
            c.Triggered = true;
        }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }
}
