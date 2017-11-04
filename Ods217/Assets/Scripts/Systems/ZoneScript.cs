using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// A script that handles camera placement and sceudo levels
/// Runs in edit mode so we can see how each zone is layed out
/// </summary>
public class ZoneScript : MonoBehaviour {

    
    public enum ViewType { Wire, Solid };
    [Header("Zone Info")]
    public ViewType type;
    public float YPosition; // Ideally 0, but this only effects inspector view and has no effect on the rest of the game
    public Vector2 ZoneSize; // The width and height of the zone
    public static ZoneScript ActiveZone; // Whichever zone is currently being occupied by the player
    ZoneScript PrevZone;
    public enum AggressionType { NoCombat, SomeCombat, OnlyCombat };
    public AggressionType ZoneAggression;


    [Header("Meta")]
    public bool PrimaryZone; // The Primary Zone is the first zone in the scene
    public bool AlwaysShowGizmos; // a helper boolean. Will show gizmos even if this game object isn't selected if true.

    public float CameraSize; // Change the camera size to this when you're in this zone

    public List<GameObject> Enemies; // A list of the enemies in this zone
    public List<Light> LightObjects; // A list of lights in this zone

    Vector3 topLeft;
    Vector3 bottomRight;


    public ZoneScript()
    {
        CameraSize = 9;
        ZoneSize = new Vector2(1, 1);
    }

	void Awake () {
        if(Application.isPlaying) // If we're not in the editor
        {
            // Set up the primary zone
            if (PrimaryZone)
            {
                CamScript c = Camera.main.GetComponent<CamScript>();
                c.ExtentsBR = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2 + 2);
                c.ExtentsTL = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2 + 4);
                Camera.main.orthographicSize = CameraSize;
                ActiveZone = this;
            }

            // Calculate the top left and bottom right variables
            topLeft = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2);
            bottomRight = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2);

            // Get every enemy in the zone
            GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject g in gos)
            {
                

            }

        }

    }
	
	// Update is called once per frame
	void Update ()
    {
  
        checkZone(); 
         
        if (PrevZone != ActiveZone)
        { 
            SetLights((ActiveZone == this));
        }
         
        PrevZone = ActiveZone;
    }


    private void OnDrawGizmos()
    {
        if (AlwaysShowGizmos)
            OnDrawGizmosSelected();
    }
    private void OnDrawGizmosSelected()
    {
        // Set up the zone color
        Color c = Color.blue;
        c.a = .3f;
        Gizmos.color = c;

        Vector3 zoneSize = new Vector3(ZoneSize.x, .5f, ZoneSize.y);

        // Draw a cube to represent the area of the zone
        Vector3 pos = transform.position;
        pos.y = YPosition; 
        if (type == ViewType.Wire)
            Gizmos.DrawWireCube(pos, zoneSize);

        if (type == ViewType.Solid)
            Gizmos.DrawCube(pos, zoneSize);


        Gizmos.DrawIcon(transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2), "TopLeftBracket");
        Gizmos.DrawIcon(transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2), "BottomRightBracket");
        Gizmos.DrawIcon(pos, "ZoneIcon");
    }

    void checkZone()
    {
        // See if the player entered this zone
        if(ActiveZone != this)
        {
           
            // Check for the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if(player) // Can't do anything if he doesn't exist
            {
                // Calculate the zones extents
                Vector3 playerPos = GlobalConstants.ZeroYComponent(player.transform.position);
                
                // If the player is in the zone
                if (playerPos.x > topLeft.x && playerPos.x < bottomRight.x && playerPos.z < topLeft.z && playerPos.z > bottomRight.z)
                {
                    // We've entered!
                    CamScript c = Camera.main.GetComponent<CamScript>();
                    c.ExtentsBR = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2 + 2);
                    c.ExtentsTL = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2 + 4);
                    Camera.main.orthographicSize = CameraSize;
                    ZoneScript.ActiveZone = this;
                    player.GetComponent<PlayerScript>().EnteredNewZone(); 
                   
                } 
            }
     
        } 
    }
 
    // Turn on and off lights in the zone
    void SetLights(bool _val)
    {

        LightObjects = LightObjects.Where(item => item != null).ToList(); // get rid of any values that are invalid

        foreach (Light l in LightObjects)
        {
            l.enabled = _val;
        }
    }
  
 
}

 

