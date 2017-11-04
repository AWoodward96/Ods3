using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The tile objects utilized by the triningMinigame script
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class traningMinigameTile : MonoBehaviour {

    BoxCollider myBoxCollider;
    Animator myAnimator;
    public trainingMinigame MinigameParent;
    public bool State; // TRUE means that you've steped on it (RED), FALSE means that it's ready to be stepped on (BLUE)

	// Use this for initialization
	void Start () {
        myBoxCollider = GetComponent<BoxCollider>();
        myBoxCollider.isTrigger = true; // ensure the box collider is a trigger. We don't want to be blocked by it
        myAnimator = GetComponent<Animator>();
        myAnimator.SetBool("State", State);

    }
	
 
    private void OnTriggerEnter(Collider other)
    { 
        // When we collide with the player let the minigame script know
        if (!State && other.tag == "Player")
        { 
            MinigameParent.SteppedOn();
        }
    }

 

}
