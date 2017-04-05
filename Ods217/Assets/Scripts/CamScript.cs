using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    CamScript instance;
    public Vector3 FollowBack;
    public Transform Target;
    public Vector3 ExtentsTL;
    public Vector3 ExtentsBR;
    public static Vector3 CursorLocation; 

	// Use this for initialization
	void Awake () {
        instance = this;
        FollowBack = GlobalConstants.DEFAULTFOLLOWBACK;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (Target == null)
            Target = new GameObject().transform;

        CursorToWorld();

        if (Target.GetComponent<CController>() != null)
        {
            Vector3 Additive = Target.position + FollowBack;

            Vector3 toCursor = CursorLocation - transform.position;
            toCursor = GlobalConstants.ZeroYComponent(toCursor);
            if (toCursor.magnitude > 2)
                toCursor = toCursor.normalized * 2;

            Additive += toCursor;


            Additive.y = Target.position.y + FollowBack.y;
            transform.position = Vector3.Lerp(transform.position, Additive, 6f * Time.deltaTime);

            HandleExtents();

        }


	}

    void HandleExtents()
    {

        Camera main = Camera.main;
        float size = main.orthographicSize;
        float verticleSize = size * 2;
        float horezontalSize = size * Screen.width / Screen.height;

        Plane horezPlane = new Plane(Vector3.up, ExtentsTL);
        Ray CameraProjectionRay = main.ScreenPointToRay(new Vector2(0, Screen.height / 2));
        float dist;
        horezPlane.Raycast(CameraProjectionRay, out dist);
        Vector3 posOnGround = CameraProjectionRay.GetPoint(dist);



        verticleSize = verticleSize / (2 * Mathf.Cos(Mathf.Deg2Rad * 30));
        float tiltDistance = (Mathf.Abs(posOnGround.z - transform.position.z));
        float topBounds = verticleSize + tiltDistance;
        float bottomBounds = tiltDistance - verticleSize;


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
        //Ray r = Camera.main.ScreenToWorldPoint()
        Plane p = new Plane(Vector3.up, Target.transform.position); 
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition); // This takes the mouse position, and starts a ray at the world position (based on where the mouse is on screen) shooting where the camera is pointer
        float dist;
        if(p.Raycast(ray,out dist))
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
