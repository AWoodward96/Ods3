using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unit Script
/// The primary script for the first type of enemy in the game:
/// The Jumping Spider
/// </summary>
[RequireComponent(typeof(CController))]
public class JumpingSpider : MonoBehaviour, IArmed
{
    public JumpingSpiderAI AiStats;

	public UnitStruct myUnit;
    ZoneScript zone;
    Animator myAnimator;
    CController myCtrl;
	EffectSystem myEmojis;

    Light myLight;
    Color redLight = new Color(209f / 255f, 0, 0, 1);
    Color greenLight = new Color(6f / 255f, 209f / 255f, 0, 1);

    // Do we want this enemy to automatically aggro on the player
    public bool aggressive;
    public bool hiveMinded;

	// Is the spider powered?
	public bool powered = false;

	bool hasAttacked = false;

    enum AISTATE { Idle, Aggro, Shimmy, Prep, Jump, Dead };
    [SerializeField]
    AISTATE AIState;

	float stateTimer = 0.0f;

    public List<AudioClip> PrimaryClips; 
    AudioSource myPrimarySource; 
    float primaryCD; 

    // Use this for initialization
    void Start()
    {
        myCtrl = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myLight = GetComponentInChildren<Light>();
		myEmojis = GetComponentInChildren<EffectSystem>();
        myPrimarySource = gameObject.GetComponent<AudioSource>();

		// Do all of this in the editor!
        //myPrimarySource.loop = false;
        //myPrimarySource.playOnAwake = false;
        //myPrimarySource.spatialBlend = 1f;
        //myPrimarySource.dopplerLevel = .2f;
        //myPrimarySource.minDistance = 1;
        //myPrimarySource.rolloffMode = AudioRolloffMode.Linear;
        //myPrimarySource.maxDistance = 25;
        //myPrimarySource.volume = 1f;

        AIState = AISTATE.Idle;
		myLight.gameObject.SetActive(false);
         
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if(myZone != ZoneScript.ActiveZone || !powered)
		{
			return;
		}

		stateTimer += Time.deltaTime;

        handleSound();
        updateVisuals();

        // Do something different for each AI
        switch (AIState)
        {
            case AISTATE.Idle: // Do nothing I guess? If at any point you can actually see the player aggro
			{
                AIIdle();
                break;
			}

			case AISTATE.Aggro:
			{
				if(stateTimer >= AiStats.ReactionSpeed)
				{
					AIState = AISTATE.Shimmy;
					stateTimer = 0;
				}
				break;
			}

            case AISTATE.Shimmy: // Ideally, move towards the player, and shimmy back and forth its a grand ol time
			{
				if(stateTimer > 1)
				{
					float rnd = UnityEngine.Random.Range(0f, 1f);
					if(rnd >= AiStats.PrepThreshold)
					{
						AIState = AISTATE.Prep;
						playSound(1);
					}
					stateTimer = 0;
				}
				else
				{
                	AIShimmy();
				}
                break;
			}

			case AISTATE.Prep:
			{
				if(stateTimer > AiStats.PrepSpeed)
				{
					AIState = AISTATE.Jump;
					Jump();
					stateTimer = 0;
				}
				break;
			}

            case AISTATE.Jump: // Go Go Go!
			{
				if(stateTimer > 1)
				{
					AIState = AISTATE.Shimmy;
					hasAttacked = false;
					stateTimer = 0;
				}
                break;
			}

			case AISTATE.Dead:
			{
				if(stateTimer > 2)
				{
					Instantiate(Resources.Load("Prefabs/Particles/SimpleDeath"), transform.position, Quaternion.identity);
					myAudioSystem.PlayAudioOneShot(PrimaryClips[5], transform.position);
					this.gameObject.SetActive(false);
				}
				break;
			}
        }
    }

    void updateVisuals()
    {
        // Update the anim stats
        bool moving = GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f;
        myAnimator.SetBool("Moving", moving);
		myAnimator.SetBool("Jumping", AIState == AISTATE.Jump && GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 2f);
        myAnimator.SetBool("Prep", (AIState == AISTATE.Prep));
        if (moving)
            myAnimator.SetBool("Front", myCtrl.Velocity.z < 1);

        // update the light
        float yValue = (AIState == AISTATE.Shimmy) ? -.7f : -.4f;
        yValue = (AIState == AISTATE.Prep) ? -.85f : yValue;

        float zValue = myAnimator.GetBool("Front") ? -0.1f : 0.1f;
        zValue = (AIState == AISTATE.Idle) ? -0.1f : zValue;
        myLight.color = (AIState == AISTATE.Idle || AIState == AISTATE.Aggro) ? greenLight : redLight;

        if (AIState == AISTATE.Dead)
            myLight.transform.localPosition = new Vector3(.5f, -.8f, -.1f);
        else
            myLight.transform.localPosition = new Vector3(0, yValue, zValue);
    }

    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

