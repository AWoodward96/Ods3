using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBehavior : MonoBehaviour, IArmed
{
	public UnitStruct myUnit;
	bool Activated;

	ZoneScript Zone;

	// Use this for initialization
	void Start()
	{
		Activated = true;
	}

	// Update is called once per frame
	void Update()
	{

	}

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    }

    public virtual void OnHit(int _damage)
	{
		if(myUnit.CurrentHealth > 0)
		{
			myVisualizer.ShowMenu();
		}

		myUnit.CurrentHealth -= _damage;
		if(myUnit.CurrentHealth <= 0)
		{
			myUnit.CurrentHealth = 0;

			if(Activated)
			{
				Activated = !Activated;

				Debug.Log(name + " deactivated");
			}
		}
	}

	public void Activate()
	{
		
	}

	public UnitStruct MyUnit
	{
		get
		{
			return myUnit;
		}
		set
		{
			myUnit = value;
		}
	}

	public ZoneScript myZone
	{
		get
		{
			return Zone;
		}
		set
		{
			Zone = value;
		}
	}

	// A logic based boolean flag
	public bool Triggered
	{
		get
		{
			return !Activated;
		}
		set
		{
			Activated = !value;
		}
	}

	public HealthBar myVisualizer
	{
		get
		{
			return GetComponentInChildren<HealthBar>();
		}
	}

	public WeaponBase myWeapon
	{
		get
		{
			// This is a cheap hack but I don't feel like writing a brand new HealthBar class just to get around it
			// I mean hey, if you want sentient, gun-toting generators in the future then i guess you have a framework now
			// You're welcome
			return null;
		}
	}
}
