using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

public class BulletBase : MonoBehaviour {

    public bool Fired;
    public Vector3 Direction;
    public float Speed;

    public IArmed myOwner;
    public WeaponInfo myInfo;

    EffectorExplode explode;
 
	public virtual void UpdateBullet()
    {
        transform.position += (Direction.normalized * Speed) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(Direction));
    } 

    public virtual bool CanShoot
    {
        get
        {
            return !Fired;
        }
    }

    public virtual void Shoot(Vector3 _dir)
    {
        Direction = _dir;
        GetComponent<SpriteRenderer>().enabled = true;
        Fired = true;
    }

    // When this hits something
    public virtual void OnTriggerEnter(Collider other)
    {
        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IArmed>() == myOwner)
            return;


        // If it's not able to be damaged
        if (other.GetComponent<IDamageable>() != null)
        {
            // Hey maybe the non unit has something that it does when it's hit by a bullet
            IDamageable u = other.GetComponent<IDamageable>();

            u.OnHit(myInfo.bulletDamage);  
        }

        BulletDeath();

    }

    // What should the bullet do when it's destroyed?
    public virtual void BulletDeath()
    {
        if (explode == null) explode = GetComponentInChildren<EffectorExplode>();

        if (explode != null)
        {
            explode.transform.position = transform.position;
            explode.transform.SetParent(null);
            explode.ExplodeAt(new Vector3(2, 0, 0), 6, 360, 0, 1);
        }

        // Either way set it's fired to false
        Fired = false;
    }

}
