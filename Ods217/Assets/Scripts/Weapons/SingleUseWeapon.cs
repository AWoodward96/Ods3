using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleUseWeapon : WeaponBase {


    public override void FireWeapon(Vector3 _dir)
    {
        base.FireWeapon(_dir);

        ReleaseWeapon();
    }
 
}
