using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour {

    public Vector3 Move;
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position += Move;
	}
}
