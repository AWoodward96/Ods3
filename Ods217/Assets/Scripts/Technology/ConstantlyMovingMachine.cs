using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantlyMovingMachine: MonoBehaviour
{
	public lgcMachineToggleSwitch mySwitch;		// The switch used to activate this platform in particular

	float myTimer;
	public float speedMultiplier;	// How fast should this thing travel from start to finish? Default is 1 second per trip

	public Vector3 start;
	public Vector3 end;

	GameObject player;

	[Space(30)]
	[Header("HELPER BUTTONS")]
	public bool MAKESTART;
	public bool MAKEEND;

	// Use this for initialization
	void Start()
	{
		if(mySwitch == null)
		{
			mySwitch = GetComponentInChildren<lgcMachineToggleSwitch>();

			if(mySwitch == null)
			{
				Debug.Log("No switch attached to " + name + ". The object will not do anything.");
			}
		}

		player = GameObject.FindGameObjectWithTag("Player");
	}

	void Update()
	{
		bool movePlayer = true;
		if(mySwitch != null)
		{
			if(mySwitch.Triggered)
			{
				myTimer += Time.deltaTime * speedMultiplier;
				if(myTimer > 1.0f)
				{
					movePlayer = false;
					myTimer = 0.0f;
				}
			}

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

			Vector3 posDelta = transform.position;

			// Lerp the platform to the position it should currently be at
			transform.position = Vector3.Lerp(start, end, myTimer);

			posDelta = transform.position - posDelta;

			// If a wall or obstacle would block the player from moving with the platform, knock 'em off!
			if(closest.collider != null && closest.collider.gameObject == gameObject && movePlayer)
			{
				myColliders = Physics.RaycastAll(player.transform.position, posDelta, posDelta.magnitude, LayerMask.GetMask("Ground"));
				if(myColliders.Length == 0)
				{
					player.transform.position += posDelta;
				}
				else
				{
					closest = myColliders[0];
					for(int i = 0; i < myColliders.Length; i++)
					{
						if(myColliders[i].distance < closest.distance)
						{
							closest = myColliders[i];
						}
					}

					player.transform.position = closest.point;
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if(MAKESTART)
		{
			start = transform.position;
			MAKESTART = false;
		}

		if (MAKEEND)
		{
			end = transform.position;
			MAKEEND = false;
		}
	}
}

