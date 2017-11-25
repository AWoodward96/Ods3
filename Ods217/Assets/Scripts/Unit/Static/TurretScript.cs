using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holy shit this is simultaniously a non unit and a unit
/// AAAAAAAA I'M FIXING IT
/// </summary>
public class TurretScript : MonoBehaviour, IArmed {

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

 
    
    public HealthBar myVisualizer
    {
        get
        {
            return myHealthBar;
        }
    }

    IWeapon IArmed.myWeapon
    {
        get
        {
            return myWeapon;
        }
    }

    public int Health
    {
        get
        {
            return 10;
        }

        set
        {
           // do nothing
        }
    }

    public UnitStruct MyUnit
    {
        get
        {
            return myUnit;
        }

        set
        {
            myUnit = value;
        }
    }

    public void OnHit(int _damage)
    {
        // Don't do anything because this object can't be hurt
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public virtual void TossWeapon()
    {

    }


    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
