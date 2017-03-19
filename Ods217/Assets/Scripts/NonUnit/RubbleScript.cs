using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleScript : MonoBehaviour, INonUnit {

    public int Health;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
            Destroy(this.gameObject);
    }


    public bool Powered
    {
        get
        { return false; }

        set { }
    }

    public void OnEMP()
    {
        
    }

    public void OnHit()
    {
        Health--;
    }

}
