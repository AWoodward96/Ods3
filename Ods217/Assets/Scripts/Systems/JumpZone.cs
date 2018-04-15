using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpZone : MonoBehaviour {

    public Vector3 Size;
    public Vector3 Entry;
    public Vector3 Destination;
    public Color Color = Color.white;

    BoxCollider col;


    // Use this for initialization
    void Start () { 
        col = gameObject.AddComponent<BoxCollider>();
        col.size = Size;
        col.center = Entry - transform.position;
        col.isTrigger = true; 
	}

    private void OnTriggerEnter(Collider other)
    {
        CController cc = other.GetComponent<CController>();
        if(cc != null)
        {
            // Move whatever object this is to its destination
            Vector3 displacement = Entry - cc.transform.position; 
            cc.transform.position = Destination + displacement;
        }
    }

    public void OnDrawGizmosSelected()
    {
            Gizmos.color = Color; 
            Gizmos.DrawWireCube(Entry, Size);
            Gizmos.DrawWireCube(Destination, Size);
    }
}
 