using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
 
public class ZoneScript : MonoBehaviour {

    // Use this for initialization
    public enum AudioZoneType { Additive, Inverse, Enable };
    public enum ViewType { Wire, Solid };
    [Header("Zone Info")]
    public ViewType type;
    public float YPosition;
    public Vector2 ZoneSize;
    public static ZoneScript ActiveZone;
    ZoneScript PrevZone;

    [Header("Meta")]
    public bool PrimaryZone;
    public bool AlwaysShowGizmos;

    public float CameraSize;

    public List<GameObject> Enemies;
    public List<Light> LightObjects;

    Vector3 topLeft;
    Vector3 bottomRight;


    public ZoneScript()
    {
        CameraSize = 9;
        ZoneSize = new Vector2(1, 1);
    }

	void Awake () {
        if(Application.isPlaying)
        {
            if (PrimaryZone)
            {
                CamScript c = Camera.main.GetComponent<CamScript>();
                c.ExtentsBR = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2 + 2);
                c.ExtentsTL = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2 + 4);
                Camera.main.orthographicSize = CameraSize;
                ActiveZone = this;
            }

            topLeft = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2);
            bottomRight = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2);

            GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject g in gos)
            {
                IUnit unit = g.GetComponent<IUnit>();
                if (unit != null)
                {
                    Vector3 zerodPos = GlobalConstants.ZeroYComponent(g.transform.position);
                    if (g.name.Contains("Spider") && (zerodPos.x > topLeft.x && zerodPos.x < bottomRight.x && zerodPos.z < topLeft.z && zerodPos.z > bottomRight.z))
                    {
                        JumpingSpider j = g.GetComponent<JumpingSpider>();
                        if (j != null)
                            j.MyZone = this;

                        DeactivatedSpider d = g.GetComponent<DeactivatedSpider>();
                        if (d != null)
                            d.MyZone = this;

                        Enemies.Add(g);
                    }
                }

                Light l = g.GetComponent<Light>();
                if (l != null)
                {
                    Vector3 zerodPos = GlobalConstants.ZeroYComponent(l.transform.position);

                    if (zerodPos.x > topLeft.x && zerodPos.x < bottomRight.x && zerodPos.z < topLeft.z && zerodPos.z > bottomRight.z)
                    {
                        LightObjects.Add(l);
                    }
                }

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
 

    void SetLights(bool _val)
    {

        LightObjects = LightObjects.Where(item => item != null).ToList(); // get rid of any values that are invalid

        foreach (Light l in LightObjects)
        {
            l.enabled = _val;
        }
    }
  
 
}

 

