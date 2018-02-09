using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcSwitchConsole : lgcSwitch {


    [Header("Console Data")]
    public SpriteRenderer ConsoleRenderer;
    public Sprite Open;
    public Sprite Closed;

    bool triggeredPrev;

    public override void Start()
    {
        base.Start();
        ind.Preset = UsableIndicator.usableIndcPreset.Interact;
    } 

    // Update is called once per frame
    void Update()
    {  
        // Update the sprite renderer
        if (ConsoleRenderer != null)
        {
            ConsoleRenderer.sprite = (Triggered) ? Open : Closed;
        }  
    }
}
