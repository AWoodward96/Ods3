using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcSwitchConsole : lgcSwitch {


    [Header("Console Data")]
    public SpriteRenderer ConsoleRenderer;
    public Sprite Open;
    public Sprite Closed;

	public bool isTimed;
	float currentTime;
	public float maxTime;

    AudioSource src;

    public override void Start()
    {
        base.Start();
        ind.Preset = UsableIndicator.usableIndcPreset.Interact;
        src = GetComponent<AudioSource>();

		currentTime = 0.0f;
    } 

    // Update is called once per frame
    void Update()
    {  
        // Update the sprite renderer
        if (ConsoleRenderer != null)
        {
            ConsoleRenderer.sprite = (Triggered) ? Open : Closed;

        } 

		// If it's a timed switch, update the timer
		if(isTimed && Triggered)
		{
			currentTime += Time.deltaTime;
			if(currentTime >= maxTime)
			{
				Triggered = false;
				currentTime = 0.0f;

				if(src != null)
					src.Play();
			}
		}
    }
    public override void Delegate()
    {
        base.Delegate();
        if (src != null)
            src.Play();
    }
}
