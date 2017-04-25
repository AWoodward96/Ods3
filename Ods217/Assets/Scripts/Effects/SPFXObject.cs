using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpriteRenderer))]
public class SPFXObject : MonoBehaviour {

    Rigidbody myRGB;
    public float killTimer;
    public Vector3 appliedForce;

    void OnEnable()
    {
        myRGB = GetComponent<Rigidbody>();
        myRGB.useGravity = false;

        if (killTimer == 0)
            killTimer = 2;

        myRGB.AddForce(appliedForce);
        StartCoroutine(KillCRT());
    }
    IEnumerator KillCRT()
    {
        yield return new WaitForSeconds(killTimer);
        Destroy(this.gameObject);
       
    }
}
