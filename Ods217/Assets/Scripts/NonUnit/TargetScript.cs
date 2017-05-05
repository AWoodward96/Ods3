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
    Animator myAnimator;
    AudioSource mySource;


    // Use this for initialization
    void Awake() {
        myAnimator = GetComponent<Animator>();
        mySource = GetComponent<AudioSource>();
        mySource.loop = false;
        mySource.playOnAwake = false;
    }
	

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    // Can't really die with almost infinite health
    public void OnDeath()
    {
        Destroy(this.gameObject);
    }

    public void OnHit(Weapon _FromWhatWeapon)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage; 
        myAnimator.SetTrigger("TakeHit");
        mySource.clip = HitClip;
        mySource.Play();
        myVisualizer.ShowMenu();
    }

    public Weapon MyWeapon
    {
        // Has no weapon
        get { return null; }
    }


    public HealthVisualizer myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthVisualizer>();
        }
    }
}
