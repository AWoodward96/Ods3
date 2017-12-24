using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Bullet Script
/// A blank melee script. Literally exists to do nothing and will disable itself if initialized
/// Used on spiders who make you take damage when you're hit by their jump meaning they have no projectile to hurt you with
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BlankMelee : MonoBehaviour, IBullet
{

    IArmed myOwner;

    // Use this for initialization
    void Awake()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CanShoot
    {
        get { return true; }
    }

    public IArmed Owner
    {
        get
        {
            return myOwner;
        }

        set
        {
            myOwner = value;
        }
    }

    public WeaponInfo WeaponData
    {
        get
        {
            WeaponInfo wi = new WeaponInfo();
            wi.bulletDamage = 0;
            wi.BulletObject = null;
            wi.currentAmmo = 0;
            wi.fireCD = 10;
            wi.reloadSpeed = 10;
            wi.name = "Melee";
            return wi;
        }

        set
        {
            
        }
    }

    public void Shoot(Vector3 _dir)
    {

    }

    public void OnHit(GameObject _unitObj)
    {

    }

    public void setOwner(IArmed _Owner)
    {

    }

    public void setWeapon(WeaponInfo _weaponData)
    {
        
    }
}
