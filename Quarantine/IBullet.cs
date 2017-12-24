using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface for bullet objects
/// </summary>
public interface IBullet {

 
    bool CanShoot
    {
        get; 
    }

    void Shoot(Vector3 _dir); // Every bullet script should have a shoot method, thats called by the weapon whenever it's fired 

    IArmed Owner
    {
        get;
        set;
    }

    WeaponInfo WeaponData
    {
        get;
        set;
    } 
}
