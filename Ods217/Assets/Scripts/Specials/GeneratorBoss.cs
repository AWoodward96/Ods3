using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBoss : GeneratorBehavior
{
	GameObject Player;

	ForceFieldScript myForceField;
	SpriteRenderer ffSprite;

	List<mobDroneT1> minions;

	GameObject myBlueprint;

	public float recoverTime;	// Time the player has to shoot before the shield starts regenerating, in seconds
	public float regenRate;		// Pace at which the shield recovers after the weak-phase is over. Purely for aesthetic.

	float currentTimer;			// The amount of time since the shield dropped.

	bool engaged;				// Once the player first shoots the generator, the fight will begin.

	UsableIndicator myInteract;


	// Use this for initialization
	void Start ()
	{
		engaged = false;

		myForceField = GetComponentInChildren<ForceFieldScript>();

		ffSprite = myForceField.GetComponent<SpriteRenderer>();

		minions = new List<mobDroneT1>(6);

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
				myForceField.RegisterHit(minions[i].MyUnit.MaxHealth);

				minions.RemoveAt(i);
				i--;
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
				myForceField.RegenTime = regenRate;
			}

			if(myForceField.Health == myForceField.MaxHealth)
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
			if((Player.transform.position - transform.position).sqrMagnitude > 400)
			{
				return;
			}

			engaged = true;
			myInteract.gameObject.SetActive(false);
		}

		myVisualizer.ShowMenu();
		if (myForceField != null)
		{
			if(myForceField.Health <= 0)
			{ 
				myUnit.CurrentHealth -= _damage;

				if(myUnit.CurrentHealth <= 0)
				{
					myUnit.CurrentHealth = 0;
					myForceField.RegisterHit(myForceField.MaxHealth);
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
		myForceField.RegenTime = 0.0f;
		currentTimer = 0.0f;

		for(int i = 0; i < numEnemies; i++)
		{
			minions.Add((Instantiate(myBlueprint) as GameObject).GetComponent<mobDroneT1>());

			Vector3 myRandom = Random.onUnitSphere;
			myRandom.y = 0;
			myRandom.Normalize();
			minions[i].gameObject.transform.position = transform.position + (myRandom * 5);
			minions[i].MyUnit.CurrentHealth = (int)(myForceField.MaxHealth / numEnemies);
			minions[i].MyUnit.MaxHealth = minions[i].MyUnit.CurrentHealth;
			minions[i].myZone = myZone;

			minions[i].Activated = true;
		}
	}
}
