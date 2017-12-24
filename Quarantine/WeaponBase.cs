using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour, IWeapon {

    [Header("Held Data")]
    public GameObject myOwner;
    public UsableWeapon weaponData;

    [Space(30)]
    [Header("Weapon Data")]
    public WeaponInfo WeaponData;
    public bool Reloading;
    List<GameObject> myBullets;
     

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void UpdateBullets()
    { 
    } 

    public List<GameObject> bulList
    {
        get
        {
            return myBullets;
        }
    }

    public bool isReloading
    {
        get
        {
            return Reloading;
        }
    }

    public WeaponInfo myWeaponInfo
    {
        get
        {
            return WeaponData;
        }

        set
        {
            WeaponData = value;
        }
    }

    public IArmed Owner
    {
        get
        {
            return null;
        }

        set
        {
            
        }
    }


    public void FireWeapon(Vector3 _dir)
    {
        throw new NotImplementedException();
    }

    public void ForceReload()
    {
        throw new NotImplementedException();
    }
}



public struct UsableWeapon
{

    public enum TossType { Toss, Place };
    public TossType WillToss;
    public enum HoldType { CircleAim, Hold }
    public HoldType holdType;

    public GameObject DroppedObj;
    public GameObject HeldObj;

    public bool isHeld;
}


