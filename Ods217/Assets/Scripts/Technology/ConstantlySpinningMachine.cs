using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantlySpinningMachine : spin
{
	public HoldSwitch mySwitch;
	float maxSpeed;

	bool update = true;
	float myValue;

	void Start()
	{
		maxSpeed = rotationsPerMin;

		if(mySwitch == null)
		{
			Debug.Log("mySwitch is null. No effect will be added to the spin.");
		}
	}

	protected override void FixedUpdate()
	{
		if(mySwitch != null && update)
		{
			myValue = mySwitch.myValue;

			// Modify our speed based on our switch's value
			rotationsPerMin = maxSpeed * mySwitch.myValue;

			// Reverse our switch
			if((rotationsPerMin == 0 && mySwitch.Invert) || (rotationsPerMin == maxSpeed && !mySwitch.Invert))
			{
				StartCoroutine(Pause());
			}
		}

		// Spin
		base.FixedUpdate();
	}

	IEnumerator Pause()
	{
		update = false;

		yield return new WaitForSeconds(0.5f);

		update = true;
		mySwitch.myValue = myValue;
		mySwitch.Invert = !mySwitch.Invert;
	}
}
