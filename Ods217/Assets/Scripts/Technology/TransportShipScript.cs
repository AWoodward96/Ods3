using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportShipScript : MonoBehaviour {

    public MeshRenderer Exterior; 
    Color ExteriorColor = Color.white;

    public enum ShipOrientation { faceX, faceZ };
    public ShipOrientation Orientation;

    Vector3 halfExtents = new Vector3(-16f,0, 4); 
    GameObject player;
     

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        ExteriorColor = Color.Lerp(ExteriorColor, (PlayerBounds() ? Color.clear : Color.white), Time.deltaTime * 10);

        Exterior.material.SetColor("_Color", ExteriorColor);
	}


    bool PlayerBounds()
    {
        // Check to see if the player is within the bounds
        Vector3 playerPos = playerRef.transform.position;

        // Get the two corners
        Vector3 topLeft = transform.position + ((Orientation == ShipOrientation.faceX) ? halfExtents : new Vector3(-4, 0, 16));
        Vector3 bottomRight = transform.position - ((Orientation == ShipOrientation.faceX) ? halfExtents : new Vector3(-4, 0, 16)); 

        return (playerPos.x < bottomRight.x) && (playerPos.x > topLeft.x) && (playerPos.z > bottomRight.z) && (playerPos.z < topLeft.z);
    }

    public GameObject playerRef
    {
        get
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");

            return player;
        }
    }
}
