using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable : IPermanent {

	UnitStruct MyUnit
    {
        get;
        set;
    } 

    // void OnMelee(int _damage);
    void OnHit(int _damage); 
}
