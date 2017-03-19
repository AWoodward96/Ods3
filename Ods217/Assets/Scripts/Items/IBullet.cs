using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBullet {

 
    bool CanShoot();
    void Shoot(Vector3 _dir);
    void OnHit(IUnit _unitObj);
    void setOwner(IUnit _Owner);
 
}
