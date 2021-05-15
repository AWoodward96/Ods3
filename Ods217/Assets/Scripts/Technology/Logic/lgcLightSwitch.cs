using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcLightSwitch : lgcSwitch {

    

    public override void Delegate()
    {
        Triggered = !Triggered;

        Light[] lights = GetComponentsInChildren<Light>();
        for(int i = 0; i < lights.Length; i ++)
        {
            lights[i].enabled = Triggered;
        } 

        AudioSource source = GetComponent<AudioSource>();
        if(source)
        {
            source.Play();
        }
    }

}
