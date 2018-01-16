using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMultiArmed : IArmed
{
    void TossWeapon(Vector3 _dir);
    void PickUpWeapon(WeaponBase _Weapon);
    void RemoveWeapon(WeaponBase _Weapon);

}
