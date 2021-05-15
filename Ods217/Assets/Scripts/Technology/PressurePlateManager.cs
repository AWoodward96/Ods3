using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateManager : MonoBehaviour
{
	public GameObject permToTrigger;
	IPermanent perm;

	GameObject[] myPlates;

	bool triggered = false;

	public bool permanentTrigger = false;	// Once triggered, can the pressure plate itself be un-triggered?
	public bool triggerCutscene = false;	// Once triggered, is a cutscene played?

	void Start()
	{
		perm = permToTrigger.GetComponent<IPermanent>();
		if(perm == null)
		{
			Debug.Log("PressurePlateManager '" + name + "' has no IPermanent to trigger. It will do nothing.");
		}

		myPlates = new GameObject[transform.childCount];
		for(int i = 0; i < myPlates.Length; i++)
		{
			myPlates[i] = transform.GetChild(i).gameObject;
		}
	}

	void FixedUpdate()
	{
		if(perm != null)
		{
			Vector3 halfExtents = new Vector3();

			for(int i = 0; i < myPlates.Length; i++)
			{
				BoxCollider box = myPlates[i].GetComponent<BoxCollider>();

				halfExtents = box.size / 2.0f;
				halfExtents.x *= box.transform.localScale.x;
				halfExtents.y *= box.transform.localScale.y;
				halfExtents.z *= box.transform.localScale.z;

				RaycastHit myHit;
				if(!Physics.BoxCast(myPlates[i].transform.position, halfExtents, Vector3.up, out myHit, Quaternion.identity, 8.0f, ~LayerMask.NameToLayer("Units")))
				{
					if(!triggered)
					{
						triggered = true;

						if(triggerCutscene)
						{
							CutsceneManager.instance.StartCutscene(
								"LoadPerm(" + permToTrigger.name + ",me)\n" + "HaltPlayer()\n" +
								"CameraTarget(Me)\n" +
								"Wait(1)\n" +
								"Trigger(Me)\n" +
								"Wait(3)\n" +  
								"CameraTarget(Player)"
							);
						}
						else
						{
							perm.Triggered = !perm.Triggered;
						}
					}
				}
				else if(!permanentTrigger && triggered)
				{
					triggered = false;
					perm.Triggered = !perm.Triggered;
				}
					
				Vector3 temp = myPlates[i].transform.position;
				temp.y = (myHit.collider == null ? 0.25f : -0.20f);
				myPlates[i].transform.position = Vector3.Lerp(myPlates[i].transform.position, temp, Time.deltaTime);
			}
		}
	}
}
