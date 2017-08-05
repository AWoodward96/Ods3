using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holy shit this is simultaniously a non unit and a unit
/// AAAAAAAA
/// </summary>
public class TurretScript : MonoBehaviour, INonUnit, IUnit {

    public bool isTriggered;
    public float FireRate;
    public UnitStruct myUnit;
    public Vector3 Direction;

    public AudioClip PowerUp;

    ParticleSystem mySystem;
    AudioSource mySource;
    HealthBar myHealthBar;
    Light onOffLight; 
    IWeapon myWeapon;
    float fireCD;
    bool reloadinginprocess;
   

    // Use this for initialization
    void Awake () {
        onOffLight = GetComponentInChildren<Light>();
        mySystem = GetComponentInChildren<ParticleSystem>();
        myWeapon = GetComponentInChildren<IWeapon>();
        myHealthBar = GetComponentInChildren<HealthBar>();
        mySource = GetComponent<AudioSource>();
        myWeapon.Owner = this;
	}
	
	// Update is called once per frame
	void Update () {
        onOffLight.enabled = isTriggered;
        if(isTriggered)
        {
            // pingpong the onofflight thing
            onOffLight.intensity = .5f + Mathf.PingPong(Time.time, 1);

            myWeapon.FireWeapon(Direction);


            if (myWeapon.isReloading)
            {
                if (!reloadinginprocess)
                {
                    reloadinginprocess = true;
                    mySystem.Play();
                    mySource.clip = PowerUp;
                    mySource.Play();
                }
            }
            else
            {
                mySystem.Stop();
                mySource.Stop();
                reloadinginprocess = false;
            }
        }else
        {
            mySystem.Stop();
            reloadinginprocess = false;
        }

        
        
       

	}

    public bool Powered
    {
        get
        {
            return false;
        }

        set
        {

        }
    }

    public bool Triggered
    {
        get
        {
            return isTriggered;
        }

        set
        {
            isTriggered = value;
        }
    }

    public UnitStruct MyUnit
    {
        get
        {
            return myUnit;
        }
    }

    public IWeapon MyWeapon
    {
        get
        {
            return myWeapon;
        }
    }

    
    public HealthBar myVisualizer
    {
        get
        {
            return myHealthBar;
        }
    }

    public void OnEMP()
    {
        return;
    }

    public void OnHit()
    {
        return;
    }

    public void OnDeath()
    {
        return;
    }

    public void OnHit(IWeapon _FromWhatWeapon)
    {
        return;
    }
}
