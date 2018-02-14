using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBoss : GeneratorBehavior
{
	GameObject Player;

	ForceFieldScript myForceField;
	SpriteRenderer ffSprite;

	EnergyManager myEnergy;

	List<mobDroneT1> minions;
	List<bool> isAlive;

	GameObject myBlueprint;

	UsableIndicator myInteract;

	public float recoverTime;	// Time the player has to shoot before the shield starts regenerating, in seconds
	public float regenRate;		// Pace at which the shield recovers after the weak-phase is over. Purely for aesthetic.

	float currentTimer;			// The amount of time since the shield dropped.

	bool engaged;				// Once the player first shoots the generator, the fight will begin.


	// Use this for initialization
	void Start ()
	{
		engaged = false;

		myForceField = GetComponentInChildren<ForceFieldScript>();
		ffSprite = myForceField.GetComponent<SpriteRenderer>();

		myEnergy = GetComponent<EnergyManager>();

		minions = new List<mobDroneT1>();
		isAlive = new List<bool>();

		myZone = transform.parent.GetComponentInChildren<ZoneScript>();

		myBlueprint = Resources.Load("Prefabs/Enemies/MiniDroneT1") as GameObject;

		myInteract = GetComponentInChildren<UsableIndicator>();
		myInteract.Preset = UsableIndicator.usableIndcPreset.Interact;

		Player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(myUnit.CurrentHealth == 0 || ZoneScript.ActiveZone != myZone || !engaged)
		{
			return;
		}

		ffSprite.enabled = true;

		bool minionsRemain = false;
		for(int i = 0; i < minions.Count; i++)
		{
			if(!minions[i].gameObject.activeInHierarchy)
			{
				if(isAlive[i])
				{
					myForceField.RegisterHit(minions[i].MyUnit.MaxHealth);

					/*minions.RemoveAt(i);
					i--;*/

					isAlive[i] = false;
				}
				continue;
			}

			minionsRemain = true;

			minions[i].UpdateDrone();
		}

		if(!minionsRemain)
		{
			currentTimer += Time.deltaTime;
			if(currentTimer >= recoverTime)
			{
				myEnergy.RegenTime = regenRate;
			}

			if(myUnit.CurrentEnergy == myUnit.MaxEnergy)
			{
				spawnNewWave(4);
			}
		}
	}

	public override void OnHit(int _damage)
	{
		if(myUnit.CurrentHealth == 0)
		{
			return;
		}

		if(!engaged)
		{
			// If the player is shooting wildly for WHATEVER reason, we don't want them to trigger the battle unintentionally
			Vector3 myVector = Player.transform.position - transform.position;
			myVector.y = 0;
			if(myVector.sqrMagnitude > 400)
			{
				return;
			}

			engaged = true;
			myInteract.gameObject.SetActive(false);
		}

		myVisualizer.ShowMenu();
		if (myForceField != null)
		{
			if(myUnit.CurrentEnergy <= 0)
			{ 
				myUnit.CurrentHealth -= _damage;

				if(myUnit.CurrentHealth <= 0)
				{
					myUnit.CurrentHealth = 0;
					myForceField.RegisterHit(myUnit.MaxEnergy);
					myForceField.gameObject.SetActive(false);

					GameObject obj = Resources.Load("Prefabs/Particles/SmallExplosion") as GameObject;
					obj = Instantiate(obj, transform.position, obj.transform.rotation);
					obj.transform.localScale *= 4;

					GetComponent<AudioSource>().Play();
				}
			}
		} 
	}

	void spawnNewWave(int numEnemies)
	{
		myEnergy.RegenTime = 0.0f;
		currentTimer = 0.0f;

		for(int i = 0; i < numEnemies; i++)
		{
			if(minions.Count <= i)
			{
				minions.Add((Instantiate(myBlueprint) as GameObject).GetComponent<mobDroneT1>());
				isAlive.Add(true);

				minions[i].transform.parent = transform;
			}
			else
			{
				minions[i].gameObject.SetActive(true);
				isAlive[i] = true;

				minions[i].myWeapon.transform.SetParent(minions[i].transform);
				minions[i].myWeapon.RotateObject.SetActive(true);
			}
				
			minions[i].gameObject.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);

			switch(i % 4)
			{
			case 0:
				minions[i].gameObject.transform.localPosition += new Vector3(-32.0f, 0.0f, 19.0f);
				break;

			case 1:
				minions[i].gameObject.transform.localPosition += new Vector3(33.0f, 0.0f, 19.0f);
				break;

			case 2:
				minions[i].gameObject.transform.localPosition += new Vector3(33.0f, 0.0f, -7.0f);
				break;

			case 3:
				minions[i].gameObject.transform.localPosition += new Vector3(-32.0f, 0.0f, -7.0f);
				break;
			}

			minions[i].MyUnit.CurrentHealth = (int)(myUnit.MaxEnergy / numEnemies);
			minions[i].MyUnit.MaxHealth = minions[i].MyUnit.CurrentHealth;
			minions[i].myZone = myZone;
		}
	}
}
