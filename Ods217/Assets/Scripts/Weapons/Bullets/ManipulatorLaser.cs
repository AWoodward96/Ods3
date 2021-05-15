using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

public class ManipulatorLaser : BulletBase
{
	LineRenderer sprite;

	float currentLife;

	UsableIndicator last;

	void Awake()
	{
		sprite = GetComponent<LineRenderer>();
	}

	void Update()
	{
		if(!Fired && last != null)
		{
			last.lasered = false;
		}

		sprite.enabled = Fired;
		gameObject.SetActive(Fired);
	}

	public override void UpdateBullet()
    {
		if(Fired)
		{
			transform.position = myOwner.gameObject.transform.position;

			LayerMask hittable = Physics.AllLayers;
			RaycastHit hit;

			// Snap to any nearby E-indicators
			Vector3 desired = CamScript.CursorLocation;
			Collider[] candidates = Physics.OverlapSphere(desired, 2.0f);
			for(int i = 0; i < candidates.Length; i++)
			{
				UsableIndicator myInd = candidates[i].GetComponentInChildren<UsableIndicator>();
				if(myInd != null && myInd.Laserable)
				{
					desired = candidates[i].bounds.center;
					break;
				}
			}

			Direction = (GlobalConstants.ZeroYComponent(desired - transform.position));
			Ray r = new Ray(transform.position, Direction);
			float dist = Direction.magnitude;

			Vector3 destination = transform.position;

			bool hitAnything = false;
			bool hitSolid = false;

			// A failsafe for the loop, just in case
			int failsafe = 0;

			// Do a raycast to see if the laser is going to hit anything
			do
			{
				failsafe++;

				hitAnything = Physics.Raycast(r, out hit, dist, hittable);
				if(hitAnything)
				{
					// In case we hit something we're ignoring, the loop shouldn't repeat forever
					dist -= (r.origin - hit.point).magnitude;
					r.origin = hit.point + Direction.normalized * 0.01f;

					if(hit.transform.gameObject.layer != LayerMask.NameToLayer("Bullet"))
					{
						hitSolid = true;
					}
					else
					{
						continue;
					}

					// Check to see if the object we hit has an E-indicator, and trigger it if so
					UsableIndicator myIndicator = hit.transform.GetComponentInChildren<UsableIndicator>();
					if(myIndicator != null && myIndicator.Laserable && !myIndicator.Disabled && myIndicator.Output != null)
					{
						if(myIndicator.Style == UsableIndicator.usableIndcStyle.Toggle && myIndicator.lasered)
						{
							break;
						}

						if(last != null)
						{
							last.lasered = false;
						}

						myIndicator.Output();
						myIndicator.lasered = true;
						last = myIndicator;
					}
					else if(last != null)
					{
						last.lasered = false;
					}
				}
			}
			// The loop should repeat until we hit something we care about, be it ground or an E-indicator
			// That, or until we have confirmation that it hasn't hit anything we care about.
			while(hitAnything && hit.transform.gameObject.layer == LayerMask.NameToLayer("Bullet") && failsafe < 10);

			// If we hit something, the laser should end at the contact point
			if(hitSolid)
			{
				destination = hit.point;
			}

			// Otherwise it should end at the mouse's position
			else
			{
				destination = transform.position + Direction;

				if(last != null)
				{
					last.lasered = false;
				}
			}

			destination.y = myOwner.gameObject.transform.position.y;

			// Animate line renderer here

			sprite.positionCount = 2;
			sprite.SetPosition(0, transform.position);
			sprite.SetPosition(1, destination);
		}

		if(currentLife > 0.1f)
		{
			Fired = false;
		}

		currentLife += Time.deltaTime;
    } 

    public override void Shoot(Vector3 _dir)
    {
		Direction = _dir.normalized;
		Fired = true;
		currentLife = 0.0f;
    }

    // When this hits something
	public override void OnTriggerEnter(Collider other)
    {
		// This does nothing because hits are taken care of via raycast
    }
}
