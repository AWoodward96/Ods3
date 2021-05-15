using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour
{
	public HoldSwitch myHoldSwitch;

	// Arbitrary positions for the piston to oscillate between
	public Vector3 positionA;
	public Vector3 positionB;

	public float cyclePos = 0.5f;	// Where in the cycle is the piston currently? In seconds.
	public float cycleSpeed;	// What's the cycle's full speed? In cycles per second.

	public bool DEBUG;

	void Update()
	{
		cyclePos += Time.deltaTime * cycleSpeed * myHoldSwitch.myValue;
		transform.position = Vector3.Lerp(positionA, positionB, (Mathf.Sin(cyclePos * 2 * Mathf.PI) + 1) / 2.0f);
	}

	void OnDrawGizmos()
	{
		if(!DEBUG)
		{
			return;
		}

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(positionA, 0.5f);
		Gizmos.DrawSphere(positionB, 0.5f);
	}
}
