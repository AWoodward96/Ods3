using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unarmed : MonoBehaviour , IWeapon{

    public WeaponInfo WeaponInfo;
    IArmed myOwner;

    // Use this for initialization
    void Start()
    {
        WeaponInfo.bulletDamage = 0;
        WeaponInfo.currentAmmo = 1;
        WeaponInfo.maxAmmo = 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<GameObject> bulList
    {
        get
        {
            return null;
        }
    }

    public bool isReloading
    {
        get
        {
            return false;
        }
    }

    public WeaponInfo myWeaponInfo
    {
        get
        {
            return WeaponInfo;
        }

        set
        {
            WeaponInfo = value;
            WeaponInfo.bulletDamage = 0;
            WeaponInfo.currentAmmo = 1;
            WeaponInfo.maxAmmo = 1;
        }
    }

    public IArmed Owner
    {
        get
        {
            return myOwner;
        }

        set
        {
            myOwner = value; 
        }
    } 

    public void FireWeapon(Vector3 _dir)
    {
        
    }

    public void ForceReload()
    {
         
    }


}
