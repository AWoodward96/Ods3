using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmed : IDamageable {

    HealthBar myVisualizer
    {
        get;
    }

    WeaponBase myWeapon
    {
        get;
    }

    //void SetWeaponsOwner(WeaponBase w);
}
