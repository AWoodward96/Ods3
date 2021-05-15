using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ConveyorBelt : MonoBehaviour
{ 
    public Vector3 MoveVec;

	bool isenabled = true;
	bool reversed = false;
 
    private void OnTriggerStay(Collider other)
    {
		if (other.tag == "Player" && isenabled)
        {
            CController ctrl = other.GetComponent<CController>();
            if (ctrl != null)
			{
				Vector3 myForce = (reversed ? -MoveVec : MoveVec);
				ctrl.ApplyForce(myForce);
			}
        } 
    }

	public void ReverseDirection()
	{
		reversed = !reversed;

		// Don't need to mess with animation if the conveyor isn't enabled
		if(!isenabled)
		{
			return;
		}

		Animator[] myAnims = transform.parent.GetComponentsInChildren<Animator>();
		for(int i = 0; i < myAnims.Length; i++)
		{
			myAnims[i].SetFloat("Speed", (reversed ? -1.0f : 1.0f));
		}
	}

	public void ToggleEnabled()
	{
		isenabled = !isenabled;

		float newSpeed = 0.0f;
		if(isenabled)
		{
			newSpeed = (reversed ? -1.0f : 1.0f);
		}

		Animator[] myAnims = transform.parent.GetComponentsInChildren<Animator>();
		for(int i = 0; i < myAnims.Length; i++)
		{
			myAnims[i].SetFloat("Speed", newSpeed);
		}
	}
}
 