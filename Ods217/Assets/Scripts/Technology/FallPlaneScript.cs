using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlaneScript : MonoBehaviour
{
	GameObject player;

	public int penalty;	// How much HP does the player lose for falling?

	public Vector3 restartPosition;	// Temporary fix

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update ()
	{
		
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
				player.transform.position = restartPosition;
			}
		}
	}
}
