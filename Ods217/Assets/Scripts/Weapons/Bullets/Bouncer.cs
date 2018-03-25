using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

public class Bouncer : BulletBase
{
	float lifetime;	// Number of seconds the bullet will live for

	public override void UpdateBullet()
	{
		RaycastHit hit;

		if(Physics.Raycast(new Ray(transform.position, Direction.normalized), out hit, Speed * Time.deltaTime))
		{
			// Bullets should *not* bounce off of each other
			if(!hit.transform.GetComponent<BulletBase>())
			{
				transform.position = hit.point;
				Direction = Vector3.Reflect(Direction, hit.normal);
			}
			else
			{
				transform.position += (Direction.normalized * Speed) * Time.deltaTime;
			}
		}
		else
		{
			transform.position += (Direction.normalized * Speed) * Time.deltaTime;
		}

		transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(Direction));

		lifetime -= Time.deltaTime;

		if(lifetime <= 0.0f)
		{
			BulletDeath();
		}
	}

	public override void Shoot(Vector3 _dir)
	{
		base.Shoot(_dir);

		lifetime = 2.0f;
	}

	public override void OnTriggerEnter(Collider other)
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
	}
}
