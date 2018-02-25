using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxLockPrevention : MonoBehaviour
{
	GameObject player;
	Vector3 safeSpawn;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("Player");
		safeSpawn = transform.GetChild(0).position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerStay(Collider other)
	{
		if(other.gameObject == player)
		{
			Vector3 myCenter = player.transform.position;
			myCenter.y = transform.position.y;

			// If player is colliding with a box at the same time as the hitbox
			Collider[] myList = Physics.OverlapSphere(myCenter, 1.0f);
			for(int i = 0; i < myList.Length; i++)
			{
				if(myList[i].gameObject.layer == LayerMask.NameToLayer("Box") || myList[i].gameObject.tag == "ExplosiveBox")
				{
					player.transform.position = safeSpawn;
					break;
				}
			}
		}
	}
}
