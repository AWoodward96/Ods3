//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class Bolt3 : MonoBehaviour {

//	public bool ActiveBool;
//	public Vector2 Position1; // the position of the line we're going to make; Start of the line
//	public Vector2 Position2; // end of the line

//	public int Segmentation;

//	public List<GameObject> ActiveSegments;
//	public List<GameObject> InactiveSegments;
//	public GameObject LinePrefab;

//	public float MaxDistance;

//	Vector2[] Positions;
//	Vector2[] Extras;

//	GameObject Placeholder;
//	Line2 LineHolder;
//	// List: List<Vector2> Positions;


//	// Use this for initialization
//	void Start () {
//		ActiveBool = false;
//		Positions = new Vector2[0];
//		// List : Positions = new List<Vector2> ();
//		Initialize (Segmentation);
//	}
	
//	public void Initialize(int maxSegments)
//	{
//		// Initiate lists
//		ActiveSegments = new List<GameObject> ();
//		InactiveSegments = new List<GameObject>();
		
//		// Fill all them inactive bolts
//		for (int i = 0; i < Mathf.Pow (2, Segmentation); i ++) {
//			GameObject line = (GameObject)GameObject.Instantiate (LinePrefab);
			
//			// parent it to the bolt
//			line.transform.parent = transform;
			
//			line.SetActive (false);
			
//			InactiveSegments.Add (line);
			
//		}
		
//	}
//	// Update is called once per frame
//	void Update () {
			



//		if (ActiveBool) {
//			SetupBolt (Position1, Position2); // Gonna start with 5

//			for (int j = 1; j < Positions.Length; j++) {
//				//Debug.DrawLine (new Vector3(Positions[j-1].x, Positions[j-1].y, 0), new Vector3(Positions[j].x, Positions[j].y, 0));
//				Placeholder = InactiveSegments [j - 1];
//				Placeholder.SetActive (true);

//				LineHolder = Placeholder.GetComponent<Line2> ();
//				LineHolder.A = Positions [j - 1];
//				LineHolder.B = Positions [j];


//			}
//		} else {
//			for (int i = 1; i < Positions.Length; i++) {
//				Placeholder = InactiveSegments [i - 1];
//				Placeholder.SetActive (false);
				
				
//			}
//		}
		
//	}

//	Vector2 AquireSmallestInterval(Vector2 start, Vector2 end)
//	{
//		// Smallest segment is the distance devided by how many segments we will make
//		// Define the return variable
//		Vector2 returnVal = new Vector2();

//		// Get the distance vector
//		Vector2 DifferenceVector = end - start;
//		// And the total amount of segments we'll be making
//		int TotalSegments = (int)Mathf.Pow (2, Segmentation); // Currently: 1-2,2-4,3-8,4-16,5-32

//		// calculate the smallest interval
//		returnVal = DifferenceVector / TotalSegments;
	
//		// return it
//		return returnVal;
//	}

//	void AlterPositionValues()
//	{
//		bool[] AlteredValues = new bool[Positions.Length];
//		AlteredValues [0] = true;
//		AlteredValues [AlteredValues.Length - 1] = true;

//		float startSlot, endSlot;
//		float fraction = 1;
//		float val;
//		float  corespondingSlot;

//		for (int i = 0; i < Segmentation; i++) {
//			fraction /= 2;

//			val = 0;
//			while(val < 1)
//			{
//				if(val !=0)
//				{
//					// Calculate the coresponding slot
//					corespondingSlot = (Positions.Length -1) * val;

//					// if the slot has not already been modified, modify it
//					if(!AlteredValues[(int)corespondingSlot])
//					{
//						startSlot = corespondingSlot - (Positions.Length -1) * fraction;
//						endSlot = corespondingSlot + (Positions.Length -1) * fraction;
						
//						// Calculate values
//						Positions[(int)corespondingSlot] = CalculateAlteration ((int)startSlot,(int)corespondingSlot,(int)endSlot, fraction);
//						//Debug.Log ("The position vector at slot: " + corespondingSlot + ", was just moved to: " + Positions[(int)corespondingSlot]);
//						AlteredValues[(int)corespondingSlot] = true;
//					}
//				}

//				val += fraction;
//			}
//		}
//	}

//	Vector2 CalculateAlteration(int startslot, int middleslot, int endslot, float lengthModifier)
//	{
//		// Calculate the distance between the two values and then alter it
//		Vector2 distanceVector = Positions [endslot] - Positions [startslot];

//		int rnd = Random.Range (0, 2);

//		Vector2 PerpendicularVector;

//		// Based on random chance, calculate the perpendicular vector to move the point up or down
//		if (rnd == 0) {
//			PerpendicularVector = new Vector2(-distanceVector.y, distanceVector.x);
//		} else {
//			PerpendicularVector = new Vector2(distanceVector.y, -distanceVector.x);
//		}

//		// normalize that perpendicularVector;
//		PerpendicularVector = PerpendicularVector.normalized;

//		// Based on what the fraction is, multiply the perpendicular vector by the max distance, and then fractionizeeee(?) it
//		PerpendicularVector = (PerpendicularVector * (MaxDistance * Random.value)) * (lengthModifier * 2);
//		//Debug.Log ("Current Modifying Perpendicular Vector: " + PerpendicularVector);

//		// Add that perpendicular vector the the position in the middle slot
//		Positions[middleslot] += PerpendicularVector;


//		// Return it
//		return Positions [middleslot];

//	}

//	public void SetupBolt(Vector2 start, Vector2 end)
//	{
//		// Set up the positions vector
//		Positions = new Vector2[(int)Mathf.Pow (2, Segmentation) + 1]; // It's +1 because we're including the start and the end of the value

//		ActiveBool = true;

//		Vector2 SmallestValue = AquireSmallestInterval (start, end);

//		int TotalSegments = (int)Mathf.Pow (2, Segmentation); // Currently: 1-2,2-4,3-8,4-16,5-32


//		Positions [0] = start;
//		Positions [Positions.Length - 1] = end;

//		// Fill the positions array with positions
//		for (int i = 1; i < TotalSegments; i ++) { // starting at i = 1 because position 0 is already full, Total segments - 1 for the 0th array issue
//			Positions[i] = Positions[i-1] + SmallestValue;

//		}

//		// Move the position values 
//		AlterPositionValues ();

//		/*for (int k = 0; k < Positions.Length; k++) {
//			Debug.Log ("Final: " + Positions[k]);
//		}*/


//	}
//}
