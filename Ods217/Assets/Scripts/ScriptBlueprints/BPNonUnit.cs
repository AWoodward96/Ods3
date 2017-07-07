using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This Script is for objects that are not controllable or movable but should be considered a unit for bullets sakes (IE Targets)
/// This is also a BluePrint, meaning that they should be duplicated then modified to save you time
/// </summary>
public class BPNonUnit: MonoBehaviour,IUnit {

    public UnitStruct myUnit;

    // No Character Controller If you want it require it

    // Use this for initialization
    void Awake() {
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void OnDeath()
    {
        throw new NotImplementedException();
    }

    public void OnHit(Weapon _FromWhatWeapon)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
        myVisualizer.ShowMenu();
    }

    public Weapon MyWeapon
    { 
        get { return null; }
    }

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }



    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }
}
