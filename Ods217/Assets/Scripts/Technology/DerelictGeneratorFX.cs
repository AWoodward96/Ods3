using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DerelictGeneratorFX : MonoBehaviour, IPermanent {

    public bool State;
    ZoneScript Z;

    public Light GlowLight;
    public ParticleSystem BurnSystem; 
    Vector3 RootPosition;
    AudioSource[] sources;
    ParticleSystem.EmissionModule emit;

    // Use this for initialization
    void Start()
    { 
        RootPosition = transform.position;

        sources = GetComponents<AudioSource>();
        emit = BurnSystem.emission;
    }

    // Update is called once per frame
    void Update()
    {  
            for (int i = 0; i < sources.Length; i++)
            { 
                sources[i].pitch = Mathf.Lerp(sources[i].pitch, ((State) ? 1 : 0), Time.deltaTime);
            }

        emit.rateOverTime = Mathf.Lerp(emit.rateOverTime.constant, ((State) ? 20 : 0), Time.deltaTime);

        if (State)
            transform.position = RootPosition + (UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0,.15f));
    }
     
    /// IPermanent stuff!  
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
            return State;
        }

        set
        {
            State = value;
 
            GlowLight.enabled = State;

            if (State)
                BurnSystem.Play();
            else
                BurnSystem.Stop();
 

        }
    }

    public void Activate()
    {
        Triggered = !Triggered;
    }


}
