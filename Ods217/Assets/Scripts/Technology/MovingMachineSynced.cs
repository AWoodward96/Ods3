using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingMachineSynced : MonoBehaviour
{  
	// Origin position and rotation
	public Transform A;
	Vector3 posA;
	Quaternion rotA;


	// Destination position and rotation
	public Transform B;
	Vector3 posB;
	Quaternion rotB; 

	bool updates;	// Does this object currently bother moving?
	public bool useLocal;	// Should the machine use the transforms directly instead of the overrides? For making the machine move relative to something.

	GameObject player;

    [Space(30)]
    [Header("HELPER BUTTONS")]
    public bool MAKEA;
    public bool MAKEB;
    public bool SWAPREF;

    // Use this for initialization
    void Start()
	{ 
		player = GameObject.FindGameObjectWithTag("Player");

		posA = A.position;
		rotA = A.rotation;

		posB = B.position;
		rotB = B.rotation;

		updates = true;
	}

	void Update()
	{
		if( updates)
		{ 
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
			if(!useLocal)
			{
				// Lerp the platform to the position it should currently be at
				transform.position = Vector3.Lerp(posA, posB, HoldSwitchSynced.globalValue);
				transform.rotation = Quaternion.Slerp(rotA, rotB, HoldSwitchSynced.globalValue); // SLURP
			}
			else
			{
				transform.position = Vector3.Lerp(A.position, B.position, HoldSwitchSynced.globalValue);
				transform.rotation = Quaternion.Slerp(A.rotation, B.rotation, HoldSwitchSynced.globalValue);
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
		}
	}


    private void OnDrawGizmosSelected()
    {
        if (MAKEA)
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

        if(SWAPREF)
        {
            Transform TEMP = A;
            A = B;
            B = TEMP;
            SWAPREF = false;
        }
    }
}
