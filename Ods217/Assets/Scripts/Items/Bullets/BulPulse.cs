using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Bullet Script
/// The primary fire of the player
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BulPulse : MonoBehaviour, IBullet
{


    [Header("Bullet Data")]
    public Vector3 Direction; // Which direction this bullet will move in
    public float BulletSpeed; // How fast the bullet will travel
    public float Lifetime; // How long the bullet will live
    float currentLife;

    bool Fired;
    IArmed Owner; // Which unit fired it
    SpriteRenderer myRenderer;
    BoxCollider myCollider;
    WeaponInfo myWeapon;

    // Use this for initialization
    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;

        Fired = false;
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet if it's fired
        if (Fired)
        {
            transform.position += (Direction * BulletSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(Direction));
        }

        // If it's lived too long kill it
        currentLife += Time.deltaTime;

        if (currentLife > Lifetime)
            Fired = false;

        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;
    }

    public bool CanShoot
    {
        get { return !Fired; }
    }



    // Align this object to that direction, enable it and set fired to true 
    public void Shoot(Vector3 _dir)
    {
        Direction = _dir.normalized;
        Fired = true;
        currentLife = 0;
    }

    // Used if we want to add hit effects, say an explosion whenever it hits something
    public void OnHit(GameObject _unitObj)
    {

    }

    // When this hits something
    public void OnTriggerEnter(Collider other)
    {
        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IArmed>() == Owner)
            return;
 

        // If it's not able to be damaged
        if (other.GetComponent<IDamageable>() != null)
        {
            // Hey maybe the non unit has something that it does when it's hit by a bullet
            IDamageable u = other.GetComponent<IDamageable>();
             
            u.OnHit(Owner.myWeapon.myWeaponInfo.bulletDamage);
            Fired = false;
        }

        // Either way set it's fired to false
        Fired = false;
    }

 
    public void setWeapon(WeaponInfo _weaponData)
    {
        myWeapon = _weaponData;
    }

    IArmed IBullet.Owner
    {
        get
        {
            return Owner;
        }

        set
        {
            Owner = value;
        }
    }

    public WeaponInfo WeaponData
    {
        get
        {
            return myWeapon;
        }

        set
        {
            myWeapon = value;
        }
    }
}
