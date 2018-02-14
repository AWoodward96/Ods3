using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
	UnitStruct myOwner;

	public float RegenTime;
	public float ChargeDelay;

	[HideInInspector]
	public float timeSinceHit;
	float regenCheck;

	[HideInInspector]
	public HealthBar myHealthBar;

	// Use this for initialization
	void Start ()
	{
		myOwner = GetComponent<IDamageable>().MyUnit;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		timeSinceHit += Time.deltaTime;
		if (timeSinceHit > ChargeDelay && myOwner.CurrentEnergy != myOwner.MaxEnergy)
		{
			ChargeEnergy();
		}
	}

	public void ExpendEnergy(int _amount)
	{
		// Subtract the appropriate amount of energy
		myOwner.CurrentEnergy -= _amount;

		if(myOwner.CurrentEnergy < 0)
		{
			myOwner.CurrentEnergy = 0;
		}

		timeSinceHit = 0;
	}

	public void ChargeEnergy()
	{
		if (myOwner.MaxEnergy <= 0)
			return;

		// RegenTime should be how much shield is regenerated per second
		regenCheck = Time.deltaTime;
		float addedHealth = regenCheck * RegenTime;
		myOwner.CurrentEnergy += (int)Mathf.Ceil(addedHealth);

		// We shouldn't ever be over full energy
		if(myOwner.CurrentEnergy >= myOwner.MaxEnergy)
		{
			myOwner.CurrentEnergy = myOwner.MaxEnergy;
		}

		if (myHealthBar != null)
		{
			myHealthBar.ShowMenu();
		}
	}
}
