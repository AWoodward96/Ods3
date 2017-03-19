using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class ZoneScript : MonoBehaviour {

    // Use this for initialization
    public enum ViewType { Wire, Solid };
    [Header("Zone Info")]
    public ViewType type;
    public float YPosition;
    public Vector2 ZoneSize;
    public static ZoneScript ActiveZone;

    Vector3 actualPosition;

    public List<TransitionZones> TransZones;

    [Header("Meta")]
    public bool PrimaryZone;
    public bool AlwaysShowGizmos;

    public float CameraSize;

    public ZoneScript()
    {
        CameraSize = 9;
        ZoneSize = new Vector2(1, 1);
    }

	void Awake () {
        if(PrimaryZone)
        {
            CamScript c = Camera.main.GetComponent<CamScript>();
            c.ExtentsBR = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2 + 2);
            c.ExtentsTL = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2 + 4);
            Camera.main.orthographicSize = CameraSize;
            ActiveZone = this;
        }
	}
	
	// Update is called once per frame
	void Update () {
        checkZone();
	}


    private void OnDrawGizmos()
    {
        if (AlwaysShowGizmos)
            OnDrawGizmosSelected();
    }
    private void OnDrawGizmosSelected()
    {

        DrawZone();

        DrawTransitions();
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
                Vector3 playerPos = GlobalConstants.ZeroYComponent(player.transform.position);
                Vector3 topLeft = transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2);
                Vector3 bottomRight = transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2);
                
                
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

 

    void DrawZone()
    {
        // Set up the zone color
        Color c = Color.blue;
        c.a = .3f;
        Gizmos.color = c;

        Vector3 zoneSize = new Vector3(ZoneSize.x, .5f, ZoneSize.y);

        // Draw a cube to represent the area of the zone
        Vector3 pos = transform.position;
        pos.y = YPosition;
        actualPosition = pos;
        if (type == ViewType.Wire)
            Gizmos.DrawWireCube(pos, zoneSize);

        if (type == ViewType.Solid)
            Gizmos.DrawCube(pos, zoneSize);


        Gizmos.DrawIcon(transform.position + new Vector3(-ZoneSize.x / 2, 0, ZoneSize.y / 2), "TopLeftBracket");
        Gizmos.DrawIcon(transform.position + new Vector3(ZoneSize.x / 2, 0, -ZoneSize.y / 2), "BottomRightBracket");
        Gizmos.DrawIcon(pos, "ZoneIcon");
    }

    void DrawTransitions()
    {
        for(int i = 0; i < TransZones.Count;i++)
        {
            TransitionZones t = TransZones[i]; // Get the zone
            // Set up the positions and associated positions
            Vector3 tpos = new Vector3(t.Position.x + actualPosition.x, YPosition, t.Position.y + actualPosition.z);
            Vector3 tarr = new Vector3(t.ArrivalPosition.x + actualPosition.x, YPosition, t.ArrivalPosition.y + actualPosition.z);
            Gizmos.color = t.AssociatedColor;

            Vector3 transSize = new Vector3(t.Size.x, 1, t.Size.y);

            if(type == ViewType.Wire)
            {
                Gizmos.DrawWireCube(tpos, transSize);
                Gizmos.DrawWireCube(tarr, transSize);

                
            }

            if(type == ViewType.Solid)
            {
                Gizmos.DrawCube(tpos, transSize);
                Gizmos.DrawCube(tarr, transSize);
            }

            Gizmos.DrawLine(tpos, tarr);
            Gizmos.DrawIcon(tpos + Vector3.up, "TransZoneIcon");
        }
    }
}

[System.Serializable]
public struct TransitionZones
{
    public Vector2 Size;
    public Vector2 Position;
    public Vector2 ArrivalPosition;
    public Color AssociatedColor;
}



