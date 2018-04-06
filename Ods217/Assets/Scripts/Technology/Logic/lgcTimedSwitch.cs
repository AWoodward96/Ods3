using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcTimedSwitch : lgcSwitch {


    [Header("Console Data")]
    public Animator ScreenAnimator;
     
	float currentTime;
    float soundTime;
    float clickRate;
    public float maxTime;

	public GameObject[] poweredUnits;
	IPermanent[] unitHandles;

    [Header("Sound")]
    public AudioClip toggleSound;
    public AudioClip clickingSound;
    AudioSource src;

    [HideInInspector]
    public bool IsCompleted;

    public override void Start()
    {
        base.Start();
        ind.Preset = UsableIndicator.usableIndcPreset.Interact;
        src = GetComponent<AudioSource>();

        soundTime = 0.0f;
		currentTime = 0.0f;

		unitHandles = new IPermanent[poweredUnits.Length];
		for(int i = 0; i < poweredUnits.Length; i++)
		{
			unitHandles[i] = poweredUnits[i].GetComponent<IPermanent>();
		}
    } 

    // Update is called once per frame
    void Update()
    {
        if(ScreenAnimator)
        {
            // Anim states:
            // 0 - Not triggered
            // 1 - Triggered, slow
            // 2 - Triggered, fast
            // 3 - Complete
            if (Triggered && !IsCompleted)
            {
                // if we're triggered but not completed we need to flash the visuals depending on how close we are to ending the timer
                ScreenAnimator.SetFloat("State", ((currentTime >= (maxTime * .75f)) ? 2 : 1));
            }
            else if (IsCompleted)
                ScreenAnimator.SetFloat("State", 3);
            else
                ScreenAnimator.SetFloat("State", 0);
        }  
        //// Update the sprite renderer
        //if (ConsoleRenderer != null)
        //{
        //    ConsoleRenderer.sprite = (Triggered) ? Open : Closed; 
        //} 

		// If it's a timed switch, update the timer
		if(!IsCompleted && Triggered)
		{
			currentTime += Time.deltaTime;
            soundTime += Time.deltaTime;

            // Handle the clicking sound
            clickRate = (currentTime >= (maxTime * .75f)) ? .25f : .5f;
            if(soundTime > clickRate)
            {
                src.clip = clickingSound;
                src.Play();
                soundTime = 0;
            } 

            // Handle the logic
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
        {
            src.clip = toggleSound;
            src.Play(); 
        }

		for(int i = 0; i < unitHandles.Length; i++)
		{ 
			unitHandles[i].Activate();
		}
    }
}
