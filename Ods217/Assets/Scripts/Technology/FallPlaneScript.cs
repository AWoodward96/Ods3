using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlaneScript : MonoBehaviour
{
	GameObject player;

	public int penalty;	// How much HP does the player lose for falling?

	static int numWeaponTosses = 0;
	static int maxWeaponTosses = 5;

	int currentBox = 0;	// Which spawn point is the player located at?

	List<Collider> spawnBoxes;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindGameObjectWithTag("Player");

		spawnBoxes = new List<Collider>();

		GetComponentsInChildren<Collider>(spawnBoxes);
	} 

	void Update()
	{
		// i = 1 to ignore the fall plane's hitbox
		for(int i = 1; i < spawnBoxes.Count; i++)
		{
			if(spawnBoxes[i].bounds.Contains(player.transform.position))
			{
				currentBox = i;
				break;
			}
		}
	}
	 

	void OnTriggerExit(Collider other)
	{
		// Only needs to affect the player
		if(other.gameObject == player)
		{
			PlayerScript ps = player.GetComponent<PlayerScript>();

            player.GetComponentInChildren<ForceFieldScript>().RegisterHit(ps.myUnit.MaxEnergy); 
			ps.myVisualizer.ShowMenu();

			if(ps.myUnit.CurrentHealth > 0)
			{
				//player.transform.position = safeSpawns[currentSpawn].transform.position;

				// If there's nowhere safe to spawn, RIP player
				if(spawnBoxes.Count == 0)
				{
					ps.myUnit.CurrentHealth = 0;
					return;
				}

				player.transform.position = spawnBoxes[currentBox].bounds.center;
			}
		}

		// ... Unless it's a weapon
		else if(other.gameObject.name == "ThrowMe" || other.gameObject.name == "ThrowGun")
		{
			other.gameObject.transform.position = player.transform.position;
			other.gameObject.transform.position += new Vector3(0.0f, 50.0f, 0.0f);

			other.attachedRigidbody.velocity = new Vector3(0.0f, other.attachedRigidbody.velocity.y, 0.0f);
			numWeaponTosses++;

			if(numWeaponTosses >= maxWeaponTosses)
			{
				// Fun easter egg?
				CutsceneManager.instance.StartCutscene
				(
					"HaltPlayer()\n" +
					"LoadChar(Con1,Console)\n" +
                    "Say(Console,Hey!)\n"+ 
                    "Continue(Console,Stop throwing your weapons off cliffs!)"
				);

				numWeaponTosses = 0;
			}
		}

		// Or an enemy unit
		else if(other.GetComponent<IDamageable>() != null)
		{
			other.gameObject.SetActive(false);
		}
	}
}
