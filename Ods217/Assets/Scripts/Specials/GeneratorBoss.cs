using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBoss : GeneratorBehavior
{
	GameObject Player;

	ForceFieldScript myForceField;
	SpriteRenderer ffSprite;

	List<mobDroneT1> minions;
    List<bool> isAlive;

	GameObject myBlueprint;

	public float recoverTime;	// Time the player has to shoot before the shield starts regenerating, in seconds
	public float regenRate;		// Pace at which the shield recovers after the weak-phase is over. Purely for aesthetic.

	float currentTimer;			// The amount of time since the shield dropped.

	public bool engaged;				// Once the player first shoots the generator, the fight will begin.

	UsableIndicator myInteract;

    public lgcLogicDoor DoorToLock;
    public Transform[] SpawnPositions;

    const float orthSizeStart = 20;
    const float orthSizeEnd = 15;

    // Use this for initialization
    void Start ()
	{
		engaged = false;

		myForceField = GetComponentInChildren<ForceFieldScript>();

		ffSprite = myForceField.GetComponent<SpriteRenderer>();

		minions = new List<mobDroneT1>(6);
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
				myForceField.RegenTime = regenRate;
			}

			if(myForceField.MaxHealth == myForceField.Health)
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
			if((Player.transform.position - transform.position).magnitude > 400)
            { 
                return;
			}

            Camera.main.GetComponent<CamScript>().LerpSize(orthSizeStart, 1);
            engaged = true;
            DoorToLock.Locked = true;
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

                    Camera.main.GetComponent<CamScript>().LerpSize(orthSizeEnd, 1);
                    DoorToLock.Locked = false;
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
				
			minions[i].gameObject.transform.position = transform.position;

			switch(i % 4)
			{
			case 0:
                minions[i].gameObject.transform.position = SpawnPositions[0].position + Vector3.down;
				break; 
			case 1:
				minions[i].gameObject.transform.position = SpawnPositions[1].position + Vector3.down;
                    minions[i].Weapon.ResetShootCD();
                    break; 
			case 2:
				minions[i].gameObject.transform.position = SpawnPositions[2].position + Vector3.down;
                break; 
			case 3:
				minions[i].gameObject.transform.position = SpawnPositions[3].position + Vector3.down;
                    minions[i].Weapon.ResetShootCD();
                    break;
			}

            minions[i].myWeapon.StaggerShootTime();
            minions[i].MyUnit.CurrentHealth = 10;
			minions[i].MyUnit.MaxHealth = minions[i].MyUnit.CurrentHealth;
			minions[i].myZone = myZone;
		}
	}
}
