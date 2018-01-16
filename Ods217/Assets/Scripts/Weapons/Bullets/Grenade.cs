using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Grenade : BulletBase
{

    Rigidbody myRigidbody;
    [Space(20)]
    [Header("Grenade Data")]
    public GameObject AlertIndicator;
    public ParticleSystem ExplosionParticle;
    
    [Tooltip("This refers to the size of the alert asset. If an alert asset is 32x32 it should be 1. 48x48: 1.5, etc etc")] public float NativeAlertIndicatorRatio = 1.5f;
    public float ExplosionSize = 1.5f;
 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_dir">Direction is where the object should land</param>
    public override void Shoot(Vector3 _dir)
    {
        // Get a the point where we'll land
        Direction = _dir + transform.position;
        Ray r = new Ray(Direction, Vector3.down);
        RaycastHit hit;
        // Raycast down 
        if(Physics.Raycast(r,out hit,50,LayerMask.GetMask("Ground")))
        { 
            Direction = hit.point;
        }

        // Set the arc
        Rigidbody myRgb = GetComponent<Rigidbody>();
        myRgb.velocity = (GlobalConstants.getPhysicsArc(transform.position, Direction));
        myRgb.angularVelocity = (new Vector3(0, 190 * Random.Range(-1f, 1f), 0));

        // Put down the alert indicator
        AlertIndicator.SetActive(true);
        AlertIndicator.transform.parent = null;
        AlertIndicator.transform.position = Direction + new Vector3(0, .1f, 0);
        AlertIndicator.transform.localScale = new Vector3(ExplosionSize / 1.5f, ExplosionSize / 1.5f, ExplosionSize / 1.5f);
         
        Fired = true;
    }

    public override void UpdateBullet()
    {
        // do nothing. After shooting we should already have the force we need to reach our destination
    }

    public override void OnTriggerEnter(Collider other)
    {

        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IArmed>() == myOwner)
            return;

        if (other.GetComponent<BulletBase>() != null)
            return;

        AlertIndicator.transform.SetParent(transform);
        AlertIndicator.SetActive(false);

        // Set the explosion particle at the new location
        ExplosionParticle.transform.SetParent(null);
        ExplosionParticle.transform.position = this.transform.position;
        ExplosionParticle.transform.localScale = new Vector3(ExplosionSize, ExplosionSize, ExplosionSize);
        ExplosionParticle.Play();

        // Play the explosion sound effect if possible
        AudioSource explosionSource = ExplosionParticle.GetComponent<AudioSource>();
        if (explosionSource != null) 
            explosionSource.Play(); 

        // See if we hit anyone
        Collider[] cols = Physics.OverlapSphere(transform.position, ExplosionSize);
        IDamageable foo;
        CController cc;
        for (int i = 0; i < cols.Length; i ++)
        {
            foo = cols[i].GetComponent<IDamageable>();
            if (foo != null)
            {
                foo.OnHit(myInfo.bulletDamage);
            }

            cc = cols[i].GetComponent<CController>();
            if(cc != null)
            {
                if (!cc.Immovable)
                {
                    Vector3 dist = cc.transform.position - transform.position;
                    float strength = (1.5f - dist.magnitude) * -10;
                    cc.ApplyForce(dist * strength);
                } 
            }
        }
 
        // Either way set it's fired to false
        Fired = false;
    }
}
