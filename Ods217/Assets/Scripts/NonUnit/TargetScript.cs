using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unit Script
/// A target that's really just a punching bag for the player
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TargetScript: MonoBehaviour,IUnit {

    public UnitStruct myUnit;
    public AudioClip HitClip;
    public AudioClip DeathClip;
    Animator myAnimator;
    AudioSource mySource;


    // Use this for initialization
    void Awake() {
        myAnimator = GetComponent<Animator>();
        mySource = GetComponent<AudioSource>();
        mySource.loop = false;
        mySource.playOnAwake = false;
        myAnimator.SetFloat("Health", myUnit.CurrentHealth);
    }

      void Update()
    {

        myAnimator.SetFloat("Health", myUnit.CurrentHealth);
    }

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    // Can't really die with almost infinite health
    public void OnDeath()
    {
        Instantiate(Resources.Load("Prefabs/Particles/SimpleDeath"), transform.position, Quaternion.identity);
        //myAudioSystem.PlayAudioOneShot(DeathClip, transform.position);
        gameObject.SetActive(false);
    }

    public void OnHit(IWeapon _FromWhatWeapon)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myUnit.CurrentHealth -= _FromWhatWeapon.myWeaponInfo.bulletDamage; 
        myAnimator.SetTrigger("TakeHit");
        //mySource.clip = HitClip;
        //mySource.Play();
        myVisualizer.ShowMenu();

        myAnimator.SetFloat("Health", myUnit.CurrentHealth);

        if(myUnit.CurrentHealth <= 50)
        {
            AudioClip myClip;

            switch(myUnit.CurrentHealth)
            {
                case 30:
                    myClip = Resources.Load("Audio/SoundEffects/Target SFX/hit4") as AudioClip;
                    break;
                case 20:
                    myClip = Resources.Load("Audio/SoundEffects/Target SFX/hit3") as AudioClip;

                    break;
                case 10:
                    myClip = Resources.Load("Audio/SoundEffects/Target SFX/hit2") as AudioClip;

                    break;
                default:
                    myClip = Resources.Load("Audio/SoundEffects/Target SFX/hit1") as AudioClip; 
                    break;
            }

            myAudioSystem.PlayAudioOneShot(myClip, transform.position,1.5f);
        }


        if (myUnit.CurrentHealth <= 0)
            OnDeath();
    }

    public IWeapon MyWeapon
    {
        // Has no weapon
        get { return null; }
    }


    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }
}
