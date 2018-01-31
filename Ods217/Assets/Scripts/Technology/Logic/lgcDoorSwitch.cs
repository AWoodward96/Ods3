using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effects Script
/// When it's interacted with by pressing [E] it will trigger another object
/// </summary> 
[RequireComponent(typeof(AudioSource))]
public class lgcDoorSwitch : MonoBehaviour {

    public SlidingDoor DoorToTrigger;
    public AudioClip SoundToPlayWhenTriggered; 
    AudioSource mySource;

    [Range(.1f, 10)]
    public float Range;
    public bool Interactable;

    public SpriteRenderer ConsoleRenderer;
    public Sprite Open;
    public Sprite Closed;
    public Sprite Locked;

    bool lockedPrev;

    UsableIndicator ind_Interactable;
    GameObject Player;

    // Use this for initialization
    void Start()
    {
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Output = OnInteract;
        ind_Interactable.Preset = ((DoorToTrigger.Triggered) ? UsableIndicator.usableIndcPreset.Close : UsableIndicator.usableIndcPreset.Open);

        mySource = GetComponent<AudioSource>();
        mySource.playOnAwake = false;
        mySource.spatialBlend = 1;
        
        mySource.clip = SoundToPlayWhenTriggered;
        if(DoorToTrigger == null) 
            Debug.Log("You have not set the DoorToTrigger in this script. It will not work otherwise. " + this);
    }

    // Update is called once per frame
    void Update()
    { 
        if(DoorToTrigger == null) 
            return; 

        // Update the sprite renderer
        if(ConsoleRenderer != null)
        {
            ConsoleRenderer.sprite = (DoorToTrigger.Locked) ? Locked : ((DoorToTrigger.Triggered) ? Open : Closed);
        }


        if (lockedPrev != DoorToTrigger.Locked)
        {
            UpdateInteractable();
        }
        lockedPrev = DoorToTrigger.Locked;

    }

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }

    void OnInteract()
    {
        DoorToTrigger.Triggered = !DoorToTrigger.Triggered;

        if (SoundToPlayWhenTriggered != null)
        {
            mySource.clip = SoundToPlayWhenTriggered;
            mySource.Play();
        }

        UpdateInteractable();

    }

    void UpdateInteractable()
    {

        if (DoorToTrigger.Locked)
            ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Locked;
        else
            ind_Interactable.Preset = ((DoorToTrigger.Triggered) ? UsableIndicator.usableIndcPreset.Close : UsableIndicator.usableIndcPreset.Open);
    }
}
