using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingMachine : MonoBehaviour
{
	public HoldSwitch fwdSwitch;
	public HoldSwitch backSwitch;	// Not required; used for when you want a separate switch to move an object backwards

	// Origin position and rotation
	public Transform A;
	Vector3 posA;
	Quaternion rotA;


	// Destination position and rotation
	public Transform B;
	Vector3 posB;
	Quaternion rotB;

	public bool reversible;	// When the moving machine reaches B from A, should the switch and regression go the opposite way?
	public bool useLocal = false;	// Should the machine use the transforms directly instead of the overrides? For making the machine move relative to something.
	bool updates;	// Does this object currently bother moving?

	public float pauseLength = 0.1f;	// How long does this object pause for if reversing?

	float myValue = 0.0f;	// Local copy of myValue, so the machine doesn't *have* to be tied to its switch's value

	// Is this machine's functionality directly tied to its switch, or is it indirectly related?
	public enum MachineType { Direct, Indirect };
	public MachineType type = MachineType.Direct;
	public bool advanceOnFullOnly;	// If indirect, does this machine advance if its switch's value is > 0, or only if it's == 1?
	public float advanceSpeed;		// If indirect, the speed this machine advances from A to B, in full distances per second.
	public float regressSpeed;		// If indirect, the speed this machine regresses from B to A, in full distances per second.

	GameObject player;

    [Space(30)]
    [Header("HELPER BUTTONS")]
    public bool MAKEA;
    public bool MAKEB;

	// Use this for initialization
	void Start()
	{
		if(fwdSwitch == null)
		{
			Debug.Log("No HoldSwitch attached to " + name + ". The object will not do anything.");
		}

		player = GameObject.FindGameObjectWithTag("Player");

		posA = A.position;
		rotA = A.rotation;

		posB = B.position;
		rotB = B.rotation;

		updates = true;
	}

	void Update()
	{
		if(fwdSwitch != null && updates)
		{
			// If we have a back switch, we need to make it so that it can make the machine move backwards, despite the forward switch's value
			if(backSwitch != null)
			{
				fwdSwitch.myValue = Mathf.MoveTowards(fwdSwitch.myValue, 0.0f, backSwitch.myValue);
				backSwitch.myValue = 0.0f;
			}

			// If this machine is directly tied to its switch, then its value should never differ
			if(type == MachineType.Direct)
			{
				myValue = fwdSwitch.myValue;
			}

			// Otherwise, modify myValue based on fwdSwitch's value and our bools
			else
			{
				if(fwdSwitch.myValue <= 0.0f)
				{
					myValue = Mathf.MoveTowards(myValue, 0.0f, Time.deltaTime * regressSpeed);
				}

				else if((advanceOnFullOnly && fwdSwitch.myValue >= 1.0f) || (!advanceOnFullOnly && fwdSwitch.myValue > 0.0f))
				{
					myValue = Mathf.MoveTowards(myValue, 1.0f, Time.deltaTime * advanceSpeed);
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

			if(!useLocal)
			{
				// Lerp the platform to the position it should currently be at
				transform.position = Vector3.Lerp(posA, posB, myValue);
				transform.rotation = Quaternion.Slerp(rotA, rotB, myValue);
			}
			else
			{
				transform.position = Vector3.Lerp(A.position, B.position, myValue);
				transform.rotation = Quaternion.Slerp(A.rotation, B.rotation, myValue);
			}

			posDelta = transform.position - posDelta;

			if(closest.collider != null && closest.collider.gameObject == gameObject)
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

			// If the object is reversible, switch the origin and destination and set the value to 0
			// All this does is make the switches and regression work in the opposite direction than they did
			// Also, pause the object for a second before letting the player move it again, though that might not be necessary?
			if(reversible && myValue >= 1.0f)
			{
				StartCoroutine(Settle());
			}
		}
	}

	IEnumerator Settle()
	{
		updates = false;

		if(!useLocal)
		{
			Vector3 posT;

			posT = posA;
			posA = posB;
			posB = posT;

			Quaternion rotT;

			rotT = rotA;
			rotA = rotB;
			rotB = rotT;
		}

		else
		{
			Transform T;

			T = A;
			A = B;
			B = T;
		}

		yield return new WaitForSeconds(pauseLength);

		fwdSwitch.myValue = 0.0f;
		myValue = 0.0f;

		updates = true;
	}

    private void OnDrawGizmosSelected()
    {
        if(MAKEA)
        {
            GameObject g = new GameObject(gameObject.name + "A");
            g.transform.parent = this.transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.parent = null;
            A = g.transform;
            MAKEA = false;
        }

        if (MAKEB)
        {
            GameObject g = new GameObject(gameObject.name + "B");
            g.transform.parent = this.transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.parent = null;
            B = g.transform;
            MAKEB = false;
        }
    }
}
