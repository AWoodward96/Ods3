using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A custom version of the TechieT1 mob
/// Move to a console, and lock down a door
/// </summary>
public class customTechie1 : mobTechieT1 {

    [Header("Custom Techie")]
    public SlidingDoor LockMe;
    public GameObject MoveToMe;
     
    float dTime;
    bool allDone = false;

    public override void AggroState()
    {
        // The base aggroState doesn't do anything anyway
        Vector3 distV = GlobalConstants.ZeroYComponent(MoveToMe.transform.position - transform.position);

        if (distV.magnitude < .3f && !allDone)
        {  
            UpdateLookingVector = false;
            animationHandler.LookingVector = Vector3.forward; // +1 is back, -1 is forward. This makes no sense. Unity why?

            dTime += Time.deltaTime;
            if(dTime > .5f && UnitData.CurrentHealth > 0)
            {
                allDone = true;
                LockMe.Locked = true;
            }
        }
        else
        {
            UpdateLookingVector = true;
            dTime = 0; 
            if(!allDone)
                myCC.ApplyForce(distV.normalized);
        }
    }
}
