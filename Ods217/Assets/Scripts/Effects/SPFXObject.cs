using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A Special Effect
/// A class for an object that shows an emotion and then deletes itself
/// Mostly used in dialog managers, but is also used to show a spier is alert
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpriteRenderer))]
public class SPFXObject : MonoBehaviour
{

    Rigidbody myRGB;
    public float killTimer;
    public Vector3 appliedForce;

    // On Enable rather then start or Awake so that we can generate this object, modify it's parameters and then enable it
    void OnEnable()
    {
        myRGB = GetComponent<Rigidbody>();
        myRGB.useGravity = false; // Don't use gravity

        // If there is no kill timer specified set the default to 2
        if (killTimer == 0)
            killTimer = 2;

        // Same deal with the applied force
        if (appliedForce == Vector3.zero)
            appliedForce = new Vector3(0, 5, 0);

        myRGB.AddForce(appliedForce); // Consider changing this to straight up velocity
        StartCoroutine(KillCRT());
    }

    IEnumerator KillCRT()
    {
        yield return new WaitForSeconds(killTimer);
        Destroy(this.gameObject);
    }
}
