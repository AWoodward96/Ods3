using System;
using System.Collections;
using SpriteToParticlesAsset;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class LobbedObject : MonoBehaviour, IBullet {
      

    bool Fired;
    IArmed Owner; // Which unit fired it
    SpriteRenderer myRenderer;
    BoxCollider myCollider;
    Rigidbody myRGB;
    WeaponInfo myWeapon;
      
    public bool Resuable;

    ParticleSystem mySystem;

    EffectorExplode explode;

    // Use this for initialization
    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
        mySystem = GetComponent<ParticleSystem>(); 
        myCollider.isTrigger = true;

        myRGB = GetComponent<Rigidbody>(); 

        Fired = false;
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;

        explode = GetComponent<EffectorExplode>();

    }

    // Update is called once per frame
    void Update()
    { 
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;

        myRenderer.flipX = false;
        myRenderer.flipY = false;
    }

    public bool CanShoot
    {
        get { return !Fired; }
    }



    // Align this object to that direction, enable it and set fired to true 
    public void Shoot(Vector3 _dir)
    {
        myRGB.velocity = GlobalConstants.getPhysicsArc(transform.position, _dir); 
        Fired = true; 
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

        explode.ExplodeAt(Vector3.down, 10, 180, 90, 1f);


        // If it's able to be damaged
        if (other.GetComponent<IDamageable>() != null)
        {
            // Hey maybe the non unit has something that it does when it's hit by a bullet
            IDamageable u = other.GetComponent<IDamageable>();
            mySystem.Play();
            u.OnHit(myWeapon.bulletDamage);

            if (Resuable) Fired = false;


            GetComponent<AudioSource>().Play();
            return;
        }


        GetComponent<AudioSource>().Play();
        mySystem.Play();
 

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
