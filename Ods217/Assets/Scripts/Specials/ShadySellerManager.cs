using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A special script for the Shady Seller in the Outpost
/// He hides offscreen until you get close and then he slowly but surely ascends from hell so you can sell him stuff for money
/// </summary>
public class ShadySellerManager : MonoBehaviour {


    GameObject player;
    public float distanceTrigger = 4;
 

    // Update is called once per frame
    void Update () {
        Vector3 pos = playerRef.transform.position;

        // if we're close enough move up
        if(GlobalConstants.ZeroYComponent(pos - transform.position).magnitude < distanceTrigger)
        {
            if(transform.localPosition.y < -1)
            { 
                transform.position += Vector3.up * .1f;
                if (transform.localPosition.y >= -1)
                    transform.localPosition = new Vector3(0, -1, .03f);
            }
        }
        else // otherwise move back down u fucking coward
        {
            if (transform.localPosition.y > -4)
            {
                transform.position += Vector3.down * .1f;
                if (transform.localPosition.y <= -4)
                    transform.localPosition = new Vector3(0, -4, .03f);
            }
        }
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
