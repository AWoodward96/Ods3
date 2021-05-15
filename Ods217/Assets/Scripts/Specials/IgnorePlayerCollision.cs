using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnorePlayerCollision : MonoBehaviour {
     
    void IgnoreCollision(Collider _col)
    {
        Collider[] cols = GetComponents<Collider>();
        for (int i = 0; i < cols.Length; i++)
        { 
            Physics.IgnoreCollision(cols[i], _col);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        { 
            IgnoreCollision(other);
        }
    }

}
