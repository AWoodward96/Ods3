using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effects Script
/// When it's interacted with by pressing [E] it will trigger another object
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class lgcOnCollisionTrigger : MonoBehaviour {

    public GameObject[] ObjectsToTrigger;
    public AudioClip SoundToPlayWhenTriggered;
    List<IPermanent> triggerObjects;
    AudioSource mySource;

    public bool disableOnFirstTrigger;


    BoxCollider myBoxCollider;
    GameObject Player;

    // Use this for initialization
    void Start()
    { 
        triggerObjects = new List<IPermanent>();

        foreach(GameObject o in ObjectsToTrigger)
        {
            IPermanent perm = o.GetComponent<IPermanent>();
            if(perm != null)
            {
                triggerObjects.Add(perm);
            }
        }

        mySource = GetComponent<AudioSource>();
        mySource.playOnAwake = false;
        mySource.spatialBlend = 1;
        
        mySource.clip = SoundToPlayWhenTriggered;

        myBoxCollider = GetComponent<BoxCollider>();
        myBoxCollider.isTrigger = true;
    } 

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach (IPermanent perm in triggerObjects)
            {
                perm.Triggered = !perm.Triggered;
            }

            if (SoundToPlayWhenTriggered != null)
            {
                mySource.clip = SoundToPlayWhenTriggered;
                mySource.Play();
            }

            if (disableOnFirstTrigger)
                myBoxCollider.enabled = false;
        }
    }
}
