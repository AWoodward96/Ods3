using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwappingRooms : MonoBehaviour
{
	// These switches will alternate moving the rooms forwards and backwards
	public HoldSwitch[] switches;

	// Lock positions
	public Vector3[] positions;
	int posIndex = 0;	// Index of the current position in the array
	int direction = 1;	// Which direction are we travelling in the array?

	public bool reversible;	// When the moving machine reaches B from A, should the switch and regression go the opposite way?
	bool updates;	// Does this object currently bother moving?

	GameObject player;

	// Use this for initialization
	void Start()
	{
		if(switches.Length == 0)
		{
			Debug.Log("No hold switches in " + name + ". The object will not function.");
		}

		player = GameObject.FindGameObjectWithTag("Player");

		posIndex = 0;

		updates = true;
	}

	void Update()
	{
		float value = switches[posIndex % switches.Length].myValue;

		if(switches[posIndex % switches.Length] != null && updates)
		{
			Vector3 posDelta = transform.position;

			// Lerp the platform to the position it should currently be at
			transform.position = Vector3.Lerp(positions[posIndex], positions[posIndex + direction], value);

			posDelta = transform.position - posDelta;

			// Keep the player on board if this object is a moving platform
			// Won't work for rotations
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

			if(closest.collider != null && closest.collider.gameObject == gameObject)
			{
				player.transform.position += posDelta;
			}

			// When finished, update posIndex and direction
			if(switches[posIndex % switches.Length].myValue >= 1.0f)
			{
				posIndex += direction;
				if(posIndex == positions.Length - 1)
				{
					direction = -1;
					StartCoroutine(Settle());
				}
				else if(posIndex == 0)
				{
					direction = 1;
					StartCoroutine(Settle());
				}
			}
		}
	}

	IEnumerator Settle()
	{
		updates = false;

		yield return new WaitForSeconds(0.1f);

		//switches[posIndex % switches.Length].myValue = 0.0f;
		//switches[posIndex % switches.Length].Invert = false;

		updates = true;
	}
}
