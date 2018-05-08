using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardElevator : MonoBehaviour, IPermanent {


    public float Range;
    public bool Interactable; 
    UsableIndicator ind_Interactable;
 
    private Vector3 Velocity;
    private bool Moving;
    private bool Direction; // true up, false down
    public int FloorIndex;

    private Vector3 MoveDownVector = new Vector3(0, -10, 0);
    private Vector3 MoveUpVector = new Vector3(0, 10, 0);

    public GameObject[] CameraLocks;
    public GameObject[] ArrivalPoints;

    PlayerScript myPlayer;
    AudioSource mySource;

    public AudioClip audioMove;
    public AudioClip audioStop;

	ZoneScript zone;


    // Use this for initialization
    void Start () {
        // Set the current floor
        int closestIndex = 0;
        float smallestDistance = 500;

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

        mySource = GetComponent<AudioSource>();

        // Set up other references
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Output = InteractDelegate;
        myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>(); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Velocity = (Direction) ? MoveUpVector : MoveDownVector;


        if (Moving)
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


                // Play the stop sound when the elevator stops moving
                if(mySource != null && audioStop != null)
                { 
                    mySource.loop = false;
                    mySource.clip = audioStop;
                    mySource.Play();
                }
            } 
        }

    }

    void InteractDelegate()
    {
        // Start playing the moving sound
        if (mySource != null && audioMove != null)
        { 
            mySource.clip = audioMove;
            mySource.loop = true;
            mySource.Play();
        }

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

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position + (Vector3.up * 2), Range);
    }

	public void Activate()
	{
		throw new NotImplementedException();
	}

	public ZoneScript myZone
	{
		get
		{
			return zone;
		}
		set
		{
			zone = value;
		}
	}

	public bool Triggered
	{
		get
		{
			return false;	
		}
		set
		{
 
		}
	}
}
