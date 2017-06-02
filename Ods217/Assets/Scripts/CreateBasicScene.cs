using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

/// <summary>
/// The objective of this script is to just completely nuke a scene and then build a basic one from the ground up
/// A starter scene if you will
/// </summary>
[ExecuteInEditMode]
public class CreateBasicScene : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // First get rid of the directional light and the main camera. We don't need those where we're coming from
        GameObject o = GameObject.Find("Directional Light");
        DestroyImmediate(o);
        DestroyImmediate(Camera.main.gameObject);

        // Ok then load in a couple of things
        Instantiate(Resources.Load("Prefabs/StarterKit/GameManager"), Vector3.zero, Quaternion.identity).name = "GameManager"; // Game Manager
        Instantiate(Resources.Load("Prefabs/StarterKit/MenuManager"), Vector3.zero, Quaternion.identity).name = "MenuManager"; // Menu Manager
        Instantiate(Resources.Load("Prefabs/StarterKit/CanvasManager"), Vector3.zero, Quaternion.identity).name = "CanvasManager"; // Canvas Manager

        GameObject Player = Instantiate(Resources.Load("Prefabs/StarterKit/Char1"), Vector3.zero + (Vector3.up * 3.4f), Quaternion.identity) as GameObject; // The Player
        Player.name = "Char1";

        GameObject Cam = Resources.Load("Prefabs/StarterKit/Main Camera") as GameObject; // The Camera
        Cam = Instantiate(Cam, Vector3.zero + (Vector3.up * 4), Cam.transform.rotation);
        Cam.GetComponent<CamScript>().Target = Player.transform; // Set the cameras target to the player
        Cam.name = "Main Camera";

        Instantiate(Resources.Load("Prefabs/StarterKit/StarterKitArena"), Vector3.zero, Quaternion.identity).name = "Starter Kit Arena"; // An arena
        Instantiate(Resources.Load("Prefabs/StarterKit/Zone"), Vector3.zero, Quaternion.identity).name = "Zone"; // A zone to start us off
       


        // Once all that is done,
        DestroyImmediate(this.gameObject); // Kill this object

    }
}
