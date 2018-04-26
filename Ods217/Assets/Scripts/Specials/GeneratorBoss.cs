using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBoss : GeneratorBehavior
{
	GameObject Player;

	ForceFieldScript myForceField;
	SpriteRenderer ffSprite;

	EnergyManager myEnergy;

	List<mobDroneT0> minions;
	List<bool> isAlive;

	GameObject myBlueprint;

	UsableIndicator myInteract;

    public Transform[] SpawnPoint;
    public GameObject[] MovePoints;
    public ParticleSystem[] Explosions;
    public ParticleSystem GlassExplosion;
    public GameObject GlasObject;
    public GameObject BrokenGlassObject;
    public GameObject FiCore;


    int explosionInd = 0;

	bool engaged;				// Once the player first shoots the generator, the fight will begin.
    bool deathCrt = false;
    bool blinkingFiCore = false;

    const int NUMOFENEMIES = 6;


    // Use this for initialization
    void Start ()
	{
		engaged = false;

		myForceField = GetComponentInChildren<ForceFieldScript>();
		ffSprite = myForceField.GetComponent<SpriteRenderer>();

		myEnergy = GetComponent<EnergyManager>();

		minions = new List<mobDroneT0>();
		isAlive = new List<bool>();

		myZone = transform.parent.GetComponentInChildren<ZoneScript>();

		myBlueprint = Resources.Load("Prefabs/Enemies/MiniDroneT0") as GameObject;

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
        myForceField.fadeCRT = true;

		bool minionsRemain = false;
		for(int i = 0; i < minions.Count; i++)
		{
			if(!minions[i].gameObject.activeInHierarchy)
			{
				if(isAlive[i])
				{
					myForceField.RegisterHit(myUnit.MaxEnergy / NUMOFENEMIES); 
					isAlive[i] = false;
				}
				continue;
			}

			minionsRemain = true;

			minions[i].UpdateDrone();
		}

		if(!minionsRemain)
		{
			if(myEnergy.timeSinceHit >= myEnergy.ChargeDelay)
			{
				myEnergy.canRecharge = true;
			}

			if(myUnit.CurrentEnergy == myUnit.MaxEnergy)
			{
                Material myMat = myForceField.GetComponent<Renderer>().material; 
                myMat.SetFloat("_Edges", 0);
                myMat.SetFloat("_Level", 0);
                spawnNewWave(NUMOFENEMIES);
			}
		}
	}

	public override void OnMelee(int _damage)
	{
		if(engaged)
		{
			OnHit(_damage);
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
			if(myVector.sqrMagnitude > 625)
			{
				return;
			}

            Camera.main.GetComponent<CamScript>().AddWatch(gameObject);
			engaged = true;
			myInteract.gameObject.SetActive(false);
		}

		myVisualizer.ShowMenu();
		if (myForceField != null)
		{
			if(myUnit.CurrentEnergy <= 0)
			{ 
				myUnit.CurrentHealth -= _damage;

                if(!blinkingFiCore)
                {
                    blinkingFiCore = true;
                    StartCoroutine(blinkCore());
                } 

                // Shake the screen a bit
                Camera.main.GetComponent<CamScript>().AddEffect(CamScript.CamEffect.Shake, 1f);

                // Play a hit explosion
                Explosions[explosionInd].Play();
                Explosions[explosionInd].GetComponent<AudioSource>().Play();
                explosionInd++;
                if (explosionInd >= Explosions.Length)
                    explosionInd = 0;

				if(myUnit.CurrentHealth <= 0)
				{
					myUnit.CurrentHealth = 0;
					myForceField.RegisterHit(myUnit.MaxEnergy);
					myForceField.gameObject.SetActive(false);

                    // death coroutine
                    if(!deathCrt)
                    {
                        Camera.main.GetComponent<CamScript>().RemoveWatch(gameObject);
                        deathCrt = true;
                        StartCoroutine(deathCRT());
                    }


					// Kill all drones just in case
                    for(int i = 0; i < minions.Count; i++)
                    {
                        if(minions[i].UnitData.CurrentHealth > 0)
                            minions[i].OnHit(10);
                    }
				}
			}
		} 
	}

	void spawnNewWave(int numEnemies)
	{
		myEnergy.canRecharge = false;

        for (int i = 0; i < numEnemies; i++)
        {
            if (minions.Count <= i)
            {
                minions.Add((Instantiate(myBlueprint) as GameObject).GetComponent<mobDroneT0>());
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

            minions[i].gameObject.transform.position = new Vector3(transform.position.x, 4.0f, transform.position.z);

            switch (i % NUMOFENEMIES)
            {
                case 0:
                    minions[i].gameObject.transform.position = SpawnPoint[0].position + Vector3.down;
                    break;

                case 1:
                    minions[i].gameObject.transform.position = SpawnPoint[1].position + Vector3.down;
                    break;

                case 2:
                    minions[i].gameObject.transform.position = SpawnPoint[2].position + Vector3.down;
                    break;

                case 3:
                    minions[i].gameObject.transform.position = SpawnPoint[3].position + Vector3.down;
                    break;
                case 4:
                    minions[i].gameObject.transform.position = SpawnPoint[4].position + Vector3.down;
                    break;
                case 5:
                    minions[i].gameObject.transform.position = SpawnPoint[5].position + Vector3.down;
                    break;
            }


            GameObject properPoint = MovePoints[0];
            float smallestDistance = 100000;
            float curDistance;
            for (int j = 0; j < MovePoints.Length; j++)
            {
                curDistance = GlobalConstants.ZeroYComponent(minions[i].transform.position - MovePoints[j].transform.position).magnitude; 
                if (curDistance < smallestDistance)
                {
                    properPoint = MovePoints[j];
                    smallestDistance = curDistance;
                }
            }

 
            minions[i].MoveTarget = properPoint;
			minions[i].MyUnit.CurrentHealth = (int)(10);
			minions[i].MyUnit.MaxHealth = minions[i].MyUnit.CurrentHealth;
			minions[i].myZone = myZone;
		}
	}

    IEnumerator blinkCore()
    {
        for(int i = 0; i < 12; i ++)
        {
            FiCore.SetActive(!FiCore.activeInHierarchy);
            yield return new WaitForSeconds(.1f);
        }

        FiCore.SetActive(true);
        blinkingFiCore = false;
    }

    IEnumerator deathCRT()
    {  
        CamScript c = Camera.main.GetComponent<CamScript>(); 

        c.AddEffect(CamScript.CamEffect.Shake);
        // Go through the explosions and explode them (twice)
        for (int i = 0; i < Explosions.Length; i++)
        {
            Explosions[i].Play();
            Explosions[i].GetComponent<AudioSource>().Play();

            yield return new WaitForSeconds(.5f);

        }

        for (int i = 0; i < Explosions.Length; i++)
        {
            Explosions[i].Play();
            Explosions[i].GetComponent<AudioSource>().Play();
             
            yield return new WaitForSeconds(.5f);

        }

        // One last big one for good messure
        for (int i = 0; i < Explosions.Length; i++)
        {
            Explosions[i].Play();
            Explosions[i].GetComponent<AudioSource>().Play();

             
            if (i == 1)
            { 
                GlassExplosion.Play();
                GlasObject.SetActive(false);
                BrokenGlassObject.SetActive(true);
                FiCore.SetActive(false);
            }
            yield return new WaitForSeconds(.05f);
        }

        FindObjectOfType<DetonationScript>().StartDetonation();

    }
}
