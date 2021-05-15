using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A Special Effects Script
/// A script for footsteps controlled usually by the CController
/// </summary>
[RequireComponent(typeof(CController))]
public class FootStepScript : MonoBehaviour {

    public enum FootStepStyle { SingleTrack, MultiTrack}; // Single track means that there's only one clip that gets slightly pitch shifted every time
    public FootStepStyle Style;
    public AudioClip[] Clips; // The audio clip
    [Range(0,4)]
    public int AudioSourceCount; // However many audio sources there are. Usually 2 is good
    [Range(0, 1)]
    public float Volume;
    [Range(0, 1)]
    public float WorldSpace = 1; // If false, the footstep sound will play at the same volume reguardless of where it is
    public float Speed;
    public float MinDist = 20;
    public float MaxDist = 75;


    [HideInInspector]
    public float stepCooldown;

    AudioSource[] Sources; // A list of all the audio sources we'll add to this object at runtime
    CController myCC;
    int currentStepCount;
    

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
                    src.spatialBlend = (WorldSpace);
                    Sources[i] = src;
                    src.minDistance = MinDist;
                    src.maxDistance = MaxDist;
                    src.rolloffMode = AudioRolloffMode.Linear;
                }


                break;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
        switch(Style)
        {
            case FootStepStyle.SingleTrack:
                // alright depending on the speed
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
                // Multitrack not implemented
        }
	}
}
