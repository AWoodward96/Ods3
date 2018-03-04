using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredElevator : MonoBehaviour, IPermanent {

	UsableIndicator ind_Interactable;

	private Vector3 Velocity;
	private bool Moving;
	private bool Direction; // true up, false down
	public int FloorIndex;

	private Vector3 MoveDownVector = new Vector3(0, -10, 0);
	private Vector3 MoveUpVector = new Vector3(0, 10, 0);

	public GameObject[] CameraLocks;
	public GameObject[] ArrivalPoints;

	public GameObject[] Triggers;
	IPermanent[] triggerHandles;
	public bool powered;

	PlayerScript myPlayer;

	ZoneScript zone;


	// Use this for initialization
	void Start () {
		// Set the current floor
		int closestIndex = 0;
		float smallestDistance = 500;

		triggerHandles = new IPermanent[Triggers.Length];
		for(int i = 0; i < Triggers.Length; i++)
		{
			triggerHandles[i] = Triggers[i].GetComponent<IPermanent>();
		}

		for (int i = 0; i < ArrivalPoints.Length; i++)
		{
			if (ArrivalPoints[i] != null)
			{
				float curDist = Vector3.Distance(ArrivalPoints[i].transform.position, transform.position);
				if (curDist < smallestDistance)
				{
					smallestDistance = curDist;
					closestIndex = i;
				}
			}
		}



		FloorIndex = closestIndex;

		// Set up other references
		ind_Interactable = GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Output = InteractDelegate;
		ind_Interactable.gameObject.SetActive(powered);
		myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>(); 
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Velocity = (Direction) ? MoveUpVector : MoveDownVector;

		bool done = true;
		for(int i = 0; i < triggerHandles.Length; i++)
		{
			if(!triggerHandles[i].Triggered)
			{
				done = false;
				break;
			}
		}
		if(done)
		{
			Triggered = true;
		}

		if (Moving && ArrivalPoints.Length > 0)
		{ 
			transform.position += Velocity * Time.deltaTime;

			// Check to see if we can stop moving
			if (Vector3.Distance(transform.position, ArrivalPoints[FloorIndex].transform.position) < .5f)
			{
				Moving = false;
				transform.position = ArrivalPoints[FloorIndex].transform.position;
				if (myPlayer == null)
					myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
				myPlayer.AcceptInput = true;
				Camera.main.gameObject.GetComponent<CamScript>().Target = myPlayer.transform;
			}
		}

	}

	void InteractDelegate()
	{
		GoToFloor(FloorIndex + 1); 
	}


	public void GoToFloor(int _to)
	{
		if (_to >= ArrivalPoints.Length)
			_to = 0;

		if (_to == FloorIndex)
			return;

		if (!Moving) // ensure this can only be called once
		{
			Direction = ((int)_to > (int)FloorIndex); 
			Moving = true;
			Camera.main.gameObject.GetComponent<CamScript>().Target = CameraLocks[FloorIndex].transform;

			if (myPlayer == null)
			{
				myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
			}

			myPlayer.AcceptInput = false;
			myPlayer.GetComponent<CController>().HaltMomentum();
			FloorIndex = _to;

			StopAllCoroutines();
			StartCoroutine(TravelCoroutine());
		}
	}

	IEnumerator TravelCoroutine()
	{
		yield return new WaitForSeconds(2);
		// Teleport to our destination
		// but a bit higher/lower 
		Vector3 playerDistanceVector = myPlayer.transform.position - transform.position;
		transform.position = ArrivalPoints[FloorIndex].transform.position + (Vector3.up * ((Direction) ? -40 : 40));
		myPlayer.transform.position = transform.position + playerDistanceVector + Vector3.up;
		CamScript main = Camera.main.gameObject.GetComponent<CamScript>();
		main.Target = CameraLocks[FloorIndex].transform;
		main.gameObject.transform.position = CameraLocks[FloorIndex].transform.position + GlobalConstants.DEFAULTFOLLOWBACK - (Vector3.down * 5);
	}

	public void Activate()
	{
		
	}

	public ZoneScript myZone
	{
		get{return zone;}
		set{zone = value;}
	}

	public bool Triggered
	{
		get{return powered;}

		set
		{
			powered = value;
			ind_Interactable.gameObject.SetActive(value);
		}
	}
}
