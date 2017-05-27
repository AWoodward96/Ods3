using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class traningMinigameTile : MonoBehaviour {

    BoxCollider myBoxCollider;
    Animator myAnimator;
    public trainingMinigame MinigameParent;
    public bool State; // TRUE means that you've steped on it (RED), FALSE means that it's ready to be stepped on

	// Use this for initialization
	void Start () {
        myBoxCollider = GetComponent<BoxCollider>();
        myBoxCollider.isTrigger = true; // ensure the box collider is a trigger. We don't want to be blocked by it
        myAnimator = GetComponent<Animator>();
    
	}
	
	// Update is called once per frame
	void Update () {
        myAnimator.SetBool("State", State);
	}

    private void OnTriggerEnter(Collider other)
    { 
        if (!State && other.tag == "Player")
        { 
            MinigameParent.SteppedOn();
        }
    }

 

}
