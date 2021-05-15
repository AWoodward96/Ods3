using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Will stay at position A unless its switch's value is > 0, at which point it will LERP to position B
public class HiddenMaze : MonoBehaviour
{
	public Vector3 positionA;
	public Vector3 positionB;

	public float speed = 1.0f;	// How fast the platform lerps from A to B, in seconds

	public HoldSwitch[] mySwitches;

	// Update is called once per frame
	void Update ()
	{
		bool active = false;
		for(int i = 0; i < mySwitches.Length; i++)
		{
			if(mySwitches[i] == null)
			{
				continue;
			}

			if(mySwitches[i].myValue > 0)
			{
				active = true;
				break;
			}
		}

		if(active)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, positionB, Time.deltaTime * speed);
		}
		else
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, positionA, Time.deltaTime * speed);
		}
	}
}
