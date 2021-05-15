using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostElevator : MonoBehaviour
{
	bool isElevatoring;
	bool destIsMine;

	IPermanent myDoors;
	public Collider myDestination;
    public GameObject ElevatorObject;
	UsableIndicator myInd;

	PlayerScript player;

	// Use this for initialization
	void Start ()
	{
		isElevatoring = false;

		myDoors = GetComponentInChildren<IPermanent>();

		if(!myDestination)
		{
			myDestination = transform.Find("Destination").GetComponent<Collider>();
		}

		if(transform.Find("Destination") != null && myDestination == transform.Find("Destination").GetComponent<Collider>())
		{
			destIsMine = true;
		}

		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

		myInd = GetComponentInChildren<UsableIndicator>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(!myDoors.Triggered)
		{
			if(!isElevatoring)
			{
				StartCoroutine(tpPlayerTo());
			}
		}

		if(destIsMine && myDestination.bounds.Contains(player.transform.position))
		{
			if(!isElevatoring)
			{
				StartCoroutine(tpPlayerFrom());
			}
		}
	}

	IEnumerator tpPlayerTo()
	{
		isElevatoring = true;
		player.AcceptInput = false;
		myInd.Disabled = true;

		yield return new WaitForSeconds(2.0f);

		Camera.main.GetComponent<CamScript>().FadeOut(1.0f);

		yield return new WaitForSeconds(1.0f);

		Vector3 myPosition = myDestination.bounds.center;
		myPosition.z += 6.0f;
		player.transform.position = myPosition;
		myDoors.Triggered = true;
		myInd.Disabled = false;

		yield return new WaitForSeconds(1.0f);

		Camera.main.GetComponent<CamScript>().FadeIn(1.0f);

		yield return new WaitForSeconds(1.0f);

		player.AcceptInput = true;
		isElevatoring = false;
	}

    IEnumerator moveElevator(float _time, bool _direction)
    {
        Vector3 dir = (_direction) ? Vector3.up : Vector3.down;
        float dt = 0;

        while(dt < _time)
        {
            dt += Time.deltaTime;
            ElevatorObject.transform.position += dir;
            yield break;
        }
        
    }

	IEnumerator tpPlayerFrom()
	{
		isElevatoring = true;
		player.AcceptInput = false;
		myDoors.Triggered = false;
		myInd.Disabled = true;

		Camera.main.GetComponent<CamScript>().FadeOut(1.0f);

		yield return new WaitForSeconds(1.0f);

		Vector3 myPosition = transform.position;
		myPosition.y += 4.0f;
		player.transform.position = myPosition;

		yield return new WaitForSeconds(1.0f);

		Camera.main.GetComponent<CamScript>().FadeIn(1.0f);

		yield return new WaitForSeconds(2.0f);

		myDoors.Triggered = true;
		player.AcceptInput = true;
		myInd.Disabled = false;

		isElevatoring = false;
	}
}
