using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A special script for the Shady Seller in the Outpost
/// He hides offscreen until you get close and then he slowly but surely ascends from hell so you can sell him stuff for money
/// </summary>
public class ChangeAnimBasedOnDistance : MonoBehaviour {

    public float distanceTrigger = 4;

    GameObject player;
    Animator myAnim;

    private void Start()
    {
        myAnim =  GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update () {
        Vector3 pos = playerRef.transform.position;

        // if we're close enough move up
        myAnim.SetBool("State", GlobalConstants.ZeroYComponent(pos - transform.position).magnitude < distanceTrigger); 
	}


    GameObject playerRef
    {
        get
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");

            return player;
        }
    }
}
