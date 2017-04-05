using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CannonBallTesting : MonoBehaviour {

    public GameObject Target;
    public bool Go;
    CController myCC;

	// Use this for initialization
	void Start () { 
        myCC = GetComponent<CController>();
	}
	
	// Update is called once per frame
	void Update () {

        if(Go)
        {
            Go = false;

            //float currentGravity = Physics.gravity.magnitude;
            float currentGravity = GlobalConstants.Gravity;
            float angle = 45;

            Vector3 planarTarget = new Vector3(Target.transform.position.x, 0, Target.transform.position.z);
            Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);


            float distance = Mathf.Max(Vector3.Distance(planarTarget, planarPosition), 1);
            float yOffset = transform.position.y - Target.transform.position.y;

            float initialVel = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * currentGravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
            Vector3 vel = new Vector3(0, initialVel * Mathf.Sin(angle), initialVel * Mathf.Cos(angle));

            float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (Target.transform.position.x < transform.position.x ? -1: 1);
            Vector3 finalVel = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * vel;


            //myRGB.velocity = finalVel;
            myCC.ApplyForce(finalVel * distance);
        }
		
	}

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Player")
        {
            Debug.Log("I hit you!");

        }
    }
}
