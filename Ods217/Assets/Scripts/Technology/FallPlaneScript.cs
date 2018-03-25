using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlaneScript : MonoBehaviour
{
	GameObject player;

	public int penalty;	// How much HP does the player lose for falling?

	static int numWeaponTosses = 0;
	static int maxWeaponTosses = 5;

	List<Vector3> safeSpawns;	// Safe places for the player to spawn after a fall

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindGameObjectWithTag("Player");

		safeSpawns = new List<Vector3>();

		for(int i = 0; i < transform.childCount; i++)
		{
			safeSpawns.Add(transform.GetChild(i).position);
		}
	} 
	 

	void OnTriggerExit(Collider other)
	{
		// Only needs to affect the player
		if(other.gameObject == player)
		{
			PlayerScript ps = player.GetComponent<PlayerScript>();

			ps.myUnit.CurrentHealth -= penalty;
			player.GetComponentInChildren<HealthBar>().ShowMenu();

			if(ps.myUnit.CurrentHealth > 0)
			{
				//player.transform.position = safeSpawns[currentSpawn].transform.position;

				// If there's nowhere safe to spawn, RIP player
				if(safeSpawns.Count == 0)
				{
					ps.myUnit.CurrentHealth = 0;
					return;
				}

				float distance = (player.transform.position - safeSpawns[0]).sqrMagnitude;
				int winner = 0;
				for(int i = 1; i < safeSpawns.Count; i++)
				{
					if((player.transform.position - safeSpawns[i]).sqrMagnitude < distance)
					{
						distance = (player.transform.position - safeSpawns[i]).sqrMagnitude;
						winner = i;
					}
				}

				player.transform.position = safeSpawns[winner];
			}
		}

		// ... Unless it's a weapon
		else if(other.gameObject.name == "ThrowMe")
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
					"Say(Console,Stop doing that!)"
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
