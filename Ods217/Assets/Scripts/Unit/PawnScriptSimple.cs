using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unit Pawn Script
/// This script is for characters who need to move during cutscenes but aren't holding weapons and don't have an aggro state
/// Mostly for non-combat related NPCs!
/// </summary>
[RequireComponent(typeof(CController))]
public class PawnScriptSimple : MonoBehaviour, IPawn {

    MovementAI moveAI;
    Animator myAnim;
    CController myCC;

    private void Start()
    {
        myCC = GetComponent<CController>();
        moveAI = new MovementAI(this.gameObject, GetComponent<CController>());
        myAnim = GetComponent<Animator>();   
    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdateAnimator();
        moveAI.Update();
	}

    void UpdateAnimator()
    {
        myAnim.SetFloat("SpeedX", myCC.Velocity.x);
        myAnim.SetFloat("SpeedY", myCC.Velocity.z);

        bool isMoving = GlobalConstants.ZeroYComponent(myCC.Velocity).magnitude > 1f;

        myAnim.SetBool("Moving", isMoving);
        myAnim.SetBool("Running", myCC.Sprinting);

        if (isMoving)
        { 
            
            myAnim.SetFloat("LookX", myCC.Velocity.x);
            myAnim.SetFloat("LookY", myCC.Velocity.z);
        } 
    }

    public void MoveTo(Vector3 _destination)
    {
        moveAI.MoveTo(_destination);
    }

    public void Look(Vector3 _look)
    {  
        myAnim.SetFloat("LookX", _look.x);
        myAnim.SetFloat("LookY", _look.z);
    }

    public void SetAggro(bool _b)
    {
        // don't do anything since there won't be a weapon attached to this pawn because it's SIMPLE not COMPLICATED
    }

    public CController cc
    {
        get { return myCC; }
    }
}
