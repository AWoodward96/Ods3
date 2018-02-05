using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlaneScript : MonoBehaviour
{
	GameObject player;

	public int penalty;	// How much HP does the player lose for falling?

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
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	/*private void OnDrawGizmos()
	{
		if (AlwaysShowGizmos)
			OnDrawGizmosSelected();
	}
	private void OnDrawGizmosSelected()
	{
		// Set up the zone color
		Color c = Color.blue;
		c.a = .3f;
		Gizmos.color = c;

		Vector3 zoneSize = new Vector3(ZoneSize.x, .5f, ZoneSize.y);

		// Draw a cube to represent the area of the zone
		Vector3 pos = transform.position;
		pos.y = YPosition; 
		if (type == ViewType.Wire)
			Gizmos.DrawWireCube(pos, zoneSize);

		if (type == ViewType.Solid)
			Gizmos.DrawCube(pos, zoneSize);

		// Draw all the locks
		Color c2 = Color.red ;
		c.a = .1f;
		Gizmos.color = c2;
		for (int i = 0; i < ZoneLocks.Length; i++)
		{
			Vector3 lockSize = new Vector3(ZoneLocks[i].Size.x, .5f, ZoneLocks[i].Size.y);
			Vector3 lockPosition = (transform.position + new Vector3(ZoneLocks[i].Location.x, 0, ZoneLocks[i].Location.y));


			Gizmos.DrawCube(lockPosition, lockSize);
		}


		Gizmos.DrawIcon(transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2), "TopLeftBracket");
		Gizmos.DrawIcon(transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2), "BottomRightBracket");
		Gizmos.DrawIcon(pos, "ZoneIcon");
	}*/

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
	}
}
