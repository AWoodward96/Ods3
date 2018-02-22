using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Just a mob that you can damage. It doesn't do anything. Build upon it for more complex enemies
/// </summary>

public class mobIdleStatic : MonoBehaviour, IMultiArmed {

    public UnitStruct UnitData;
    public ZoneScript z;

    public WeaponBase currentWeapon;
    public WeaponBase WeaponSlot1;
    public WeaponBase WeaponSlot2;

    ForceFieldScript myForceField;

    // Use this for initialization
    void Start()
    {
        myForceField = GetComponentInChildren<ForceFieldScript>();
    }
    

    public UnitStruct MyUnit
    {
        get
        {
            return UnitData;
        }

        set
        {
            UnitData = value;
        }
    }

    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

    public WeaponBase myWeapon
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public ZoneScript myZone
    {
        get
        {
            return z;
        }

        set
        {
            z = value;
        }
    }

    public void TossWeapon(Vector3 _dir)
    {
        StartCoroutine(toss(_dir));
    }

    IEnumerator toss(Vector3 _dir)
    {
        yield return new WaitForEndOfFrame();

        currentWeapon.heldData.Toss(_dir, transform.position);
        if (WeaponSlot2 == currentWeapon)
        {
            // Call the toss method
            currentWeapon = WeaponSlot1;
            WeaponSlot2 = null;
        }
        else if (WeaponSlot1 == currentWeapon)
        {
            currentWeapon = WeaponSlot2;
            WeaponSlot1 = null;
        }

        myVisualizer.BuildAmmoBar();


    }

    public virtual bool Triggered
    {
        get
        {
            return false;
        }

        set
        {

        }
    }

    public virtual void Activate()
    {

    }
    public void OnHit(int _damage)
    {
        if (myZone != ZoneScript.ActiveZone) // Don't let them take damage if you're not in their scene
            return;
         

        // Firstly show the health bar (Remove this when we have the on-screen healthbar)
        myVisualizer.ShowMenu();
        if (myForceField != null)
        {
            if (MyUnit.CurrentEnergy > 0)
                myForceField.RegisterHit(_damage);
            else
            {
                UnitData.CurrentHealth -= _damage;
            }
        }

        if (UnitData.CurrentHealth <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void PickUpWeapon(WeaponBase _newWeapon)
    {
        //// First get a reference to the weapon itself
        if (_newWeapon == null)
            return;

        _newWeapon.myOwner = this; // Set it's owner
        if (WeaponSlot1 == null)
        {
            WeaponSlot1 = _newWeapon;
            myVisualizer.BuildAmmoBar(); // Let the visualizer know
            currentWeapon = WeaponSlot1;
            return;
        }

        if (WeaponSlot1 != null && WeaponSlot2 == null)
        {
            WeaponSlot2 = _newWeapon;
            myVisualizer.BuildAmmoBar(); // Let the visualizer know
            currentWeapon = WeaponSlot2;
            return;
        }

        // If we're full, toss the weapon that we're currently holding and replace it
        if (WeaponSlot1 != null && WeaponSlot2 != null)
        {
            // Toss the current active weapon
            currentWeapon.myOwner = null;
            currentWeapon.heldData.Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position), transform.position);
            if (currentWeapon == WeaponSlot2)
            {
                WeaponSlot2 = _newWeapon;
                currentWeapon = WeaponSlot2;
                myVisualizer.BuildAmmoBar();
            }

            if (currentWeapon == WeaponSlot1)
            {
                WeaponSlot1 = _newWeapon;
                currentWeapon = WeaponSlot1;
                myVisualizer.BuildAmmoBar();
            }

        } 

    }

    public void RemoveWeapon(WeaponBase _Weapon)
    {

        if (_Weapon == WeaponSlot1)
        { WeaponSlot1 = null; currentWeapon = WeaponSlot2; }

        if (_Weapon == WeaponSlot2)
        { WeaponSlot2 = null; currentWeapon = WeaponSlot1; }

        if (myWeapon != null)
            myWeapon.ResetShootCD();

        myVisualizer.BuildAmmoBar();
    }
 


}
