using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxLockPrevention : MonoBehaviour, IPermanent
{ 
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
		
	void FixedUpdate()
	{
		// Check if there is any unit that is near the box
		Collider[] hits;
		hits = Physics.OverlapBox(transform.position, myBB.size, transform.rotation);
		for(int i = 0; i < hits.Length; i++)
		{
			if(hits[i].gameObject.layer != LayerMask.NameToLayer("Units"))
			{
				continue;
			}

			Vector3 pos = hits[i].transform.position;
			pos.y = transform.position.y;

			// If so, check to see if there is a wall near said unit; if so, the box needs to be broken to avoid a softlock
			RaycastHit wall;
			if(Physics.Raycast(pos, (pos - transform.position).normalized, out wall, hits[i].bounds.size.x))
			{
				if(wall.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
				{
					Triggered = true;
					break;
				}
			}
		}
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
			return isBroken;
		}
		set
		{
			if(myExplosiveBox != null)
			{
				if(!myExplosiveBox.Triggered)
				{
					myExplosiveBox.Triggered = value;
				}
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