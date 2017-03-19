using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropShadowScript : MonoBehaviour {

 

	// Use this for initialization
	void Start () {
   
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        Ray r = new Ray(transform.parent.position + (Vector3.up), Vector3.down);
        Vector3 pos = transform.position;
        if (Physics.Raycast(r, out hit, 50, LayerMask.GetMask("Ground")))
        {
            pos = Vector3.Lerp(transform.position, hit.point + Vector3.up / 5, .98f); 
        }
        else
        {
            Debug.Log("Never see this");
        }
        transform.position = new Vector3(transform.parent.position.x, pos.y, transform.parent.position.z);
    }
}
