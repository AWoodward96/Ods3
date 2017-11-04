using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class LobbedObject : MonoBehaviour, IBullet {
     
    [Header("Bullet Data")]
    public Vector3 Direction; // Which direction this bullet will move in   

    bool Fired;
    IArmed Owner; // Which unit fired it
    SpriteRenderer myRenderer;
    BoxCollider myCollider;
    Rigidbody myRGB;
    WeaponInfo myWeapon;

    public float DesiredAngle = 45;

    public bool Resuable;

    ParticleSystem mySystem; 

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
    }

    // Update is called once per frame
    void Update()
    { 
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



        // If it's able to be damaged
        if (other.GetComponent<IDamageable>() != null)
        {
            // Hey maybe the non unit has something that it does when it's hit by a bullet
            IDamageable u = other.GetComponent<IDamageable>();
            mySystem.Play();
            u.OnHit(Owner.myWeapon.myWeaponInfo.bulletDamage);
            if (Resuable) Fired = false;
            GetComponent<AudioSource>().Play();
            return;
        }

        // Break! 
        this.gameObject.SetActive(false);

        GetComponent<AudioSource>().Play();
        mySystem.Play();
        // Either way set it's fired to false
        if (Resuable)
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

    /// <summary>
    /// Slightly edited version. From Zolran at https://www.youtube.com/watch?v=3B5NV4bAkoE
    /// </summary>
    /// <returns></returns>
    IEnumerator SimulateProjectile(Vector3 _target)
    { 

        // Calculate distance to target
        float target_Distance = Vector3.Distance(transform.position, _target);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * DesiredAngle * Mathf.Deg2Rad) / GlobalConstants.Gravity);

        // Extract the X  Y componenent of the velocity
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(DesiredAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(DesiredAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        float flightDuration = target_Distance / Vx;

       
        float elapse_time = 0;

        while (elapse_time < flightDuration)
        {
            transform.Translate(0, (Vy - (GlobalConstants.Gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

            elapse_time += Time.deltaTime;

            yield return null;
        }
    }

}
