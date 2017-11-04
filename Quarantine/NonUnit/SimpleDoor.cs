using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A NonUnit Script
/// A nice little door that rotates open once triggered
/// </summary>
public class SimpleDoor : MonoBehaviour, INonUnit
{

    public GameObject ChildToRotate; // The object that we'll rotate once triggered
    [Range(-359, 359)]
    public float RotateDegrees; // How far we'll rotate once triggered
    public bool _Triggered;

    float origionalRotation;
    float currentRotation;



    // Use this for initialization
    void Start()
    {
        // We have origional rotation so it can rotate back when not triggered
        origionalRotation = ChildToRotate.transform.rotation.y;
        currentRotation = origionalRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the object based on if this script has been triggered or not
        currentRotation = ChildToRotate.transform.rotation.y;
        Vector3 to = new Vector3(ChildToRotate.transform.rotation.x, (_Triggered) ? RotateDegrees : origionalRotation, ChildToRotate.transform.rotation.z);
        ChildToRotate.transform.rotation = Quaternion.Lerp(ChildToRotate.transform.rotation, Quaternion.Euler(to), .9f * Time.deltaTime);
    }

    // This object cannot be powered
    public bool Powered
    {
        get
        {
            return false;
        }

        set
        {

        }
    }

    public bool Triggered
    {
        get
        {
            return _Triggered;
        }

        set
        {
            _Triggered = value;
        }
    }

    public void OnEMP()
    {
        // nothing
    }

    public void OnHit()
    {
        // nothing
    }


}
