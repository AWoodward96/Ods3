using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpireElevator: MonoBehaviour {
     
    public float Range;
    public bool Interactable;
    public bool MenuOpen;
    UsableIndicator ind_Interactable;

    public enum SpireFloors { Residential, CommandCenter, Outlook };
    public SpireFloors CurrentFloor;

    private Vector3 Velocity;
    private bool Moving;
    private bool Direction; // true up, false down

    private Vector3 MoveDownVector = new Vector3(0, -10, 0);
    private Vector3 MoveUpVector = new Vector3(0, 10, 0);

    public GameObject[] CameraLocks;
    public GameObject[] ArrivalPoints;

    PlayerScript myPlayer;

    public Canvas myCanvas;

    // Use this for initialization
    void Start () {
        // Set the current floor
        int closestIndex = 0;
        float smallestDistance = 500;

        for(int i =0; i <ArrivalPoints.Length; i++)
        {
            if(ArrivalPoints[i] != null)
            {
                float curDist = Vector3.Distance(ArrivalPoints[i].transform.position, transform.position);
                if (curDist < smallestDistance)
                {
                    smallestDistance = curDist;
                    closestIndex = i;
                }
            }
        }

        CurrentFloor = (SpireFloors)closestIndex;

        // Set up other references
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
        ind_Interactable.Output = toggleDelegate; 
        myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        MenuOpen = false;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        Velocity = (Direction) ? MoveUpVector : MoveDownVector;


        if(Moving)
        { 
            transform.position += Velocity * Time.deltaTime;

            // Check to see if we can stop moving
            if (Vector3.Distance(transform.position, ArrivalPoints[(int)CurrentFloor].transform.position) < .5f)
            {
                Moving = false;
                transform.position = ArrivalPoints[(int)CurrentFloor].transform.position;
                if (myPlayer == null)
                    myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
                myPlayer.AcceptInput = true;
                Camera.main.gameObject.GetComponent<CamScript>().Target = myPlayer.transform;
            }
        }

        ind_Interactable.Disabled = Moving;

      
        myCanvas.enabled = MenuOpen && ind_Interactable.ActiveInd;
        if (!ind_Interactable.ActiveInd)
            MenuOpen = false;
    }

    void toggleDelegate()
    {
        MenuOpen = !MenuOpen;
    }

    public void GoToFloor(int _index)
    {
        SpireFloors _to = (SpireFloors)_index;
        if (_to == CurrentFloor)
            return;

        if(!Moving) // ensure this can only be called once
        { 
            Direction = ((int)_to > (int)CurrentFloor);
            MenuOpen = false;
            Moving = true;
            Camera.main.gameObject.GetComponent<CamScript>().Target = CameraLocks[(int)CurrentFloor].transform;

            if(myPlayer == null)
            {
                myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>(); 
            }

            myPlayer.AcceptInput = false;
            CurrentFloor = _to;

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
        transform.position = ArrivalPoints[(int)CurrentFloor].transform.position + (Vector3.up * ((Direction) ? -40 : 40));
        myPlayer.transform.position = transform.position + playerDistanceVector + Vector3.up;
        CamScript main = Camera.main.gameObject.GetComponent<CamScript>();
        main.Target = CameraLocks[(int)CurrentFloor].transform;
        main.gameObject.transform.position = CameraLocks[(int)CurrentFloor].transform.position + GlobalConstants.DEFAULTFOLLOWBACK - (Vector3.down * 5);
    }

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }
}
