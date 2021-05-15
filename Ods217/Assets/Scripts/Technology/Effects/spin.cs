using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spin : MonoBehaviour {

    public float rotationsPerMin;

	protected virtual void FixedUpdate()
    {
		transform.Rotate(0, 0, 6.0f * rotationsPerMin * Time.fixedDeltaTime);
    }
}
