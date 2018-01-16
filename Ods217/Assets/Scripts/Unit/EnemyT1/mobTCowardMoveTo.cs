using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mobTCowardMoveTo : mobCowardUnit
{

    public bool isTriggered; 
    UsableIndicator myusableIndicator;
    public Vector2[] MoveToAr;
    Vector3 PrevPos;
    public int MoveToIndex;
    public bool SkipAggroTime;
    public bool UseWorldSpace;
     

    public override void Start()
    {
        myusableIndicator = GetComponent<UsableIndicator>();
        PrevPos = transform.position;
        base.Start();
    }

    public override bool CheckAggro()
    {
        if (isTriggered && MoveToIndex == MoveToAr.Length - 1)
            return base.CheckAggro();
        else
            return false;
    }

    public override bool Triggered
    {
        get
        {
            return isTriggered;
        }

        set
        {
            isTriggered = value; 
        }
    }

    public override void VulnState()
    {
        if(SkipAggroTime)
        {
            base.AIState = EnemyAIState.Aggro;
            myAnimator.SetBool("Aggressive", true);
            return;
        }

        base.VulnState();
    }

    public override void IdleState()
    {
        if (SkipAggroTime)
        {
            base.AIState = EnemyAIState.Aggro;
            myAnimator.SetBool("Aggressive", true);
            return;
        }
        base.IdleState();
    }

    public void OnDrawGizmosSelected()
    {
        Vector3 prevPos = transform.position;
        Vector3 desiredPos = Vector3.zero;

        Gizmos.color = Color.red;

        if (MoveToAr.Length < 1)
            return;

        for(int i = 0; i < MoveToAr.Length; i ++)
        {
            desiredPos = new Vector3(MoveToAr[i].x, transform.position.y, MoveToAr[i].y);

            if (!UseWorldSpace)
                desiredPos = prevPos + GlobalConstants.ZeroYComponent(desiredPos);

            Gizmos.DrawLine(prevPos, desiredPos);
            prevPos = desiredPos;


            RaycastHit hit;
            Ray r = new Ray(desiredPos, Vector3.down);
            if(Physics.Raycast(r, out hit, 10, LayerMask.GetMask("Ground")))
            {
                Gizmos.DrawSphere(hit.point, .3f);
            }
        }
    }

    public override void AggroState()
    {
        if(isTriggered)
        {
            // If we still have a place to move to
            if(MoveToIndex < MoveToAr.Length)
            {
                // Move towards each point
                Vector3 desiredPos = new Vector3(MoveToAr[MoveToIndex].x, 0, MoveToAr[MoveToIndex].y);

                if (!UseWorldSpace) 
                    desiredPos = PrevPos + desiredPos;  

                // If we're close enough
                if (Vector3.Distance(GlobalConstants.ZeroYComponent(desiredPos), GlobalConstants.ZeroYComponent(transform.position)) < .5f)
                {
                    // Move it to that position
                    transform.position = desiredPos;
                    PrevPos = desiredPos;
                    // Increment the index
                    MoveToIndex++;
                    return; // No more applying forces here
                }

                myCC.ApplyForce((desiredPos - PrevPos).normalized);
                animationHandler.Update();
            }else
            {
                base.AggroState();
            }
            
        }

    }
 
}
