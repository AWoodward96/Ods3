using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WeaponInfo
{
    public string name;
	public int shotCost;
    public float fireCD;

    public GameObject BulletObject;
    public int bulletDamage;
    public AudioClip reloadClip;


}
