﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour {

    public GameObject ThrownObject;
    public GameObject RotateObject;

    [Space(10)]
    public HeldWeapon heldData;
    public WeaponInfo weaponData;
    public List<BulletBase> myBullets;

    [Space(10)] 
    public AudioClip ShootClip;

    [HideInInspector]
	public int maxBullets;
	protected int currentBullet;
    public IArmed myOwner;
	public ForceFieldScript myShield;
	public EnergyManager myEnergy;

    [HideInInspector]
    public float currentshootCD;

    GameObject player;
    [HideInInspector]
    public AudioSource myAudioSource;

    protected bool WeaponReleased;
	public bool isFiring = false;
     
 
    // Use this for initialization
    public virtual void Awake () {
        myAudioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        heldData.Initialize(this, ThrownObject, player);
        myBullets = new List<BulletBase>();

		myOwner = GetComponentInParent<IArmed>();

		if(myOwner != null)
		{
			// Set to ceiling to prevent running out of bullet objects if someone shoots constantly from full energy
			maxBullets = 10;
			myShield = myOwner.gameObject.GetComponentInChildren<ForceFieldScript>();
			myEnergy = myOwner.gameObject.GetComponent<EnergyManager>();
		}

		// Index of the bullet to be fired to stop us from firing the same bullet twice in a row
		currentBullet = maxBullets - 1;

        // Check if we're being held
        IMultiArmed isMulti = GetComponentInParent<IMultiArmed>();
        IArmed isArmed = GetComponentInParent<IArmed>();

        if(isMulti != null)
        {
            heldData.PickUp(isMulti);
        }else if(isArmed != null)
        {
            heldData.DisableMulti();
        }else
        {
            heldData.PickedUp = false;
            heldData.Toss(Vector3.down, transform.position);
        }
 
        MakeBullets();
 
	}
	
	// Update is called once per frame
	void Update () {

        if(!WeaponReleased)
        {
            ThrownObject.SetActive(!heldData.PickedUp);
            RotateObject.SetActive(heldData.PickedUp);

            if (!heldData.PickedUp)
            {
                heldData.HandlePlayerPickupRange(); 
            }
        }
         

        currentshootCD += Time.deltaTime;
	}

    private void FixedUpdate()
    {
        UpdateBullets();
    }

    public virtual void UpdateBullets()
    { 
        for(int i = 0; i < myBullets.Count; i ++)
        {
            if (myBullets[i].Fired)
            {
                myBullets[i].UpdateBullet(); 
            }
            else
            {
                if (myBullets[i].gameObject.activeInHierarchy) 
                    myBullets[i].gameObject.SetActive(false);  
            }
        }
    }
   

    public virtual void MakeBullets()
    { 

        // Ensure the bullets are properly stored
		if (myBullets.Count != maxBullets)
        {
            // If they aren't, clear the list
            foreach (BulletBase o in myBullets)
            {
                Destroy(o.gameObject);
            }

            myBullets = new List<BulletBase>();

            // Then refill it
            for (int i = 0; i < maxBullets; i++)
            { 
                GameObject newObj = (GameObject)Instantiate(weaponData.BulletObject);
                myBullets.Add(newObj.GetComponent<BulletBase>());
                newObj.SetActive(false);
            }
        }
    }

    public virtual void FireWeapon(Vector3 _dir)
    {
        // Break out if we can't even shoot
        if (currentshootCD < weaponData.fireCD)
            return;

		if (myOwner.MyUnit.CurrentEnergy < weaponData.shotCost)
            return;

        if (!myEnergy)
            myEnergy = myOwner.gameObject.GetComponent<EnergyManager>();

        if (myEnergy.BrokenEnergy)
            return;

		if(myBullets.Count == 0)
		{
			maxBullets = 10;
			MakeBullets();
			currentBullet = 0;
			if(myBullets.Count == 0)
				return;
		}
 
        // Do something based on the type
        // For now we'll just shoot one bullet

		// If we've exceeded the max number of bullets, make some more
		if(myBullets[currentBullet].gameObject.activeInHierarchy)
		{
			maxBullets += 10;
			MakeBullets();
			currentBullet = maxBullets - 1;
		}

		myBullets[currentBullet].gameObject.SetActive(true); 
		myBullets[currentBullet].myOwner = (myOwner);
		myBullets[currentBullet].myInfo = (weaponData);
		myBullets[currentBullet].transform.position = transform.position + (_dir.normalized/2);
		myBullets[currentBullet].Shoot(_dir);

        currentshootCD = 0;
        currentBullet = ((currentBullet - 1) + maxBullets) % maxBullets;
		myEnergy.ExpendEnergy(weaponData.shotCost);


        //Play the sound
        if(myAudioSource != null)
        {
            myAudioSource.clip = ShootClip;
            myAudioSource.Play();
        }

        // Check for an animator
        Animator anim = RotateObject.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Fire");

        if (myOwner != null)
            myOwner.myVisualizer.ShowMenu();

        if(weaponData.fireCD < 0)
        {
            // if our fire CD is less then 0 then it's a single use weapon
            ReleaseWeapon();
        }

        return; 
    }
 
    private void OnDisable()
    {
        if(ThrownObject != null)
            ThrownObject.SetActive(false);
    }

    private void OnDestroy()
    {
		// destroy all bullets
        for(int i = 0; i < myBullets.Count;i ++)
        {
            Destroy(myBullets[i]);
        }
    }

    private void OnEnable()
    {
        if(!heldData.PickedUp && ThrownObject != null)
        {
            ThrownObject.SetActive(true);
        }
    }

    public void ResetShootCD()
    {
        currentshootCD = 0;
    }

    /// <summary>
    /// Method called when we want to get rid of a weapon but not toss it when it's owner dies. 
    /// This will keep the weapon active in scene but it won't be usable, allowing it to update it's bullets at will
    /// </summary>
    public virtual void ReleaseWeapon()
    {
        transform.SetParent(null);
        ThrownObject.SetActive(false);
        RotateObject.SetActive(false);
        heldData.PickedUp = false;
        WeaponReleased = true;

        IMultiArmed armed = myOwner.gameObject.GetComponent<IMultiArmed>();
        if(armed != null)
        {
            armed.RemoveWeapon(this);
        }
    }
}