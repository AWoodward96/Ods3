using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
public class AnimatedDoor : MonoBehaviour, INonUnit
{
    public bool _Triggered;
    Animator myAnimator;
    BoxCollider myCollider;

    // Use this for initialization
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        myCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        myAnimator.SetBool("Open", _Triggered);
        myCollider.enabled = !_Triggered;
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

    }

    public void OnHit()
    {
 
    }
}
