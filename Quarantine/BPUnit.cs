﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This Script is for units that are like the player. They have weapons and will try to use them against you or on allies.
/// This is also a BluePrint, meaning that they should be duplicated then modified to save you time
/// </summary>
[RequireComponent(typeof(CController))]
public class BPUnit : MonoBehaviour,IUnit {

    public UnitStruct myUnit;
    public IWeapon myWeapon;
    GameObject GunObj;
    CController myCtrl;

    public Vector3 LookingVector;

    // Use this for initialization
    void Awake() {

        // Get the gun object in the child of this object
        Transform[] objs = GetComponentsInChildren<Transform>();
        foreach (Transform o in objs)
        {
            if (o.name == "GunChild")
                GunObj = o.gameObject;
        }

        myCtrl = GetComponent<CController>();


        // Initialize the weapon just in case
        if (myWeapon == null)
        {
            myWeapon = GetComponentInChildren<IWeapon>();
            myWeapon.Owner = this;
        }
        else
        {
            myWeapon.Owner = this;
        }


    }
	
	// Update is called once per frame
	void Update () {
        GunObject();	
	}


    void GunObject()
    {
        // Do everything related to the gun object here (idealy)
        // Make the gun look at where your cursor is
        // At a rotation of 0 the gun points right 

        // Where the unit is looking is in world space
        Vector3 toLook = GlobalConstants.ZeroYComponent(LookingVector) - transform.position;  
        toLook = LookingVector.normalized;
        // Alright now we need the angle between those two vectors and then rotate the object 
        GunObj.transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(LookingVector));
    }

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    public IWeapon MyWeapon
    {
       get{ return myWeapon; }
    }

    public void OnDeath()
    {
        throw new NotImplementedException();
    }

    public void OnHit(IWeapon _FromWhatWeapon)
    {  
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myUnit.CurrentHealth -= _FromWhatWeapon.myWeaponInfo.bulletDamage;
        myVisualizer.ShowMenu();
    }

    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

}
