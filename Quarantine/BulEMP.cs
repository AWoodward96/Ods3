using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Bullet Script
/// Used for EMP bullets which power objects rather then hurting them
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BulEMP : MonoBehaviour, IBullet
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

    IWeapon myWeapon;


    // Use this for initialization
    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
        myWeapon = GetComponent<IWeapon>();

        Fired = false;
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;
    }

    // Update is called once per frame
    void Update()
    {
        // If we're fired then move the object
        if (Fired)
        {
            transform.position += (Direction * BulletSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(Direction));
        }

        // If it's lived for too long kill it
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

    // Do nothing if it hits a unit because it's an emp - it doesn't hurt
    public void OnHit(GameObject _unitObj)
    {

    }

    public void OnTriggerEnter(Collider other)
    {

        // EMPS are special actually. When they collide with something we don't want to assign damage.
        // The EMP bullet should have it's own weapon script on it that has 0 as a damage value
        // Assign damage using that weapon as opposed to the Owners weapon

        if (other.GetComponent<IArmed>() == Owner)
            return;


        if (other.GetComponent<IArmed>() != null)
        {
            IArmed u = other.GetComponent<IArmed>(); 
            u.OnHit(myWeapon.myWeaponInfo.bulletDamage);
            Fired = false;
        }

        if (other.GetComponent<IDamageable>() != null)
        {
            IDamageable u = other.GetComponent<IDamageable>();
            u.OnEMP();
            Fired = false;
        }

        Fired = false;
    }

    public void setOwner(IArmed _Owner)
    {
        Owner = _Owner;
    }
}
