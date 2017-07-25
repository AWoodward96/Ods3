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
    void OnHit(GameObject objHit); // Every bullet script should have an OnHit method that tells the bullet what to do when it hits something
    void setOwner(IUnit _Owner); // This method is required so that you don't have instances where the bullet hits the person shooting the bullet. This method is usually called from the weaon script
 
}
