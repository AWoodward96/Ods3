using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IWeapon {

    WeaponInfo myWeaponInfo
    {
        get;
        set;
    }

    List<GameObject> bulList
    {
        get;
    }

    IArmed Owner
    {
        get;
        set;
    }

    bool isReloading
    {
        get;
    }

    void FireWeapon(Vector3 _dir);
    void ForceReload();
}


