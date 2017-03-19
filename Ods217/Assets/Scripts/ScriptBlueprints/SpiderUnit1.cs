using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This Script is for units that are like the player. They have weapons and will try to use them against you or on allies.
/// This is also a BluePrint, meaning that they should be duplicated then modified to save you time
/// </summary>
[RequireComponent(typeof(CController))]
public class SpiderUnit1 : MonoBehaviour,IUnit {

    public UnitStruct myUnit;
    public Weapon myWeapon;
    GameObject GunObj;
    CController myCtrl;

    public Vector3 LookingVector;

    enum AISTATE { Wander, Chase };
    [SerializeField]
    AISTATE AiState;

    bool wanderMoving;
    bool wanderCoroutine; 
    public bool chaseJumping; // Is the spider jumping at a target right now
    bool chaseCoroutine; // The bool coroutine flag for chasing
    Vector3 wanderRandomDirection; // Where the spider will move when wandering
    Vector3 wanderRootPosition; // The point that the spider will rotate around
    Vector3 chaseJumpVector; // The direction the spider will jump 
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
        if(wanderMoving)  // If we want to move
            myCtrl.ApplyForce(wanderRandomDirection * myCtrl.Speed); // Then move

        // Toggle back and forth between moving and not moving
        if (!wanderCoroutine)
        {
            wanderCoroutine = true;
            StartCoroutine(WanderWaiting());
        }

        // Check to see if a target is near you and if so attack them
        chaseTaggedAlliedObjects = Physics.OverlapSphere(transform.position, 5);
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
        // If we're not jumping check to see if the coroutine is running
        if (!chaseCoroutine)
        {
            chaseCoroutine = true;
            StopCoroutine(ChaseJump());
            StartCoroutine(ChaseJump());
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

    IEnumerator ChaseJump()
    {
        yield return new WaitForSeconds(1);
        chaseCoroutine = false;
        chaseJumping = !chaseJumping;

        // Jump
        if (chaseJumping)
        {
            // I really don't like hard coding this but hey it's a techdemo. Or, if this is in the live version: shut up. You saw nothing.
            chaseJumpVector = GlobalConstants.ZeroYComponent(chaseTarget.transform.position) - GlobalConstants.ZeroYComponent(transform.position);
            chaseJumpVector *= toPlayerVector;
            chaseJumpVector += Vector3.up * upVector;
            myCtrl.ApplyForce(chaseJumpVector);
        }
        else
            chaseObjectHit.Clear();
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
}
