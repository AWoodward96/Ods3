using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CController))]
public class FootStepScript : MonoBehaviour {

    public enum FootStepStyle { SingleTrack, MultiTrack};
    public FootStepStyle Style;
    public AudioClip[] Clips;
    [Range(0,4)]
    public int AudioSourceCount;
    [Range(0, 1)]
    public float Volume;
    public float Speed;
    public bool WorldSpace;

    AudioSource[] Sources;
    int currentStepCount;
    [HideInInspector]
    public float stepCooldown;
    CController myCC;


	// Use this for initialization
	void Start () {
        myCC = GetComponent<CController>();

        switch (Style)
        {
            case FootStepStyle.SingleTrack:
                // Generate a bunch of audio sources with the parameters listed and dump the first clip in.
                Sources = new AudioSource[AudioSourceCount];
                for (int i = 0; i <AudioSourceCount; i++)
                {
                    AudioSource src = gameObject.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    src.loop = false;
                    src.clip = Clips[0];
                    src.volume = Volume;
                    src.spatialBlend = (WorldSpace) ? 1 : 0;
                    Sources[i] = src;
                }


                break;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
        switch(Style)
        {
            case FootStepStyle.SingleTrack:
                // alright depending on the speec
                if(GlobalConstants.ZeroYComponent(myCC.Velocity).magnitude > 1f && !myCC.Airborne)
                {
                    float rnd = Random.Range(0f, .2f) - .1f;
                    stepCooldown += Time.deltaTime;
                    if (stepCooldown > Speed)
                    {
                        stepCooldown = 0;
                        Sources[currentStepCount].Play();
                        Sources[currentStepCount].pitch = 1 + rnd;
                        currentStepCount++;
                        if(currentStepCount >= AudioSourceCount)
                        {
                            currentStepCount = 0;
                        }
                    }
                }


                break;
        }
	}
}
