using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effect Script
/// This script should be placed on two backgrounds that move continuously in the background of a scene
/// The object will move from OriginPoint to DestinationPoint and once reaching it, will jump back to OriginPoint
/// </summary>

public class infiniteMovingTexture : MonoBehaviour {

    public Vector3 OriginPoint;
    public Vector3 DestinationPoint;
    public float Speed = 2;

	void FixedUpdate () {
     
        if(Speed > 0)
        {
            // Check to see if we can go back yet
            if ((DestinationPoint - transform.position).magnitude < Speed)
            {
                transform.position = OriginPoint;
            }
        }
        else
        {
            if ((OriginPoint - transform.position).magnitude < -Speed)
            { 
                transform.position = DestinationPoint;
            }
        }

        // Move the object
        Vector3 direction = DestinationPoint - OriginPoint;
        direction = direction.normalized;
        transform.position += (direction * Speed);


    }
}
