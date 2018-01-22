using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ConveyorBelt : MonoBehaviour {
     
    public Vector3 MoveVec; 
 
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            CController ctrl = other.GetComponent<CController>();
            if (ctrl != null)
                ctrl.ApplyForce(MoveVec); 
        } 
    }

 
}
 