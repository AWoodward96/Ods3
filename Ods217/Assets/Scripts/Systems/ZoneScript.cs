using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SpriteToParticlesAsset;
 
/// <summary>
/// A script that handles camera placement and sceudo levels
/// Runs in edit mode so we can see how each zone is layed out
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ZoneScript : MonoBehaviour {

    
    public enum ViewType { Wire, Solid };
    [Header("Zone Info")]
    public ViewType type;
    public float YPosition; // Ideally 0, but this only effects inspector view and has no effect on the rest of the game
    public Vector2 ZoneSize; // The width and height of the zone
    public static ZoneScript ActiveZone; // Whichever zone is currently being occupied by the player
    public static ZoneLock ActiveLock;
    ZoneScript PrevZone;
    public enum AggressionType { NoCombat, SomeCombat, OnlyCombat };
    public AggressionType ZoneAggression;

    public ZoneLock[] ZoneLocks;


    [Header("Meta")]
    public bool PrimaryZone; // The Primary Zone is the first zone in the scene
    public bool AlwaysShowGizmos; // a helper boolean. Will show gizmos even if this game object isn't selected if true.

    public float CameraSize; // Change the camera size to this when you're in this zone

    public List<IPermanent> Perms; // A list of the enemies in this zone
    public List<Light> LightObjects; // A list of lights in this zone
    public List<WeaponBase> Weapons;
    [Tooltip("Sprite to Particle objects")]
    public List<SpriteToParticles> StP;
 
    Vector3 topLeft;
    Vector3 bottomRight;

    PlayerScript player;
    


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

            // Get every important object in the zone
            GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
            Weapons = new List<WeaponBase>();
            Perms = new List<IPermanent>();
            StP = new List<SpriteToParticles>();
            LightObjects = new List<Light>();
            foreach (GameObject g in gos)
            {
                if (g.transform.position.x < bottomRight.x && g.transform.position.x > topLeft.x && g.transform.position.z < topLeft.z && g.transform.position.z > bottomRight.z)
                {
                     
                    SpriteToParticles spritToPart = g.GetComponent<SpriteToParticles>();
                    if (spritToPart != null)
                    {
                        StP.Add(spritToPart);
                        // No continue because we want all sprite to parts
                    }

                    WeaponBase isUsableWeapon = g.GetComponent<WeaponBase>();
                    if(isUsableWeapon != null)
                    {
                        Weapons.Add(isUsableWeapon);
                        continue;
                    }

                    Light l = g.GetComponent<Light>();
                    if(l != null)
                    {
                        LightObjects.Add(l);
                        continue;
                    }

                    IPermanent perminant = g.GetComponent<IPermanent>();
                    if(perminant != null)
                    {
                        perminant.myZone = this;
                        Perms.Add(perminant);
                        continue;
                    }

                }

            }

        }

    }
	
	// Update is called once per frame
	void Update ()
    {
  
        CheckZone();
        CheckLocks();
        if (PrevZone != ActiveZone)
        { 
            SetLights((ActiveZone == this));
            SetParts((ActiveZone == this));
        }
         
        PrevZone = ActiveZone;
    }

    void SetParts(bool _val)
    { 
        StP = StP.Where(item => item != null).ToList(); // get rid of any values that are invalid

        foreach (SpriteToParticles P in StP)
        {
            P.enabled = _val;
        }
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

        // Draw all the locks
        Color c2 = Color.red ;
        c.a = .1f;
        Gizmos.color = c2;
        for (int i = 0; i < ZoneLocks.Length; i++)
        {
            Vector3 lockSize = new Vector3(ZoneLocks[i].Size.x, .5f, ZoneLocks[i].Size.y);
            Vector3 lockPosition = (transform.position + new Vector3(ZoneLocks[i].Location.x, 0, ZoneLocks[i].Location.y));

            
            Gizmos.DrawCube(lockPosition, lockSize);
        }


        Gizmos.DrawIcon(transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2), "TopLeftBracket");
        Gizmos.DrawIcon(transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2), "BottomRightBracket");
        Gizmos.DrawIcon(pos, "ZoneIcon");
    }

    void CheckZone()
    {
        // See if the player entered this zone
        if(ActiveZone != this)
        {

            // Check for the player
            GameObject player = playerRef.gameObject;
            if(player) // Can't do anything if he doesn't exist
            {
                // Calculate the zones extents
                Vector3 playerPos = GlobalConstants.ZeroYComponent(player.transform.position);
                
                // If the player is in the zone
                if (playerPos.x > topLeft.x && playerPos.x < bottomRight.x && playerPos.z < topLeft.z && playerPos.z > bottomRight.z)
                {
                    // We've entered!
                    CamScript c = Camera.main.GetComponent<CamScript>();
                    Camera.main.orthographicSize = CameraSize;
                    c.ExtentsBR = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2 + 2);
                    c.ExtentsTL = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2 + 4);
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
  
 
    public void CheckLocks()
    {
        bool anyActive = false;
        for(int i = 0; i < ZoneLocks.Length; i++)
        {
            Vector3 worldPosTopLeft = (transform.position + new Vector3(ZoneLocks[i].Location.x, 0, ZoneLocks[i].Location.y)) + new Vector3(-ZoneLocks[i].Size.x / 2, 0, ZoneLocks[i].Size.y / 2);
            Vector3 worldPosBottomRight = (transform.position + new Vector3(ZoneLocks[i].Location.x, 0, ZoneLocks[i].Location.y)) + new Vector3(ZoneLocks[i].Size.x / 2, 0, -ZoneLocks[i].Size.y / 2);
            Vector3 playerPos = playerRef.transform.position;
            if (playerPos.x > worldPosTopLeft.x && playerPos.x < worldPosBottomRight.x && playerPos.z < worldPosTopLeft.z && playerPos.z > worldPosBottomRight.z && !ZoneLocks[i].Enabled && ZoneLocks[i].EnableOnEntry && !ZoneLocks[i].Completed)
            {
                ActiveLock = ZoneLocks[i];
                ZoneLocks[i].Enabled = true;
            }

            if (ZoneLocks[i].Enabled)
            {
                anyActive = true;

                Vector3 newPos = playerPos;
                if (newPos.x < worldPosTopLeft.x)
                    newPos.x = worldPosTopLeft.x;
                if (newPos.z > worldPosTopLeft.z)
                    newPos.z = worldPosTopLeft.z;

                if (newPos.x > worldPosBottomRight.x)
                    newPos.x = worldPosBottomRight.x;
                if (newPos.z < worldPosBottomRight.z)
                    newPos.z = worldPosBottomRight.z;

                playerRef.transform.position = newPos;

                LineRenderer lRender = GetComponent<LineRenderer>();
                if(lRender != null)
                {
                    lRender.enabled = true; 
                    lRender.positionCount = 4;
                    lRender.useWorldSpace = true;
                    float newYPos = YPosition + lRender.widthMultiplier / 2;
                    lRender.SetPosition(0, new Vector3(worldPosTopLeft.x, newYPos, worldPosTopLeft.z));
                    lRender.SetPosition(1, new Vector3(worldPosTopLeft.x, newYPos, worldPosBottomRight.z));
                    lRender.SetPosition(2, new Vector3(worldPosBottomRight.x, newYPos, worldPosBottomRight.z));
                    lRender.SetPosition(3, new Vector3(worldPosBottomRight.x, newYPos, worldPosTopLeft.z));
                }


                // Check each enemy for if alive
                bool b = true;
                // loop through each unit in the GameObject. If they're dead, or defeated then, turn the lock off
                for (int e = 0; e < ZoneLocks[i].Enemies.Length; e++)
                {
                    IArmed a = ZoneLocks[i].Enemies[e].GetComponent<IArmed>();
                    if (a.MyUnit.CurrentHealth > 0)
                    {
                        b = false;
                    }
                }

                if (b && !ZoneLocks[i].Completed) {
                    ZoneLocks[i].Completed = true;
                    ZoneLocks[i].Enabled = false; 
                }

                break; // only have one lock on at a time
            }
        }

        LineRenderer lineRender = GetComponent<LineRenderer>();
        if(lineRender != null)
        { 
            lineRender.widthMultiplier = Mathf.Lerp(lineRender.widthMultiplier, ((anyActive) ? 1 : 0), .9f * Time.deltaTime);
            if(lineRender.widthMultiplier < .1f)
            {
                lineRender.widthMultiplier = .1f;
                lineRender.enabled = false;
            }
        }

    }


    public virtual PlayerScript playerRef
    {
        get
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

            return player;
        }
    }
}


[System.Serializable]
public struct ZoneLock
{
    public bool Enabled;
    public bool EnableOnEntry;
    public Vector2 Size;
    public Vector2 Location;
    public GameObject[] Enemies;
    public bool Completed;
}

 

