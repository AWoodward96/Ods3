using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This Script is for units that are like the player. They have weapons and will try to use them against you or on allies.
/// This is also a BluePrint, meaning that they should be duplicated then modified to save you time
/// </summary>
[RequireComponent(typeof(CController))]
[RequireComponent(typeof(NavMeshAgent))]
public class SpiderUnit1 : MonoBehaviour,IUnit {

    public UnitStruct myUnit;
    public Weapon myWeapon;
    GameObject GunObj;
    CController myCtrl;
    NavMeshAgent myAgent;


    enum AISTATE { Wander, Chase };
    [SerializeField]
    AISTATE AiState;

    public bool WillWander;
    bool wanderMoving;
    bool wanderCoroutine; 
    public bool chaseJumping; // Is the spider jumping at a target right now
    bool chaseCoroutine; // The bool coroutine flag for chasing
    Vector3 wanderRandomDirection; // Where the spider will move when wandering
    Vector3 wanderRootPosition; // The point that the spider will rotate around 
    Collider[] chaseTaggedAlliedObjects; // Not so important
    List<GameObject> chaseObjectHit = new List<GameObject>();
    GameObject chaseTarget; // The gameobject we'll be juping at

    Animator myAnimator;

   
    [Range(0, 100)]
    float toPlayerVector = 5;
    [Range(0, 100)]
    float upVector = 50;
    

    // Use this for initialization
    public void Awake() {

        // Get the gun object in the child of this object
        Transform[] objs = GetComponentsInChildren<Transform>();
        foreach (Transform o in objs)
        {
            if (o.name == "GunChild")
                GunObj = o.gameObject;
        }

        myCtrl = GetComponent<CController>();
        myAgent = GetComponent<NavMeshAgent>();
        myAnimator = GetComponent<Animator>();

        wanderRootPosition = transform.position;
        wanderRootPosition.y = 0;

        if(myWeapon == null)
        {
            myWeapon.GetComponent<Weapon>();
        }

    }
	
	// Update is called once per frame
	void Update () {
        myAnimator.SetBool("Moving", GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f);
        myAnimator.SetBool("Jumping", chaseJumping && GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 2f);

        if(myUnit.CurrentHealth > 0)
        {
            switch (AiState)
            {
                case AISTATE.Wander:
                    Wander();
                    break;
                case AISTATE.Chase:
                    Chase();
                    break;
            }
        } 
        
	}


    /// <summary>
    /// Not your usual wandering script. Move this unit around a set point in a random direction. Follow this pattern Move - Wait - Move - Wait etc
    /// Do not get too far away from said point, if you do move back towards it
    /// </summary>
    void Wander()
    {
        if (WillWander) // if we want to move around a bit
        {
            if (wanderMoving)  // Check the toggling bool to see if it's time to move
                myCtrl.ApplyForce(wanderRandomDirection * myCtrl.Speed); // Then move

            // Toggle back and forth between moving and not moving
            if (!wanderCoroutine)
            {
                wanderCoroutine = true;
                StartCoroutine(WanderWaiting());
            } 
        }


        // Check to see if a target is near you and if so attack them
        chaseTaggedAlliedObjects = Physics.OverlapSphere(transform.position, 12);
        foreach(Collider c in chaseTaggedAlliedObjects)
        { 
            if(c.gameObject.layer == LayerMask.NameToLayer("Units")) // Any units are targets for spiders
            { 
                // You also have to have vision of the target because if you don't then... well we don't want them to aggro through walls  
                chaseTarget = c.gameObject; 
                AiState = AISTATE.Chase; 
                return;
            }
        }
    }

