using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TossedProjectile : BulletBase
{

    Rigidbody myRigidbody;
    [Space(20)]
    [Header("Tossed Data")]
    public GameObject AlertIndicator;
    bool broken;
    

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

        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody>();

        // Set the arc
        myRigidbody.velocity = (GlobalConstants.getPhysicsArc(transform.position, Direction));
        myRigidbody.angularVelocity = (new Vector3(0, 190 * Random.Range(-1f, 1f), 0));

        // Put down the alert indicator
        AlertIndicator.SetActive(true);
        AlertIndicator.transform.parent = null;
        AlertIndicator.transform.position = Direction + new Vector3(0, .1f, 0);

        // Make sure you're unable to be effected by your thrower
        //Physics.IgnoreCollision(this.GetComponent<Collider>(), myOwner.gameObject.GetComponent<Collider>());
         

        Fired = true;
    }

    public override void UpdateBullet()
    {
        // do nothing. After shooting we should already have the force we need to reach our destination
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (broken)
            return;

        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IArmed>() == myOwner)
            return;

        if (other.GetComponent<BulletBase>() != null)
            return;

       


        // Set off the exploder
        EffectorExplode explode = GetComponentInChildren<EffectorExplode>();
        if (explode != null)
        {
            explode.transform.SetParent(null);
            explode.transform.position = transform.position;
            explode.ExplodeAt(new Vector3(0, -1, 0), 10, 180, 0, 2f);

            // Set off the audio thing 
            AudioSource source = explode.GetComponent<AudioSource>();
            if (source != null)
                source.Play();
        }

  
        AlertIndicator.transform.SetParent(transform);
        AlertIndicator.SetActive(false);

        IDamageable foo = other.GetComponent<IDamageable>();
        if (foo != null)
        {
            foo.OnHit(myInfo.bulletDamage);
        }

        Fired = false;
    }

    IEnumerator ResetBullet()
    {
        yield return new WaitForSeconds(1);
        Fired = false;
        broken = false;
    }
}
