using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A simple script that will blow up boxes if Fredrick moves through them
[RequireComponent(typeof(BoxCollider))]
public class FredrickSensor : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    { 
        if(other.tag == "ExplosiveBox")
        {
            other.GetComponent<ExplosiveBox>().Triggered = true;
        }
    }
}
