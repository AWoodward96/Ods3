using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAndTarget : MonoBehaviour {

    public GameObject Target;
    Rigidbody rgb;

	// Use this for initialization
	void Start () {
        rgb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Y))
        {
            rgb.velocity = GlobalConstants.getPhysicsArc(transform.position, Target.transform.position);
        }
	}

    void boop()
    {
        Vector3 p = Target.transform.position;

        float gravity = Physics.gravity.magnitude;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - p.y;

        // Selected angle in radians 
        float angle = GlobalConstants.inverseMap(distance, 1, 15, 60, 89) * Mathf.Deg2Rad;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > transform.position.x ? 1 : -1);
        // Rotate our velocity to match the direction between the two objects 
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire! 
        rgb.velocity = (finalVelocity);
    }
}
