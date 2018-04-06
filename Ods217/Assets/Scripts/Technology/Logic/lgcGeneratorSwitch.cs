using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcGeneratorSwitch : lgcSwitch {

    public DerelictGeneratorFX myGen;
    public SpriteRenderer ScreenRenderer;
    
    
    public override bool Triggered
    {
        get
        {
            return base.Triggered;
        }

        set
        {
            base.Triggered = value;
            myGen.Triggered = value;
            ScreenRenderer.enabled = value;
            GetComponent<AudioSource>().Play();
        }
    }
}
