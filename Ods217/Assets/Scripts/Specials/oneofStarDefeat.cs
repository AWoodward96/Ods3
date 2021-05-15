using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A one off script for the defeat of the Star miniboss
/// </summary>
public class oneofStarDefeat : MonoBehaviour, IPermanent {

    bool triggered;
    public GameObject StarReference;
    public ParticleSystem SmokeBomb;

    ZoneScript zone;
    Animator myAnim;
    SpriteRenderer myRnd;
    AudioSource mySource;
    int triggeredCount;
    float dTime;

    void Start () {
        mySource = GetComponent<AudioSource>();
        myAnim = GetComponent<Animator>();
        myRnd = GetComponent<SpriteRenderer>();
        myRnd.enabled = false;
	}
	
	void Update () {
        myAnim.SetInteger("State", triggeredCount);

        if(triggeredCount == 4)
        {
            dTime += Time.deltaTime;
            if(dTime > 1)
            {
                SmokeBomb.gameObject.SetActive(true);
                SmokeBomb.transform.position = this.transform.position;
                SmokeBomb.Play();
                SmokeBomb.GetComponent<AudioSource>().Play();
                triggeredCount++;
                gameObject.SetActive(false);
            }
        }
    }

    public ZoneScript myZone
    {
        get
        {
            return zone;
        }

        set
        {
            zone = value;
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
            
            if (triggeredCount == 0)
            { 
                transform.position = StarReference.transform.position;
                Camera.main.GetComponent<CamScript>().Target = this.transform;
                myRnd.enabled = true;
                if (mySource != null)
                    mySource.Play();
            }

            triggeredCount++;
        }
    }
}
