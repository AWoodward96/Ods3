using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class lgcJukebox : MonoBehaviour, IPermanent {

    ZoneScript Z;
    bool triggered;
    AudioSource[] mySources;

    public ZoneScript myZone
    {
        get
        {
            return Z;
        }

        set
        {
            Z = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return triggered;
        }

        set
        {
            triggered = value;
            if (mySources != null)
                mySources = GetComponents<AudioSource>();
 
            for(int i = 0; i < mySources.Length; i ++)
            {
                if(triggered)
                    mySources[i].Play(); 
            } 
        }
    }

    public void Activate()
    {
        
    }

    private void Start()
    {
        mySources = GetComponents<AudioSource>();
    }

    private void Update()
    {
        for(int i = 0; i < mySources.Length; i++)
        {
            mySources[i].volume = Mathf.Lerp(mySources[i].volume, (Triggered) ? 1 : 0, Time.deltaTime);

            if (!Triggered && mySources[i].volume <= .1f)
                mySources[i].Stop();
        }
    }
 
}
