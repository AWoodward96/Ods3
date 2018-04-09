using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxLockPrevention : MonoBehaviour, IPermanent
{
	CharacterController player;
	BoxCollider myBB;
	ExplosiveBox myExplosiveBox;

	ZoneScript zone;
	public bool isBroken = false;

	public GameObject broken;
	public GameObject unbroken;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("Player").GetComponent<CharacterController>();
		myBB = GetComponentInChildren<BoxCollider>();
		myExplosiveBox = GetComponent<ExplosiveBox>();
	}

	// Update is called once per frame
	void Update ()
	{
		// If we're in a reasonable distance
		Vector3 playerPos = player.transform.position;
		playerPos.y = transform.position.y;

		if((playerPos - transform.position).sqrMagnitude <= Mathf.Pow((myBB.size.x + player.radius) * 2, 2))
		{
			// If player is colliding with another hitbox at the same time as this box
			RaycastHit[] hit = Physics.RaycastAll(transform.position, playerPos - transform.position, (myBB.size.x + (player.radius * 2)) + 0.0625f);
			for(int i = 0; i < hit.Length; i++)
			{
				//if(hit[i].gameObject.layer == LayerMask.NameToLayer("Box") || myList[i].gameObject.tag == "ExplosiveBox")
				if(hit[i].transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
				{
					Triggered = true;
					break;
				}
			}
		}
	}

	public void Activate()
	{

	}

	public ZoneScript myZone
	{
		get
		{
			return zone;
		}
		set
		{

		}
	}

	public bool Triggered
	{
		get
		{
			return broken;
		}
		set
		{
			if(myExplosiveBox)
			{
				myExplosiveBox.Triggered = value;
			}
			else if(value != isBroken)
			{
				unbroken.SetActive(!value);
				broken.SetActive(value);
			}

			isBroken = value;
		}
	}
}