using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class WaypointsMovement : MonoBehaviour {

    public Vector3[] Waypoints; // Y axis is neglidgable
	public int[] resetWP;
    public bool ZeroY;
    public int waypointIndex;
    public float Speed;
    [Range(.01f, 5)]
    public float BufferDistance;

    CController myCC;
	BoxLockPrevention myBLP;

	// Use this for initialization
	void Start () {

        // Automatically zeros the y component of the waypoints script if zeroY is true
		if(ZeroY)
        {
            Vector3 foo = Vector3.zero;
            for (int i = 0; i < Waypoints.Length; i++)
            {
                foo = Waypoints[i];
                foo.y = 0;
                Waypoints[i] = foo;
            }
        }

        myCC = GetComponent<CController>();
		myBLP = GetComponent<BoxLockPrevention>();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 zeroTPos;

		if(ZeroY)
		{
			zeroTPos = GlobalConstants.ZeroYComponent(transform.position);
		}
		else
		{
			zeroTPos = transform.position;
		}

        Vector3 dst = Waypoints[waypointIndex] - zeroTPos;
         
		if (dst.magnitude <= BufferDistance)
        {
            //myCC.Velocity = dst;
			transform.position += dst;
            IncrementWaypoints(); 
        }
        else
        {
            //myCC.Velocity = dst.normalized * Speed;
			transform.position += dst.normalized * Speed * Time.deltaTime;

			Vector3 newDst = Waypoints[waypointIndex] - zeroTPos;

			// If we've passed the point in a single step but are still somehow outside of the range, account for that
			if(Vector3.Dot(dst, newDst) <= -0.9f)
			{
				transform.position += newDst;
				IncrementWaypoints();
			}
        }

        /*Collider[] d = Physics.OverlapBox(transform.position, new Vector3(1.1f, 1.1f, 1.1f), Quaternion.identity, LayerMask.GetMask("Units"));
        if (d.Length > 0)
        { 
            d[0].GetComponent<CController>().ApplyForce(myCC.Velocity);
        }*/


    }

    void IncrementWaypoints()
    {
        waypointIndex++;
        if (waypointIndex >= Waypoints.Length)
		{
            waypointIndex = 0;
		}
		if(myBLP)
		{
			for(int i = 0; i < resetWP.Length; i++)
			{
				if(resetWP[i] == waypointIndex)
				{
					myBLP.Triggered = false;
					break;
				}
			}
		}
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 prev = transform.position;
        for(int i = 0; i < Waypoints.Length; i++)
        {
            Gizmos.DrawLine(prev, Waypoints[i]);
            Gizmos.DrawSphere(Waypoints[i], .1f);
            prev = Waypoints[i];
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Player")
    //    {
    //        GameObject p = collision.gameObject;
    //        p.GetComponent<CController>().ApplyForce(myRGB.velocity);

    //    }
    //}
}
