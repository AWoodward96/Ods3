using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// The objective of this script is to just completely nuke a scene and then build a basic one from the ground up
/// A starter scene if you will
/// </summary>
[ExecuteInEditMode]
public class CreateBasicScene : Editor {
 
	
	// Update is called once per frame
	void OnEnable () {

        // First get rid of the directional light and the main camera. We don't need those where we're coming from
        GameObject o = GameObject.Find("Directional Light");
        if(o != null)
            DestroyImmediate(o);

        if(Camera.main != null)
            DestroyImmediate(Camera.main.gameObject);

        // Ok then load in a couple of things
        PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/StarterKit/GameManager")).name = "GameManager"; // Game Manager
        PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/StarterKit/MenuManager")).name = "MenuManager"; // Menu Manager
        PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/StarterKit/CanvasManager")).name = "CanvasManager"; // Canvas Manager
        
        GameObject Player = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/StarterKit/Char1")) as GameObject; // The Player
        Player.name = "Char1";

        GameObject Cam = Resources.Load("Prefabs/StarterKit/Main Camera") as GameObject; // The Camera
        Cam = (GameObject)PrefabUtility.InstantiatePrefab(Cam);
        Cam.GetComponent<CamScript>().Target = Player.transform; // Set the cameras target to the player
        Cam.name = "Main Camera";

        PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/StarterKit/StarterKitArena") ).name = "Starter Kit Arena"; // An arena
        PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/StarterKit/Zone") ).name = "Zone"; // A zone to start us off
       


        // Once all that is done,
        DestroyImmediate(this); // Kill this object

    }
}
