using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatforms : MonoBehaviour, IPermanent {

	public bool isCyclic;
	public bool isPlatform;

	public Vector3[] positions;
	int currentWP;

	public float Speed = 5;

	ZoneScript zone;
	public bool isActive;

	BoxCollider BBox;
	GameObject player;

	// Use this for initialization
	void Start ()
	{
		currentWP = 0;
		BBox = GetComponent<BoxCollider>();
		player = GameObject.FindWithTag("Player");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(isCyclic)
		{
			if(isActive)
			{
				Vector3 dst = positions[currentWP] - transform.localPosition;
				if(dst.sqrMagnitude < Mathf.Pow(Speed, 2))
				{
					transform.localPosition = positions[currentWP];
					currentWP = (currentWP + 1) % positions.Length;
				}

				transform.localPosition += (positions[currentWP] - transform.localPosition).normalized * Speed;
			}
		}
		else
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, (isActive) ? positions[1] : positions[0], Speed * Time.deltaTime);
		}

		RaycastHit[] myColliders = Physics.RaycastAll(player.transform.position, Vector3.down, 10.0f, LayerMask.GetMask("Ground"));
		RaycastHit closest = new RaycastHit();
		if(myColliders.Length > 0)
		{
			closest = myColliders[0];
		}
		for(int i = 0; i < myColliders.Length; i++)
		{
			if(myColliders[i].distance < closest.distance)
			{
				closest = myColliders[i];
			}
		}

		if(closest.collider == BBox)
		{
			if(isCyclic)
			{
				if(isActive)
				{
					player.transform.position += transform.parent.TransformDirection((positions[currentWP] - transform.localPosition).normalized * Speed);
				}
			}
			else
			{
				player.transform.position += Vector3.Lerp(transform.localPosition, (isActive) ? positions[1] : positions[0], Speed * Time.deltaTime) - transform.localPosition;
			}
		}
	}

	public void Activate()
	{
		
	}

	public ZoneScript myZone
	{
		get{return zone;}
		set{zone = value;}
	}

	// A logic based boolean flag
	public bool Triggered
	{
		get{return isActive;}
		set{isActive = value;}
	} 
}
