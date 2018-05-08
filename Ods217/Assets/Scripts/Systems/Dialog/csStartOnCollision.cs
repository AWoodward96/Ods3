﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class csStartOnCollision : MonoBehaviour, IPermanent {

    BoxCollider myBoxCollider;
    
    [Header("Text")]
    [TextArea(1,100)]
    public string Dialog;

    [Space(40)]
    [Header("OR File")]
    public TextAsset Text;
     
 

    public bool Triggered
    {
        get
        {
            return false;
        }

        set
        {
           
        }
    }

    // Use this for initialization
    void Start ()
    { 
        myBoxCollider = GetComponent<BoxCollider>();
        myBoxCollider.isTrigger = true;
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player") 
            runCS(); 
    }
 

    void runCS()
    { 
        if (Dialog == "")
            CutsceneManager.instance.StartCutscene(Text.text);
        else
            CutsceneManager.instance.StartCutscene(Dialog);

        StartCoroutine(Deactivate());
    }

    IEnumerator Deactivate()
    {
        // Ensure that the cutscene actually started before deactivating this object because the CS Manager.instance is slow
        while (CutsceneManager.InCutscene)
        { 
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
