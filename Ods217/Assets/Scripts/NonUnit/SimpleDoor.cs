using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDoor : MonoBehaviour, INonUnit{

    public GameObject ChildToRotate;
    [Range(-359,359)]
    public float RotateDegrees;
    public bool _Triggered;

    float origionalRotation;
    float currentRotation;



    // Use this for initialization
    void Start()
    {
        origionalRotation = ChildToRotate.transform.rotation.y;
        currentRotation = origionalRotation;
    }

    // Update is called once per frame
    void Update()
    {
        currentRotation = ChildToRotate.transform.rotation.y; 
        Vector3 to = new Vector3(ChildToRotate.transform.rotation.x, (_Triggered) ? RotateDegrees : origionalRotation, ChildToRotate.transform.rotation.z);
        ChildToRotate.transform.rotation = Quaternion.Lerp(ChildToRotate.transform.rotation, Quaternion.Euler(to), .9f * Time.deltaTime);
 

    }


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
