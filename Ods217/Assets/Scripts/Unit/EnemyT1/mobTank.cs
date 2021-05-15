using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mobTank : AIStandardUnit
{
	[Header("Tank Enemy")]

	public Transform patrolPointsParent;
	public GameObject hook;
	LineRenderer chain;
	MeleeWeapon myMelee;

	public Vector3 lookingVector;

	// TODO: This is for debugging only!
	//Light myFOVCone;
	Vector3[] patrolPoints;
	public int currentPoint;
	public bool walksPath;
	public bool pathLoops;
	public bool chasePlayer;
	public bool usesHook;
	int direction;
	public float lookTime;

	public enum BulletHandling { Destroy, Reflect, Absorb };
	public BulletHandling bulletHandling;

	float timer;
	static float stunLength = 2.0f;

	float rangeTimer;
	public float hookCooldown;
	bool playerHooked;
	IEnumerator undoPlayerStun;

	// How fast the tank can turn immediately after an attack, in radians
	static float cooldownTurnSpeed = 60.0f;

	float dashTimer;
	static float dashCooldownTime = 3.0f;
	static float dashLength = 0.25f;

	public float hookRange;
	static float hookDeadzone = 8.0f;
	bool isAttacking;
	bool preparingHook;
	static float meleeTelegraph = 0.5f;
	static float meleeCDLength = 3.0f;		// CAVEAT: Length in seconds *from the beginning of the swing.*
	float meleeCD;

	static float hookTurnDefault = 300.0f;
	float hookTurn = 300.0f;
	static float hookThrowSpeed = 33.0f;
	static float hookDrawSpeed = 50.0f;

    [Space(10)]
    [Header("References")]
    public GameObject Shield;
    public ParticleSystem DashPart;
    ParticleSystem.EmissionModule dashEmit;
    public AudioClip[] AudioClips;
    AudioSource mySource;
    public bool DEBUG;

	public override void Start()
	{
		chain = hook.GetComponent<LineRenderer>();
        mySource = GetComponent<AudioSource>();
		myMelee = WeaponSlot1.GetComponent<MeleeWeapon>();

		patrolPoints = new Vector3[patrolPointsParent.childCount];
		for(int i = 0; i < patrolPoints.Length; i++)
		{
			patrolPoints[i] = patrolPointsParent.GetChild(i).position;
		}
		direction = 1;
			
		if(lookingVector == Vector3.zero)
		{
			lookingVector = Vector3.down;
		}

		base.Start();

		timer = 0.0f;

		rangeTimer = hookCooldown / 2.0f;

		dashTimer = 0.0f;

		meleeCD = meleeCDLength;

		playerHooked = false;

		isAttacking = false;
		preparingHook = false;

        if (DashPart != null)
        {
            dashEmit = DashPart.emission;
            dashEmit.enabled = false;
        }
	}

	public override void FixedUpdate()
	{
		if(AIState == EnemyAIState.Stunned)
		{
			timer += Time.fixedDeltaTime;
			if(timer >= stunLength)
			{
				AIState = EnemyAIState.Aggro;
				myAnimator.SetBool("Stunned", false);
				myEmojis.Fired = false;
			}
		}

		base.FixedUpdate();
	}

	public override bool CheckAggro()
	{
		Vector3 distVec = playerRef.transform.position - transform.position;

		// Check if the player is hypothetically in range
		if (distVec.magnitude <= AggroRange)
		{
			// Check if our view of the player isn't blocked by something
			Ray r = new Ray(transform.position, distVec.normalized);
			if(!Physics.Raycast(r, distVec.magnitude, LayerMask.GetMask("Ground")))
			{
				moveAI.ActionComplete = true;
				return true;
			}
		}

		return false;
	}

	public override void IdleState()
	{
		// Check if we go aggro!
		base.IdleState();

		// If not, walk around!
		if(walksPath)
		{
			// We've reached the point
			if(moveAI.ActionComplete && moveAI.curType == MovementAI.MovementAIType.MoveTo)
			{
				if(pathLoops)
				{
					currentPoint = (currentPoint + 1) % patrolPoints.Length;
				}
				else
				{
					if(currentPoint + direction >= patrolPoints.Length || currentPoint + direction < 0)
					{
						direction *= -1;	
					}
					currentPoint += direction;

					// Exception; if there's only one patrol point, we have to manually set it to 0.
					if(currentPoint >= patrolPoints.Length || currentPoint < 0)
					{
						currentPoint = 0;
					}
				}
			}

			MoveTo(patrolPoints[currentPoint]);
		}
		else
		{
			timer += Time.fixedDeltaTime;

			// We're looking at the point
			if(Vector3.Angle(lookingVector, GlobalConstants.ZeroYComponent(patrolPoints[currentPoint] - transform.position)) <= 5.0f)
			{
				if(timer >= lookTime)
				{
					timer = 0.0f;

					if(pathLoops)
					{
						currentPoint = (currentPoint + 1) % patrolPoints.Length;
					}
					else
					{
						if(currentPoint + direction >= patrolPoints.Length || currentPoint + direction < 0)
						{
							direction *= -1;
						}
						currentPoint += direction;

						// Exception; if there's only one patrol point, we have to manually set it to 0.
						if(currentPoint >= patrolPoints.Length || currentPoint < 0)
						{
							currentPoint = 0;
						}
					}
				}
			}
		}

		lookingVector = GlobalConstants.ZeroYComponent(patrolPoints[currentPoint] - transform.position);
	}

	public override void VulnState()
	{
		moveAI.ActionComplete = true;
		lookingVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

		base.VulnState();
	}

	public override void AggroState()
	{
		dashTimer += Time.fixedDeltaTime;
		rangeTimer += Time.fixedDeltaTime;
		meleeCD += Time.fixedDeltaTime;

		if(SeePlayer())
		{
			// Beeline!
			moveAI.ActionComplete = true;
			if(!(isAttacking || WeaponSlot1.isFiring) && !preparingHook && chasePlayer)
			{
				// Where's the player relative to us?
				Vector3 distVec = GlobalConstants.ZeroYComponent(transform.position - playerRef.transform.position);

				Vector3 desired;

				// If we use our hook, we want to be in hooking range, since why risk melee range when you can reel 'em in?
				// We want them right in the middle of our hook range, since that gives them the lowest chance of escape
				if(usesHook)
				{
					desired = playerRef.transform.position + (distVec.normalized * (hookDeadzone + ((hookRange - hookDeadzone) / 2.0f)));
				}

				// Otherwise, we want to move to the nearest point we can melee attack the player
				// (0.5 units closer because we don't want to be on the tangent line)
				else
				{
					desired = playerRef.transform.position + (distVec.normalized * (myMelee.range - 0.5f));
				}

				// If we're too close, too far, or the player's in our weak spot, sprint!
				if(((transform.position - desired).sqrMagnitude >= 3.0f 
					|| Vector3.Dot(-distVec.normalized, GlobalConstants.ZeroYComponent(-lookingVector.normalized)) >= Mathf.Cos(135.0f * Mathf.Deg2Rad))
					&& dashTimer >= dashCooldownTime
					&& Physics.Raycast(transform.position + ((desired - transform.position).normalized * (myCC.SprintSpeed + 0.5f)), Vector3.down, 10.0f))
				{
                    if (DashPart != null)
                        dashEmit.enabled = true;

                    if(mySource != null && !myCC.Sprinting)
                    {
                        mySource.clip = AudioClips[0];
                        mySource.Play();
                    }

					myCC.Sprinting = true;  
					StartCoroutine(endDash());
				}

				// If the player's in our weak spot or we can't attack, move away!
				// I put hook range here because we don't want to run *too* far away, and for hook users that's a smart distance to be
				if(Vector3.Dot(-distVec.normalized, GlobalConstants.ZeroYComponent(-lookingVector.normalized)) >= Mathf.Cos(135.0f * Mathf.Deg2Rad)
					|| (myMelee.currentshootCD < myMelee.weaponData.fireCD && meleeCD < meleeCDLength))
				{
					if(Physics.Raycast(transform.position + (((playerRef.transform.position + (distVec.normalized * hookRange)) - transform.position).normalized * (myCC.Speed + 0.5f)), Vector3.down, 10.0f))
					{
						moveAI.MoveTo(playerRef.transform.position + (distVec.normalized * hookRange));	
					}
				}

				// Otherwise, try to get in an intelligent range!
				else if(Physics.Raycast(transform.position + ((desired - transform.position).normalized * (myCC.Speed + 0.5f)), Vector3.down, 10.0f))
				{
					moveAI.MoveTo(desired);
				}
			}

			if(!(isAttacking || WeaponSlot1.isFiring) && !preparingHook && myMelee.currentshootCD >= myMelee.weaponData.fireCD && meleeCD >= meleeCDLength)
			{
				// If the player's in range, go for a melee attack!
				if((transform.position - player.transform.position).sqrMagnitude <= Mathf.Pow(myMelee.range, 2) &&
					Vector3.Dot((player.transform.position - transform.position).normalized, lookingVector.normalized) >= Mathf.Cos(45.0f * Mathf.Deg2Rad))
				{
					StartCoroutine(MeleeAttack());
				}

				// Otherwise, prepare for the hook attack!
				else if(usesHook && (transform.position - player.transform.position).sqrMagnitude <= Mathf.Pow(hookRange, 2) &&
					(transform.position - player.transform.position).sqrMagnitude >= Mathf.Pow(hookDeadzone, 2) &&
					Vector3.Dot((player.transform.position - transform.position).normalized, lookingVector.normalized) >= Mathf.Cos(45.0f * Mathf.Deg2Rad) &&
					!player.Stunned)
				{
					if(rangeTimer >= hookCooldown)
					{
						StartCoroutine(HookAttack());
					}
				}
			}
		}
		else if(chasePlayer)
		{
			// Move to the last place they were seen, stay there waiting for their inevitable return
			if(Physics.Raycast(transform.position + ((LastSeen - transform.position).normalized * (myCC.Speed + 0.5f)), Vector3.down, 10.0f))
			{
				moveAI.MoveTo(LastSeen);
			}
		}

        // Update the shield to be rotated and looking at the player at almost all times
        //Shield.transform.rotation = Quaternion.Euler(0, -GlobalConstants.angleBetweenVec(lookingVector.normalized) - 90, 0);
    }

	IEnumerator MeleeAttack()
	{
		isAttacking = true;

		yield return new WaitForSeconds(meleeTelegraph);

		WeaponSlot1.FireWeapon(lookingVector);

		isAttacking = false;
		meleeCD = 0.0f;
	}

    IEnumerator endDash()
	{
		yield return new WaitForSeconds(dashLength);

        if (DashPart != null) 
            dashEmit.enabled = false;
         

        myCC.Sprinting = false; 
        dashTimer = 0.0f;
	}

	public override void UpdateAnimationController()
	{
		if(AIState == EnemyAIState.Aggro && !(isAttacking || WeaponSlot1.isFiring)) // If we're not in idle state ...
		{
			if (SeePlayer()) // If we can see the player ...
			{
				if(myMelee.currentshootCD >= myMelee.weaponData.fireCD && meleeCD >= meleeCDLength)
				{
					// We're not in cooldown, we can look the player in the eye pretty quickly
					lookingVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);
				}

				// If we're currently attacking, we're too blinded by our attack to focus on looking right at the player
				else if(!(isAttacking || WeaponSlot1.isFiring))
				{
					// We just attacked, we need a minute
					lookingVector = Vector3.RotateTowards(
						lookingVector,
						GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position),
						cooldownTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
						0.0f);
				}

				LastSeen = playerRef.transform.position;
			}
			else
			{
				// If we can't see the player then look at the last position he was at
				lookingVector = LastSeen - transform.position;
			}
		}

		if(UpdateLookingVector)
		{
			animationHandler.LookingVector = lookingVector;

			// Update the shield to be rotated and looking at the player at almost all times
			Shield.transform.rotation = Quaternion.Euler(0, -GlobalConstants.angleBetweenVec(lookingVector.normalized) - 90, 0);
		}

		animationHandler.velocity = myCC.Velocity;


		animationHandler.holdGun = AIState != EnemyAIState.Idle;
		myAnimator.SetBool("Passive", AIState == EnemyAIState.Idle);

		// For the transition from vulnerable to aggro
		Vector3 rotSave = WeaponSlot1.RotateObject.transform.localEulerAngles;
		Vector3 posSave = WeaponSlot1.transform.localPosition;

		animationHandler.gunObject1 = WeaponSlot1;
		animationHandler.gunObject2 = WeaponSlot2; 
		animationHandler.activeGunObject = currentWeapon;
		animationHandler.Update();

		// Special code for the tank's handling of the melee weapon
		if((isAttacking || WeaponSlot1.isFiring))
		{
			WeaponSlot1.RotateObject.transform.localEulerAngles = rotSave;
			WeaponSlot1.transform.localPosition = posSave;
		}
	}

	public override WeaponBase myWeapon
	{
		get
		{
			return currentWeapon;
		}
	}
		
	public override void OnMelee(int _damage)
	{
		Vector3 distVec = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

		// Check if the player is in our weak spot
		if(Vector3.Dot(distVec.normalized, GlobalConstants.ZeroYComponent(-lookingVector.normalized)) >= Mathf.Cos(90f * Mathf.Deg2Rad))
		{
			base.OnHit(_damage);

			Stun();
		}
	}
		
	public override void OnHit(int _damage)
	{
		// We don't want bullets to do anything by default when they hit us
		return;
	}

	public bool CheckAttack(Vector3 center, int _damage)
	{
		Vector3 distVec = GlobalConstants.ZeroYComponent(center - transform.position);

		// Check if the source is in our weak spot
		if(Vector3.Dot(distVec.normalized, GlobalConstants.ZeroYComponent(-lookingVector.normalized)) >= Mathf.Cos(90f * Mathf.Deg2Rad))
		{
			base.OnHit(_damage);
			return true;
		}

		return false;
	}

	IEnumerator HookAttack()
	{
		preparingHook = true;


		// WHY DOES THIS WORK?!?!?!?
		float dt = Time.deltaTime;

        myAnimator.SetTrigger("Hook");
		// Telegraph
		yield return new WaitForSeconds(0.45f);

        hook.GetComponent<SpriteRenderer>().enabled = true; 
        chain.positionCount = 2;
        chain.SetPosition(0, transform.position);
		chain.SetPosition(1, transform.position);
        chain.enabled = true;

        isAttacking = true;

		// Try and guess what angle to throw the hook at to hit the player
		// TODO: Why on earth does this work? By all means, it really shouldn't...
		Vector3 a = GlobalConstants.ZeroYComponent(player.transform.position - transform.position);
		Vector3 b = GlobalConstants.ZeroYComponent(player.cc.ProjectedPosition - transform.position);
		float angularVelocity = -Vector3.SignedAngle(a, b, Vector3.up) * 1.0f;
			
		Vector3 predictedPos = new Vector3();
		Vector3 predictedDirection = lookingVector;
		Vector3 hookPos = transform.position;

		for(int i = 0; (transform.position - hookPos).sqrMagnitude <= (player.transform.position - transform.position).sqrMagnitude; i++)
		{
			predictedPos = player.transform.position + (player.cc.Velocity * i * dt);

			predictedDirection = Quaternion.AngleAxis(angularVelocity * i, Vector3.up) * predictedDirection;

			hookPos = transform.position + (predictedDirection.normalized * hookThrowSpeed * i * dt);
		}

		lookingVector = predictedDirection;
		animationHandler.LookingVector = lookingVector;
		WeaponSlot1.RotateObject.transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(lookingVector.normalized));

		// Set the hook up
		hookTurn = hookTurnDefault;
		hook.transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(lookingVector));

		// Throw the hook!
		yield return new WaitUntil(ThrowHook);

		chain.positionCount = 2;

		// Draw in the hook, potentially with the player in tow
		yield return new WaitUntil(DrawHook);

		hook.GetComponent<SpriteRenderer>().enabled = false;
		chain.enabled = false;

		if(hookCooldown == 0)
		{
			lookingVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);
		}

		preparingHook = false;
		isAttacking = false;
		rangeTimer = 0.0f;

        myAnimator.ResetTrigger("Hook");
    }
 

	bool ThrowHook()
	{
		myCC.HaltMomentum();

		Vector3 distVec = playerRef.transform.position - transform.position;

		// Uncomment for homing hook
		/*if (distVec.magnitude <= hookRange)
		{
			// Check if the player is in our field of view
			if(Vector3.Dot(distVec.normalized, lookingVector.normalized) >= 0.75f)
			{
				// Check if our view of the player isn't blocked by something
				Ray r = new Ray(transform.position, distVec.normalized);
				if(!Physics.Raycast(r, distVec.magnitude, LayerMask.GetMask("Ground")))
				{
					hook.transform.right = Vector3.RotateTowards(
						hook.transform.right,
						GlobalConstants.ZeroYComponent(player.cc.ProjectedPosition - hook.transform.position),
						hookTurn * Mathf.Deg2Rad * Time.deltaTime,
						0.0f
					);

					hook.transform.eulerAngles = new Vector3(90.0f, hook.transform.eulerAngles.y, 0.0f);

					// Nerf the homing ability more the longer the hook is out
					hookTurn *= 0.75f;
				}
			}
		}*/

		// Firstly, raycast to see if we're about to hit a wall
		RaycastHit myHit;
		if(Physics.Raycast(new Ray(hook.transform.position, hook.transform.right), out myHit, hookThrowSpeed * Time.deltaTime, ~LayerMask.NameToLayer("Ground")))
		{
			// If we're here, our hook hit the ground.
			return true;
		}
			
		hook.transform.localPosition += hook.transform.right * hookThrowSpeed * Time.deltaTime;
		chain.positionCount++;
		chain.SetPosition(chain.positionCount - 1, hook.transform.position);
		chain.SetPosition(0, transform.position);

		if(hook.transform.localPosition.sqrMagnitude >=  Mathf.Pow(hookRange, 2))
		{
			return true;
		}

		// Uncomment if you want the hook to phase through the player if they're in the hook deadzone (a very edge case)
		/*if(hook.transform.localPosition.sqrMagnitude <= Mathf.Pow(hookDeadzone, 2))
		{
			return false;
		}*/

		BoxCollider hookBox = hook.GetComponent<BoxCollider>();
		Collider[] c = Physics.OverlapBox(hookBox.transform.position, hookBox.bounds.extents, hookBox.transform.rotation);

		for(int i = 0; i < c.Length; i++)
		{
			if(c[i].gameObject == player.gameObject)
			{
				player.AcceptInput = false;
				player.OnMelee(10);

				player.Stun(1.5f, false);
				undoPlayerStun = player.Unstun(1.5f);
				StartCoroutine(undoPlayerStun);

				playerHooked = true;
			}
		}

		return playerHooked;
	}

	bool DrawHook()
	{
		if(Vector3.Dot(hook.transform.position - transform.position, lookingVector) < Mathf.Cos(90.0f * Mathf.Deg2Rad)
			|| (hook.transform.position - transform.position).sqrMagnitude <= 1.0f)
		{
			hook.transform.position = transform.position;
			playerHooked = false;
			return true;
		}

		hook.transform.position -= ((hook.transform.position - transform.position).normalized * hookDrawSpeed * Time.deltaTime);
		chain.SetPosition(1, hook.transform.position);
		hook.transform.right = hook.transform.position - transform.position;
		hook.transform.eulerAngles = new Vector3(90.0f, hook.transform.eulerAngles.y, 0.0f);

		if(playerHooked && (hook.transform.position - transform.position).sqrMagnitude >= Mathf.Pow(myMelee.range  / 4.0f, 2))
		{
			// GET OVER HERE!!!
			player.transform.position = hook.transform.position;
			player.cc.HaltMomentum();
		}

		return false;
	}

	void OnTriggerEnter(Collider other)
	{
		BulletBase myBullet = other.GetComponent<BulletBase>();

		if(myBullet != null)
		{
			// Check to see if bullet is in weak spot
			Vector3 myBack = -lookingVector.normalized;
			myBack.x *= (8.0f/6.0f);
			myBack.z *= 2.5f;

			if(!CheckAttack((myBullet.transform.position - (myBullet.Direction * myBullet.Speed * Time.deltaTime)) - myBack, myBullet.myInfo.bulletDamage))
			{
				// If it isn't, neutralize it!
				base.OnHit(0);

				// TODO: Is there a better way to stop the bullet from killing itself?
				if(bulletHandling == BulletHandling.Reflect)
				{
					StartCoroutine(Reflect(myBullet));
				}

				else if(bulletHandling == BulletHandling.Absorb)
				{
					MyUnit.CurrentHealth += myBullet.myInfo.bulletDamage;
					if(MyUnit.CurrentHealth > MyUnit.MaxHealth)
					{
						MyUnit.CurrentHealth = MyUnit.MaxHealth;
					}
				}
			}
		}
	}

	IEnumerator Reflect(BulletBase myBullet)
	{
		yield return new WaitForEndOfFrame();

		myBullet.Fired = true;
		myBullet.Direction = GlobalConstants.ZeroYComponent(player.transform.position - transform.position);
		myBullet.myOwner = GetComponent<IArmed>();
		myBullet.GetComponent<SpriteRenderer>().enabled = true;
	}

	public void Stun()
	{
		moveAI.ActionComplete = true;
		AIState = EnemyAIState.Stunned;
		if(MyUnit.CurrentHealth > 0)
		{
			timer = 0.0f;
			myAnimator.SetBool("Stunned", true);
			if (myEmojis != null)
				myEmojis.Fire(EffectSystem.EffectType.Stunned, transform.position + (Vector3.up * 6) + (Vector3.back * 0.5f));

			GameObject rotateMe = WeaponSlot1.RotateObject;

			Vector3 pos = animationHandler.HeldPosition;
			pos.z = (animationHandler.faceFront) ? -.1f : .1f;

			rotateMe.transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(lookingVector.normalized));

			WeaponSlot1.transform.localPosition = pos;
		}
	}

	void OnDrawGizmosSelected()
	{
        if (!DEBUG)
            return;

		Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
		Gizmos.DrawSphere(transform.position, AggroRange);

		Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		Gizmos.DrawSphere(transform.position, myMelee.range);

		Gizmos.color = new Color(1.0f, 0.5f, 0.0f);
		Gizmos.DrawLine(transform.position + (-transform.forward * hookDeadzone), transform.position + (-transform.forward * hookRange));
	}
}