    void AIIdle()
    {
		if (ZoneScript.ActiveZone == zone && aggressive) // You can't aggro if the player isn't even in the zone
        {
            Collider[] potentialTargets = Physics.OverlapSphere(transform.position, 12);
            foreach (Collider c in potentialTargets)
            {
                if (c.gameObject.layer == LayerMask.NameToLayer("Units")) // Any units are targets for spiders
                {
                    if (c.name.ToUpper().Contains("SPIDER"))
                        continue;

                    // You also have to have vision of the target because if you don't then... well we don't want them to aggro through walls  
                    Vector3 myZeroPosition = GlobalConstants.ZeroYComponent(transform.position);
                    Vector3 targetZeroPosition = GlobalConstants.ZeroYComponent(c.transform.position);
                    // Raycast to see if we can jump at the target
                    Vector3 distVector = c.transform.position - transform.position;
                    Ray r = new Ray(transform.position, distVector);
                    if (!Physics.Raycast(r, distVector.magnitude, LayerMask.GetMask("Ground"))) // If we didn't hit a wall on our raycast
                    {
                        AiStats.Target = c.gameObject;
                        AIState = AISTATE.Aggro;
						stateTimer = 0;
						if (myEmojis != null)
						{
							myEmojis.Fire(EffectSystem.EffectType.Alert, transform.position + (Vector3.up * 4));
						}
						playSound(2);
                    }
                    return;
                }
            }
        }
    }

    void AIShimmy()
    {
        // move towards the player  (slowly)
        // Also move left and right based on the shimmy shimmy
        Vector3 TargetPos = AiStats.Target.transform.position;
        Vector3 distToPlayer = TargetPos - transform.position;
        Vector3 y0_unitDistToPlayer = GlobalConstants.ZeroYComponent(distToPlayer).normalized;

        myCtrl.ApplyForce(y0_unitDistToPlayer.normalized * myCtrl.Speed / 3);

        // apply the shimmy vector force to move the spider back and forth
        Vector3 ShimmyVector = new Vector3(-y0_unitDistToPlayer.z, 0, y0_unitDistToPlayer.x);
        ShimmyVector = ShimmyVector.normalized;
        AiStats.shimmyValue = Mathf.Sin(Time.time) / 2;
        myCtrl.ApplyForce(ShimmyVector * AiStats.shimmyValue);
    }

	// NOTE: The spiders need to jump no higher than 4 units, or they can get on the ceilings!
    void Jump()
    {

        // play the sound effect 
        playSound(4);

        //PHYSICS PHYSICS PHYSICS PHYSICS
        float currentGravity = GlobalConstants.Gravity;
        float angle = 70 * Mathf.Deg2Rad; // To radian because Unity's sin and cos functions randomly take radians. like why?


        Vector3 targetPos = AiStats.Target.transform.position;
        //float angle = Vector3.Angle(GlobalConstants.ZeroYComponent(targetPos - transform.position), targetPos - transform.position)  * Mathf.Deg2Rad; 
        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);
        if ((planarTarget - planarPosition).magnitude < 3)
            planarTarget += (planarTarget - planarPosition).normalized; // We want to move past the player

        float distance = Mathf.Max(Vector3.Distance(planarTarget, planarPosition), 1);
        float yOffset = transform.position.y - targetPos.y;

        //PHYSICS PHYSICS PHYSICS PHYSICS
        float initialVel = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * currentGravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
        Vector3 vel = new Vector3(0, initialVel * Mathf.Sin(angle), initialVel * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (targetPos.x < transform.position.x ? -1 : 1);
        Vector3 finalVel = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * vel;

		finalVel = new Vector3(finalVel.x, Mathf.Min(8.0f, finalVel.y), finalVel.z);

        myCtrl.ApplyForce(finalVel * 7);
        // Ok so this finally apply force isn't physics, it's making my CC script work
        // If I don't multiply the finalVel, the spider will just make a pitaful hop towards the player and barely even get off the ground
        // As long as we cap the spiders max velocity at a high enough number, this should work just fine for what we want to do

    }


