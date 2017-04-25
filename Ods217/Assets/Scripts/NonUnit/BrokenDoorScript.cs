using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BrokenDoorScript : MonoBehaviour, INonUnit {

    public int Health;
    public GameObject Top;
    Animator myAnimator;
    Animator topAnimator;
    BoxCollider myCollider;


    // Use this for initialization
    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        myAnimator = GetComponent<Animator>();
        topAnimator = Top.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            myAnimator.SetBool("Open", true);
            topAnimator.SetBool("Open", true);
            myCollider.center = new Vector3(-1.6f, 0, 0.5f);
            myCollider.size = new Vector3(0.8f, 4, 1);
        } 
    }


    public bool Powered
    {
        get
        { return false; }

        set { }
    }

    public bool Triggered
    {
        get
        { return false;  }

        set
        {  // nothing 
        }
    }

    public void OnEMP()
    {
        
    }

    public void OnHit()
    {
        Health--;
    }

}
