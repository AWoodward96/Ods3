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

    ParticleSystem Part;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("Player").GetComponent<CharacterController>();
		myBB = GetComponentInChildren<BoxCollider>();
		myExplosiveBox = GetComponent<ExplosiveBox>();
        Part = GetComponentInChildren<ParticleSystem>();

		if(broken == null)
		{
			broken = transform.Find("Broken").gameObject;
		}

		if(unbroken == null)
		{
			unbroken = transform.Find("Unbroken").gameObject;
		}

		Triggered = false;
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

		// Check enemies, too!
		if(zone == null)
		{
			return;
		}

		for(int i = 0; i < zone.Perms.Count; i++)
		{
			if(zone.Perms[i].gameObject.activeInHierarchy && zone.Perms[i].gameObject.layer == LayerMask.NameToLayer("Units"))
			{
				Collider currentUnit = zone.Perms[i].gameObject.GetComponent<Collider>();
				Vector3 unitPos = currentUnit.transform.position;
				unitPos.y = transform.position.y;

				if((unitPos - transform.position).sqrMagnitude <= Mathf.Pow((myBB.size.x + currentUnit.bounds.size.z) * 2, 2))
				{
					// If player is colliding with another hitbox at the same time as this box
					RaycastHit[] hit = Physics.RaycastAll(transform.position, unitPos - transform.position, (myBB.size.x + (currentUnit.bounds.size.z * 2)) + 0.0625f);
					for(int j = 0; j < hit.Length; j++)
					{
						//if(hit[i].gameObject.layer == LayerMask.NameToLayer("Box") || myList[i].gameObject.tag == "ExplosiveBox")
						if(hit[j].transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
						{
							Triggered = true; 
							break;
						}
					}
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
			zone = value;
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

            else if (value != isBroken && unbroken != null && broken != null)
            {
				if(Part != null)
				{
                	Part.Play();
				}
                unbroken.SetActive(!value);
                broken.SetActive(value);
            }

			isBroken = value;
		}
	}
}