    public void OnDeath()
    {
        AIState = AISTATE.Dead;
		stateTimer = 0;
        myCtrl.enabled = false;

		if(powered)
		{
	        playSound(3);  
	        myAnimator.SetBool("Death", true);
		}
    }

	// INTERFACE IMPLEMENTATION //

	public void OnHit(int _damage)
    {
		if(!powered)
			return;

        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        // Hello future me stop reading these comments and get back to work (4/24/17)
        if (myUnit.CurrentHealth > 0)
        {
            myVisualizer.ShowMenu();
            myUnit.CurrentHealth -= _damage;
            if (myUnit.CurrentHealth <= 0)
            {
				stateTimer = 0;
                OnDeath();
            }

			if(!powered)
				return;
			
			GameObject Target = GameObject.FindWithTag("Player");
            if (AIState == AISTATE.Idle)
            {
                // Raycast to the player and see if you can see him if so aggro on him 
                Vector3 distVector = Target.transform.position - transform.position;
                Ray r = new Ray(transform.position, distVector);
                if (!Physics.Raycast(r, distVector.magnitude, LayerMask.GetMask("Ground"))) // If we didn't hit a wall on our raycast
                {
                    // Well lets attack him!
                    AiStats.Target = Target;
                    AIState = AISTATE.Aggro;
					stateTimer = 0;

					if (myEmojis != null)
					{
						myEmojis.Fire(EffectSystem.EffectType.Alert, transform.position + (Vector3.up * 4));
					}
					playSound(2);
                }
            }

            
            if (hiveMinded)
            {
                // Aggro the rest of the hivemind spiders in the zone 
				foreach (IPermanent enemy in ZoneScript.ActiveZone.Perms)
                {
					JumpingSpider thisscript = enemy.gameObject.GetComponent<JumpingSpider>();
					if (thisscript != null && thisscript != this && thisscript.AIState == AISTATE.Idle && thisscript.hiveMinded)
                    {
                        thisscript.AiStats.Target = Target;
                        thisscript.AIState = AISTATE.Aggro;
						thisscript.stateTimer = 0;
						if (thisscript.myEmojis != null)
						{
							thisscript.myEmojis.Fire(EffectSystem.EffectType.Alert, enemy.gameObject.transform.position + (Vector3.up * 4));
						}
                    }
                }
            }


        }
    }

    public UnitStruct MyUnit
    {
        get { return myUnit; }
		set { myUnit = value; }
    }

	public WeaponBase myWeapon
	{
		get { return null; }
		set { return; }
	}

	public ZoneScript myZone
	{
		get { return zone; }
		set { zone = value; }
	}

	public bool Triggered
	{
		get { return powered; }

		set
		{
			powered = value;
			myLight.gameObject.SetActive(value);
		}
	}

	public void Activate()
	{
		
	}

	// END INTERFACE IMPLEMENTATION //

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (AIState == AISTATE.Jump && myUnit.CurrentHealth > 0) // If we're jumping and alive
        {
			if (!hasAttacked && hit.gameObject.GetComponent<IArmed>() != null) // If the list of things we've hit during this jump doesn't include what we just hit and it has an IUnit
            {
                // Well check to make sure it aint a spider
                if (!hit.gameObject.name.Contains("Spider")) // Rudimentary but it works
                {
					IArmed u = hit.gameObject.GetComponent<IArmed>();
					u.OnHit(AiStats.damageValue);
					hasAttacked = true;
                }
            }
        }
    } 
    
    // Handles sound on a frame to frame basis
    void handleSound()
    {

        primaryCD += Time.deltaTime; 

        if (AIState == AISTATE.Shimmy && primaryCD > .1f)
        {
            primaryCD = 0;
            myPrimarySource.Stop();
            myPrimarySource.clip = PrimaryClips[0];
            myPrimarySource.Play(); 
        }


    }

    // Plays a one shot sound when certain AI events are triggered
    void playSound(int clipIndex)
    {
        myPrimarySource.Stop();
        myPrimarySource.pitch = 1 + (UnityEngine.Random.Range(0, .2f) - .1f);
        myPrimarySource.clip = PrimaryClips[clipIndex];
        myPrimarySource.Play();
    }
}

[Serializable]
public struct JumpingSpiderAI
{

    [Range(.1f, 2)]
    public float ReactionSpeed;
    [Range(.1f, 2)]
    public float PrepSpeed;
    [Range(.1f, 1f)]
    public float PrepThreshold;

    public GameObject Target;

    //[HideInInspector] 
    public float shimmyValue;
	public int damageValue;
}
