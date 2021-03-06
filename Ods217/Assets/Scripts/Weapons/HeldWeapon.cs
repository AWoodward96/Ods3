﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeldWeapon {


    public enum TossType { Toss, Place };
    public TossType WillToss;
    public enum HoldType { CircleAim, Hold }
    public HoldType holdType;

    WeaponBase weaponBase;
    GameObject thrownObj;
    GameObject playerObj;
    bool initialized = false;
      
    public bool PickedUp;
    public bool DropOnInactive;
     
    Rigidbody myRigidBody;

    [HideInInspector]
    public UsableIndicator ind_MyIndicator;

    bool DEBUG = false;
    
    public void Initialize(WeaponBase _weaponBase, GameObject _thrownObj, GameObject _player)
    {
        weaponBase = _weaponBase;
        playerObj = _player;
        thrownObj = _thrownObj;

        ind_MyIndicator = thrownObj.GetComponentInChildren<UsableIndicator>();
        ind_MyIndicator.Output = PickupDelegate;
        ind_MyIndicator.Preset = UsableIndicator.usableIndcPreset.PickUp;


        myRigidBody = thrownObj.GetComponent<Rigidbody>();
        initialized = true;

        if (playerObj == null) // Ensure that we have the player
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            Physics.IgnoreCollision(thrownObj.GetComponent<Collider>(), playerObj.GetComponent<Collider>(), true);
        }


        if (DEBUG) Debug.Log("HeldWeapon: Initialize");
    }

 

    public void PickUp(IMultiArmed _multiArmedUnit)
    {
        if (!initialized)
        {
            Debug.Log("Weapon is not initialized: " + this);
            return;
        }


        // First things first, toggle the flag to say we're being used
        PickedUp = true;

        ind_MyIndicator.Disabled = true;
        if (ind_MyIndicator == UsableIndicator.Grab.ind)
            UsableIndicator.ResetInd();

        // Reset the thrownObj
        thrownObj.transform.localRotation = Quaternion.identity;
        thrownObj.transform.parent = weaponBase.transform;
        thrownObj.SetActive(false);

        _multiArmedUnit.PickUpWeapon(weaponBase);

        weaponBase.transform.parent = _multiArmedUnit.gameObject.transform;
        weaponBase.transform.localPosition = Vector3.zero;
        weaponBase.transform.localScale = Vector3.one;
        if (DEBUG) Debug.Log("HeldWeapon: Pickup");
    }

    public void DisableMulti() // The object is already parented properly and we need to disable the thrown object
    {
        if (!initialized) 
        {
            Debug.Log("Weapon is not initialized: " + this);
            return;
        } 

        // First things first, toggle the flag to say we're being used
        PickedUp = true;

        thrownObj.transform.localRotation = Quaternion.identity;
        thrownObj.transform.parent = weaponBase.transform;
        thrownObj.SetActive(false);

        // The object should already have this weapon populated via the inspector so no need to call a pickup here
    }
 
    public WeaponBase PickUp(IArmed _armedUnit)
    {
        if (!initialized)
        {
            Debug.Log("Weapon is not initialized: " + this);
            return null;
        }


        // First things first, toggle the flag to say we're being used
        PickedUp = true;

        ind_MyIndicator.Disabled = true;
        if (ind_MyIndicator == UsableIndicator.Grab.ind)
            UsableIndicator.ResetInd();
          

        // Reset the thrownObj
        thrownObj.transform.localRotation = Quaternion.identity;
        thrownObj.transform.parent = weaponBase.transform; 
        thrownObj.SetActive(false);

        weaponBase.transform.SetParent(_armedUnit.gameObject.transform);
        weaponBase.transform.localPosition = Vector3.zero;
        weaponBase.transform.rotation = Quaternion.identity;
        weaponBase.transform.localScale = Vector3.one;

        return weaponBase;
    }

    /// <summary>
    /// Thow this weapon with reckless abandon!
    /// </summary>
    /// <param name="dir">In what direction will this be tossed. Magnitutde does not matter</param>
    /// <param name="pos">Where this will be tossed from</param>
    public void Toss(Vector3 dir, Vector3 pos)
    {
        if (!initialized)
        {
            Debug.Log("Weapon is not initialized: " + this);
            return; 
        }

        Physics.IgnoreCollision(thrownObj.GetComponent<Collider>(), playerObj.GetComponent<Collider>(), true);


        thrownObj.transform.parent = null;
        thrownObj.transform.position = pos;

        // Reset the transform
        if (WillToss == TossType.Toss)
        {
            thrownObj.transform.rotation = Quaternion.Euler(90, 0, 0);
            thrownObj.transform.localScale = new Vector3(1, 1, 1);
            myRigidBody.isKinematic = false;
            myRigidBody.velocity = dir; 
            myRigidBody.angularVelocity = (new Vector3(0, 500, 0) * ((Random.Range(0, 1) == 1) ? 1 : -1));
        }

        weaponBase.transform.SetParent(null);
        weaponBase.transform.localPosition = Vector3.zero;
        weaponBase.transform.rotation = Quaternion.identity;
        //weaponBase.transform.localScale = Vector3.one;

        ind_MyIndicator.Disabled = false;

        if (DEBUG) Debug.Log("HeldWeapon: Toss");

        // We've been tossed!
        PickedUp = false;
    }

    public void HandlePlayerPickupRange()
    {
        if (!initialized)
        {
            Debug.Log("Weapon is not initialized: " + this);
            return;
        }


        if (!PickedUp)
        {
            if (playerObj == null) // Ensure that we have the player
            {
                playerObj = GameObject.FindGameObjectWithTag("Player");
                Physics.IgnoreCollision(thrownObj.GetComponent<Collider>(), playerObj.GetComponent<Collider>(), true);
            } 
            
            ind_MyIndicator.transform.rotation = Quaternion.Euler(-thrownObj.transform.rotation.x, -thrownObj.transform.rotation.y, -thrownObj.transform.rotation.z); 
 

            if(DEBUG) Debug.Log("HeldWeapon: HandlePlayerPickupRange");
        } 
    }

    void PickupDelegate()
    {
        PickUp(playerObj.GetComponent<IMultiArmed>());
    }

 
}
