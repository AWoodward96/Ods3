using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredElevator : MonoBehaviour, IPermanent {

	UsableIndicator ind_Interactable;
	Rigidbody myRB;

	private Vector3 Velocity;
	private bool Moving;
	private bool falling = false;

	public int FloorIndex;

	private Vector3 MoveUpVector = new Vector3(0, 10, 0);

	public GameObject[] CameraLocks;
	public GameObject[] ArrivalPoints;

	public GameObject[] Triggers;
	IPermanent[] triggerHandles;
	public bool powered;

	public GameObject[] poweredEnemies;
	JumpingSpider[] poweredHandles;

	PlayerScript myPlayer;

	ZoneScript zone;


	// Use this for initialization
	void Start () {
		triggerHandles = new IPermanent[Triggers.Length];
		for(int i = 0; i < Triggers.Length; i++)
		{
			triggerHandles[i] = Triggers[i].GetComponent<IPermanent>();
		}

		// Set up other references
		ind_Interactable = GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Output = InteractDelegate;
		ind_Interactable.gameObject.SetActive(powered);
		myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

		poweredHandles = new JumpingSpider[poweredEnemies.Length];
		for(int i = 0; i < poweredEnemies.Length; i++)
		{
			poweredHandles[i] = poweredEnemies[i].GetComponent<JumpingSpider>();
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Velocity = MoveUpVector * ((Random.Range(0, 4) - 1) / 2.0f);

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
			if(!Triggered)
			{
				for(int i = 0; i < poweredEnemies.Length; i++)
				{
					poweredHandles[i].Triggered = true;
				}
			}

			Triggered = true;
		}
		else if(Triggered)
		{
			for(int i = 0; i < poweredEnemies.Length; i++)
			{
				poweredHandles[i].Triggered = false;
			}

			Triggered = false;
		}

		if (Moving && ArrivalPoints.Length > 0)
		{ 
			if(!falling)
			{
				transform.position += Velocity * Time.deltaTime;
			}

			// Is it time to fall?
			if(Mathf.Abs(transform.position.y - ArrivalPoints[FloorIndex].transform.position.y) > 10.0f && !falling)
			{
				falling = true;
				myRB = gameObject.AddComponent<Rigidbody>();
			}

			// Check to see if we can stop moving
			if (falling && Mathf.Abs(transform.position.y - ArrivalPoints[FloorIndex].transform.position.y) > 100.0f)
			{
				myRB.velocity = Vector3.zero;
				Destroy(myRB);
				FloorIndex = (FloorIndex + 1) % ArrivalPoints.Length;
				falling = false;
				Moving = false;

				Vector3 playerDistanceVector = myPlayer.transform.position - transform.position;

				myPlayer.transform.position = ArrivalPoints[FloorIndex].transform.position + playerDistanceVector + Vector3.up;

				if (myPlayer == null)
					myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
				myPlayer.AcceptInput = true;
				Camera.main.gameObject.GetComponent<CamScript>().Target = myPlayer.transform;
			}
		}

		else if(ArrivalPoints.Length > 0)
		{
			transform.position = ArrivalPoints[FloorIndex].transform.position;
		}

	}

	void InteractDelegate()
	{
		GoToFloor((FloorIndex + 1) % ArrivalPoints.Length); 
	}


	public void GoToFloor(int _to)
	{
		if (_to == FloorIndex)
			return;

		if (!Moving) // ensure this can only be called once
		{
			Moving = true;
			Camera.main.gameObject.GetComponent<CamScript>().Target = CameraLocks[FloorIndex].transform;

			for(int i = 0; i < triggerHandles.Length; i++)
			{
				triggerHandles[i].Triggered = false;
			}
			Triggered = false;

			if (myPlayer == null)
			{
				myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
			}

			myPlayer.AcceptInput = false;
			myPlayer.GetComponent<CController>().HaltMomentum();
			//FloorIndex = _to;

			/*StopAllCoroutines();
			StartCoroutine(TravelCoroutine());*/
		}
	}

	/*IEnumerator TravelCoroutine()
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
	}*/

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
