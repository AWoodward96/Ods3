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
    void Shoot(Vector3 _dir);
    void OnHit(IUnit _unitObj);
    void setOwner(IUnit _Owner);
 
}
