using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effects Script
/// When it's interacted with by pressing [E] it will trigger another object
/// </summary>
[RequireComponent(typeof(UsableIndicator))]
[RequireComponent(typeof(AudioSource))]
public class lgcInteractToTrigger : MonoBehaviour {

    public GameObject[] ObjectsToTrigger;
    public AudioClip SoundToPlayWhenTriggered;
    List<IPermanent> triggerObjects;
    AudioSource mySource;

    [Range(.1f, 10)]
    public float Range;
    public bool Interactable;
 
    UsableIndicator ind_Interactable;
    GameObject Player;

    // Use this for initialization
    void Start()
    {
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Output = InteractDelgate;

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
    }
 
    void InteractDelgate()
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
    }

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }
}
