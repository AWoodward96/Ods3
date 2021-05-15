using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpZone : MonoBehaviour {

    public Vector3 Size;
    public Vector3 Entry;
    public Vector3 Destination;
    public Color Color = Color.white;
    public bool CameraSnap = true; // Snap the camera to the new location

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
            Vector3 displacement = cc.transform.position - Entry; 
            cc.transform.position = Destination + displacement;
            if (CameraSnap)
                Camera.main.GetComponent<CamScript>().SnapCam();
        }
    }

    public void OnDrawGizmosSelected()
    {
            Gizmos.color = Color; 
            Gizmos.DrawWireCube(Entry, Size);
            Gizmos.DrawWireCube(Destination, Size);
    }
}
 