    void Chase()
    {
        myAgent.enabled = !chaseJumping; // Ok see, the NavMesh agent doesn't let anything off the ground. Which is a problem. So if we're jumping, we need it to be disabled, but if we want to chase down the player it needs to be enabled. This works.
        // Raycast to the player
        if(!chaseJumping) // If we're not in the process of jumping
        {
            Vector3 myZeroPosition = GlobalConstants.ZeroYComponent(transform.position);
            Vector3 targetZeroPosition = GlobalConstants.ZeroYComponent(chaseTarget.transform.position);
            if (Vector3.Distance(myZeroPosition, targetZeroPosition) < 10) // If we're close enough
            {
                // Raycast to see if we can jump at the target
                Vector3 distVector = chaseTarget.transform.position - transform.position;
                Ray r = new Ray(transform.position, distVector);
                if (!Physics.Raycast(r, distVector.magnitude, LayerMask.GetMask("Ground"))) // If we didn't hit a wall on our raycast
                {
                    // Jump at the player!
                    Jump();
                    chaseJumping = true;
                }
            }else
            {
                // if we're not close enough chace him down!
                myAgent.SetDestination(chaseTarget.transform.position);
                myCtrl.ApplyForce(myAgent.desiredVelocity.normalized * myCtrl.Speed); 
            }

        }else
        {
            // If there is very little x and z movement currently
            if (GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude < .2f)
            {
                chaseJumping = false;
                chaseObjectHit.Clear();
            }
        }
        
      
    }

    IEnumerator WanderWaiting()
    {
        // wait for a random amount of time
        float max = (wanderMoving) ? 1 : 3;
        float rnd = UnityEngine.Random.Range(.5f, max);
        yield return new WaitForSeconds(rnd);
        wanderMoving = !wanderMoving;

        // Get a random direction
        // also convert that v2 to a v3
        Vector2 v = UnityEngine.Random.insideUnitCircle.normalized;
        wanderRandomDirection = new Vector3(v.x, 0, v.y);

        if (Vector3.Distance(GlobalConstants.ZeroYComponent(transform.position), wanderRootPosition) > 10)
            wanderRandomDirection = (wanderRootPosition - GlobalConstants.ZeroYComponent(transform.position)).normalized;
 

        // Lets us do this
        wanderCoroutine = false;
    }

    void Jump()
    {
        //PHYSICS PHYSICS PHYSICS PHYSICS
        float currentGravity = GlobalConstants.Gravity;
        float angle = 50 * Mathf.Deg2Rad; // To radian because Unity's sin and cos functions randomly take radians. like why?

        Vector3 planarTarget = new Vector3(chaseTarget.transform.position.x, 0, chaseTarget.transform.position.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);
        planarTarget += (planarTarget - planarPosition).normalized; // We want to move past the player

        float distance = Mathf.Max(Vector3.Distance(planarTarget, planarPosition), 1);
        float yOffset = transform.position.y - chaseTarget.transform.position.y;

        //PHYSICS PHYSICS PHYSICS PHYSICS
        float initialVel = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * currentGravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
        Vector3 vel = new Vector3(0, initialVel * Mathf.Sin(angle), initialVel * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (chaseTarget.transform.position.x < transform.position.x ? -1 : 1);
        Vector3 finalVel = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * vel;
 
        myCtrl.ApplyForce(finalVel * distance); 
        // Ok so this finally apply force isn't physics, it's making my CC script work
        // If I don't multiply by the distance, the spider will just make a pitaful hop towards the player and barely even get off the ground
        // As long as we cap the spiders max velocity at a high enough number, this should work just fine for what we want to do
  
    }

    public UnitStruct MyUnit()
    {
        return myUnit;
    }

    public Weapon MyWeapon()
    {
        return myWeapon;
    }

    public void OnDeath()
    {
        StopCoroutine(DeathCoroutine());
        StartCoroutine(DeathCoroutine());
        myCtrl.enabled = false;
        myAnimator.SetBool("Death", true);
    }

    IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2);
        Instantiate(Resources.Load("Prefabs/Particles/SimpleDeath"), transform.position, Quaternion.identity); 
        this.gameObject.SetActive(false);
    }

    public void OnHit(Weapon _FromWhatWeapon)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        if(myUnit.CurrentHealth > 0)
        {
            myVisualizer.ShowMenu();
            myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
            if(myUnit.CurrentHealth <= 0)
            {
                OnDeath();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawIcon(wanderRootPosition, "SpiderRoot");
    }
  
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(chaseJumping && myUnit.CurrentHealth > 0)
        {
            if(!chaseObjectHit.Contains(hit.gameObject) && hit.gameObject.GetComponent<IUnit>() != null)
            {
                IUnit u = hit.gameObject.GetComponent<IUnit>();
                chaseObjectHit.Add(hit.gameObject); 
                u.OnHit(myWeapon);
            }
        }
    }

    public HealthVisualizer myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthVisualizer>();
        }
    }

}
