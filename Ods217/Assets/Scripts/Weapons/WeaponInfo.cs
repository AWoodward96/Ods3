using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public AudioClip reloadClip;


}
