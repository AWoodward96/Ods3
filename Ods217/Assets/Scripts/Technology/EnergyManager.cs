using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
	UnitStruct myOwner;

	public float RegenTime = 1.5f;
	public float ChargeDelay = 1;

	[HideInInspector]
	public float timeSinceHit;

    public bool BrokenEnergy;
	public bool canRecharge = true;

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

	public void ExpendEnergy(float _amount)
	{
		// Subtract the appropriate amount of energy
		myOwner.CurrentEnergy -= _amount;

		if(myOwner.CurrentEnergy <= 0)
		{
			myOwner.CurrentEnergy = 0;
            BrokenEnergy = true;
		}

		timeSinceHit = 0;
	}

	public void ChargeEnergy()
	{
		if (myOwner.MaxEnergy <= 0 || !canRecharge)
			return;

		// RegenTime is how long it should take to regen the full bar in seconds
		//regenCheck = Time.deltaTime;
		//float addedHealth = regenCheck * RegenTime;
		myOwner.CurrentEnergy += (myOwner.MaxEnergy / RegenTime) * Time.deltaTime;

		// We shouldn't ever be over full energy
		if(myOwner.CurrentEnergy >= myOwner.MaxEnergy)
		{
			myOwner.CurrentEnergy = myOwner.MaxEnergy;
            BrokenEnergy = false;
		}

		if (myHealthBar != null)
		{
			myHealthBar.ShowMenu();
		}
	}
}
