using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An automated shooting turret
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
    WeaponBase myWeapon;  
   

    // Use this for initialization
    void Awake () {
        onOffLight = GetComponentInChildren<Light>();
        mySystem = GetComponentInChildren<ParticleSystem>();
        myWeapon = GetComponentInChildren<WeaponBase>();
        myHealthBar = GetComponentInChildren<HealthBar>();
        mySource = GetComponent<AudioSource>();
        myWeapon.myOwner = this;
	}
	
	// Update is called once per frame
	void Update () {
        onOffLight.enabled = isTriggered;
        if(isTriggered)
        {
            // pingpong the onofflight thing
            onOffLight.intensity = .5f + Mathf.PingPong(Time.time, 1);

            myWeapon.FireWeapon(Direction);

			// This code might need to be modified because of the switch to the energy system
            /*if (myWeapon.Reloading)
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
            }*/
        }else
        {
            mySystem.Stop(); 
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

    WeaponBase IArmed.myWeapon
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
