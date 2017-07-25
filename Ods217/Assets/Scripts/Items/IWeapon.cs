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

    IUnit Owner
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


[System.Serializable]
public class WeaponInfo
{
    public string name;
    public int currentAmmo;
    public int maxAmmo;
    public float reloadSpeed;
    public float fireCD;

    public GameObject BulletObject;
    public int bulletDamage;


}


