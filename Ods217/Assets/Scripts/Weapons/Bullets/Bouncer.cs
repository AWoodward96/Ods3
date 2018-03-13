using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

public class Bouncer : BulletBase
{
	Vector3 newDirection;	// The direction this bullet will turn when it hits the object ahead of it

	float lifetime;	// Number of seconds the bullet will live for

	/*void FixedUpdate()
	{
		RaycastHit hit;

		//if(Physics.Raycast(transform.position, Direction.normalized, out hit, Speed, LayerMask.NameToLayer("Ground")))
		if(Physics.Raycast(transform.position, Direction, out hit))
		{
			newDirection = Vector3.Reflect(Direction, hit.normal);
		}
	}*/

	public override void UpdateBullet()
	{
		base.UpdateBullet();

		lifetime -= Time.deltaTime;

		RaycastHit hit;

		//if(Physics.Raycast(transform.position, Direction.normalized, out hit, Speed, LayerMask.NameToLayer("Ground")))
		if(Physics.Raycast(transform.position, Direction, out hit))
		{
			newDirection = Vector3.Reflect(Direction, hit.normal);
		}

		if(lifetime <= 0.0f)
		{
			BulletDeath();
		}
	}

	public override void Shoot(Vector3 _dir)
	{
		base.Shoot(_dir);

		lifetime = 2.0f;

		newDirection = Vector3.zero;
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

		if(newDirection != Vector3.zero)
		{
			Direction = newDirection;
		}

		newDirection = Vector3.zero;
	}
}
