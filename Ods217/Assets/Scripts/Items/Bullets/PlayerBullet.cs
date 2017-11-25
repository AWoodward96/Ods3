using System;
using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

/// <summary>
/// A Bullet Script
/// The primary fire of the player
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class PlayerBullet : MonoBehaviour, IBullet
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

    public Upgrades.bulletUpgradeType myUpgrades; // 0: Explosive; 1: Piercing; 2: Bouncy; anything else is nothing
    List<IDamageable> HitList;

    ParticleSystem myExplosionSystem;

    AudioSource mySource;
    public AudioClip ExplosionClip;
    public AudioClip HitClip;

    WeaponInfo myWeapon; 
    EffectorExplode explode;


    // Use this for initialization
    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
        mySource = GetComponent<AudioSource>();
        myCollider.isTrigger = true;

        Fired = false;
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;


        myExplosionSystem = GetComponentInChildren<ParticleSystem>();

        HitList = new List<IDamageable>();
         
            explode = GetComponent<EffectorExplode>();
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

    // Align this object to that direction, enable it and set fired to true 
    public void Shoot(Vector3 _dir)
    { 
             GetComponent<ParticleSystem>().Clear(); 

        HitList = new List<IDamageable>();
        Direction = _dir.normalized;
        Fired = true;
        currentLife = 0;
    }

    // Used if we want to add hit effects, say an explosion whenever it hits something
    public void OnHit(GameObject objHit)
    {
        if (myUpgrades == Upgrades.bulletUpgradeType.Bouncy)
        {
            if (Mathf.Abs(Direction.z) > Mathf.Abs(Direction.x))
            {
                Direction = new Vector3(Direction.x, Direction.y, -Direction.z);
            }
            else
            {
                Direction = new Vector3(-Direction.x, Direction.y, Direction.z);
            }

            return;
        }

        if(myUpgrades == Upgrades.bulletUpgradeType.Explosive)
        {
            mySource.clip = ExplosionClip;
            mySource.Play();

            myExplosionSystem.Emit(50);
            Collider[] collateralHit = Physics.OverlapSphere(transform.position, 1);
            for(int i = 0; i < collateralHit.Length; i ++)
            {
                GameObject obj = collateralHit[i].gameObject;

                if (obj == objHit)
                    continue;

                if(obj.GetComponent<IArmed>() != null)
                {
                    IArmed u = obj.GetComponent<IArmed>();
                    u.OnHit(myWeapon.bulletDamage);
                }
            }
        }


        Fired = false;
    }

    // When this hits something
    public void OnTriggerEnter(Collider other)
    {

        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IArmed>() == Owner)
            return;



        // If it's a unit
        if (other.GetComponent<IDamageable>() != null)
        {
            // Trigger a hit
            IDamageable hit = other.GetComponent<IDamageable>();

            if (myUpgrades == Upgrades.bulletUpgradeType.Piercing)
            {
                if (!HitList.Contains(hit)) // Check if we haven't hit this yet
                {
                    HitList.Add(hit); // If we haven't then add it to the list
                    hit.OnHit(myWeapon.bulletDamage); // Trigger a hit on it
                                                      // And then trigger a hit with the bullet
                                                      //OnHit(other.gameObject); 

                }
            }
            else
            {
                OnHit(other.gameObject);
                hit.OnHit(myWeapon.bulletDamage); // Trigger a hit on it
            }

            explode.ExplodeAt(Vector3.right, 10, 180, 90, 1);
            return;
        }

        mySource.clip = HitClip;
        mySource.Play();
        explode.ExplodeAt(Vector3.right, 10, 180, 90, 1);
        OnHit(other.gameObject);

        return;
    }
 
}
