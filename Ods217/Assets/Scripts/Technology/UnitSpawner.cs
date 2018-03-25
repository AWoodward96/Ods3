using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour, IPermanent
{
	ZoneScript zone;

	public GameObject unitPrefab;

	public bool powered;

	List<GameObject> myUnits;
	public int maxUnits;

	public float spawnRate = 10.0f;	// Number of seconds between enemy spawns
	float timer;

	// Use this for initialization
	void Start ()
	{
		myUnits = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(!powered || !unitPrefab)
		{
			return;
		}

		timer += Time.deltaTime;

		if(timer >= spawnRate)
		{
			timer = 0.0f;

			bool unitReady = false;
			for(int i = 0; i < myUnits.Count; i++)
			{
				if(!myUnits[i].activeInHierarchy)
				{
					unitReady = true;
					myUnits[i].SetActive(true);
					myUnits[i].transform.parent = transform.parent;
					myUnits[i].transform.position = transform.position - (transform.forward * transform.localScale.z);

					IDamageable myPerm = myUnits[i].GetComponent<IDamageable>();
					if(myPerm != null)
					{
						myPerm.myZone = zone;
					}

					break;
				}
			}

			if(!unitReady && myUnits.Count < maxUnits)
			{
				myUnits.Add(GameObject.Instantiate(unitPrefab, transform.parent));
				myUnits[myUnits.Count - 1].transform.position = transform.position - (transform.forward * transform.localScale.z);

				IDamageable myPerm = myUnits[myUnits.Count - 1].GetComponent<IDamageable>();
				if(myPerm != null)
				{
					myPerm.myZone = zone;
					zone.Perms.Add(myPerm);
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
			return powered;
		}

		set
		{
			powered = value;
		}
	}

	public void Activate()
	{
		powered = !powered;
	}
}
