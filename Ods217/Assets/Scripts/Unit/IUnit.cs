using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit  {

    void OnDeath();
    void OnHit(Weapon _FromWhatWeapon);
    UnitStruct MyUnit();
    Weapon MyWeapon();

    HealthVisualizer myVisualizer
    {
        get;
    }
}
