using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Bullet Script
/// The primary fire of the player
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PlayerBullet : MonoBehaviour, IBullet
{ 
    [Header("Bullet Data")]
    public Vector3 Direction; // Which direction this bullet will move in
    public float BulletSpeed; // How fast the bullet will travel
    public float Lifetime; // How long the bullet will live
    float currentLife;

    bool Fired;
    IUnit Owner; // Which unit fired it
    SpriteRenderer myRenderer;
    BoxCollider myCollider;

    public Upgrades.bulletUpgradeType myUpgrades; // 0: Explosive; 1: Piercing; 2: Bouncy; anything else is nothing
    List<IUnit> HitList;

    ParticleSystem myExplosionSystem;


    // Use this for initialization
    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;

        Fired = false;
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;


        myExplosionSystem = GetComponentInChildren<ParticleSystem>();

        HitList = new List<IUnit>();
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
        HitList = new List<IUnit>();
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
            myExplosionSystem.Emit(50);
            Collider[] collateralHit = Physics.OverlapSphere(transform.position, 1);
            for(int i = 0; i < collateralHit.Length; i ++)
            {
                GameObject obj = collateralHit[i].gameObject;

                if (obj == objHit)
                    continue;

                if(obj.GetComponent<IUnit>() != null)
                {
                    IUnit u = obj.GetComponent<IUnit>();
                    u.OnHit(Owner.MyWeapon);
                }
            }
        }


        Fired = false;
    }

    // When this hits something
    public void OnTriggerEnter(Collider other)
    {
        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IUnit>() == Owner)
            return;


        // If it's a unit
        if (other.GetComponent<IUnit>() != null)
        {
            // Trigger a hit
            IUnit u = other.GetComponent<IUnit>();

            if(myUpgrades == Upgrades.bulletUpgradeType.Piercing)
            {
                if (!HitList.Contains(u)) // Check if we haven't hit this yet
                {
                    HitList.Add(u); // If we haven't then add it to the list
                    u.OnHit(Owner.MyWeapon); // Trigger a hit on it
                                             // And then trigger a hit with the bullet
                    OnHit(other.gameObject);
                }
            }else
            {
                OnHit(other.gameObject);
                u.OnHit(Owner.MyWeapon);
            }
            return;
        }

        // If it's not a unit
        if (other.GetComponent<INonUnit>() != null)
        {
            // Hey maybe the non unit has something that it does when it's hit by a bullet
            INonUnit u = other.GetComponent<INonUnit>();

            OnHit(other.gameObject);
            u.OnHit(); 

            return;
        }

        OnHit(other.gameObject); 
    }

    public void setOwner(IUnit _Owner)
    {
        Owner = _Owner;
    }
}
