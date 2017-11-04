using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The primary camera script
/// Uses orthographic view and follows a target at a constant distance
/// </summary>
public class CamScript : MonoBehaviour
{

    CamScript instance;
    public Vector3 FollowBack;
    public Transform Target;
    public Vector3 ExtentsTL; // The restrictions of where the camera can move and where the edges are
    public Vector3 ExtentsBR;
    public static Vector3 CursorLocation;

    public Transform Vizualiser;

    bool Loaded;
    float loadedPoint = 0;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        FollowBack = GlobalConstants.DEFAULTFOLLOWBACK;
        SceneManager.sceneLoaded += LevelLoad;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target == null)
            Target = new GameObject().transform;

        CursorToWorld();


        if (Target.GetComponent<CController>() != null)
        {
            Vector3 Additive = Target.position + FollowBack;

            if (!DialogManager.InDialog && !UpgradesManager.MenuOpen)
            {
                Vector3 toCursor = CursorLocation - transform.position;
                toCursor = GlobalConstants.ZeroYComponent(toCursor);
                if (toCursor.magnitude > 6)
                    toCursor = toCursor.normalized * 6;

                Additive += toCursor;
            }

            Additive.y = Target.position.y + FollowBack.y;

            // If we just loaded a new scene then snap onto the location that the player is at
            if (Loaded)
                transform.position = Vector3.Lerp(transform.position, Additive, 6f * Time.deltaTime);
            else
                transform.position = Additive;

            HandleExtents();
        }else
        { 
            transform.position = Vector3.Lerp(transform.position, Target.position + FollowBack, 6f * Time.deltaTime);
        }

        // once a level is loaded, jump to where the camera is supposed to be instead of panning
        if (loadedPoint < 1)
        {
            loadedPoint += Time.deltaTime;
            if (loadedPoint > 1)
            {
                Loaded = true;
            }
        }

        if(Vizualiser != null)
        {
            Vizualiser.transform.position = CursorLocation;
        }
    }

    void LevelLoad(Scene level, LoadSceneMode _mode)
    {
        Loaded = false;
        loadedPoint = 0;
    }

    // Extents are the points where the camera is restricted from moving further
    void HandleExtents()
    {
        // Restrict the camera from moving outside the designated area
        Camera main = Camera.main;
        float size = main.orthographicSize;
        float verticleSize = size * 2;
        float horezontalSize = size * Screen.width / Screen.height;

        // Get the position on the ground that is right in the center of the screen
        Plane horezPlane = new Plane(Vector3.up, ExtentsTL);
        Ray CameraProjectionRay = main.ScreenPointToRay(new Vector2(0, Screen.height / 2));
        float dist;
        horezPlane.Raycast(CameraProjectionRay, out dist);
        Vector3 posOnGround = CameraProjectionRay.GetPoint(dist);
     
        // Get the bounds
        verticleSize = verticleSize / (2 * Mathf.Cos(Mathf.Deg2Rad * 30));
        float tiltDistance = (Mathf.Abs(posOnGround.z - transform.position.z));
        float topBounds = verticleSize + tiltDistance;
        float bottomBounds = tiltDistance - verticleSize;

        // Restrict the camera
        if (transform.position.x < (ExtentsTL.x + horezontalSize))
            transform.position = new Vector3(ExtentsTL.x + horezontalSize, transform.position.y, transform.position.z);

        if (transform.position.x > (ExtentsBR.x - horezontalSize))
            transform.position = new Vector3(ExtentsBR.x - horezontalSize, transform.position.y, transform.position.z);

        if (transform.position.z > (ExtentsTL.z - topBounds))
            transform.position = new Vector3(transform.position.x, transform.position.y, (ExtentsTL.z - topBounds));

        if (transform.position.z < (ExtentsBR.z - bottomBounds))
            transform.position = new Vector3(transform.position.x, transform.position.y, (ExtentsBR.z - bottomBounds));
    }

    void CursorToWorld()
    {
        // Find in world space where the cursor is pointing
        Plane p = new Plane(Vector3.up, Target.transform.position);
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition); // This takes the mouse position, and starts a ray at the world position (based on where the mouse is on screen) shooting where the camera is pointer
        float dist;
        if (p.Raycast(ray, out dist))
        { 
            CursorLocation = ray.GetPoint(dist);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = (ExtentsTL - ExtentsBR) / 2;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(center.x + Mathf.Abs(center.x - ExtentsBR.x), 2, center.x + Mathf.Abs(center.z - ExtentsBR.z)));
    }
}
