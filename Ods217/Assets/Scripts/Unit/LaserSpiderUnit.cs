using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is a Unit Script
/// This is the script for the second type of enemy you'll encounter - a Laser Spider
/// </summary>
[RequireComponent(typeof(CController))]
[RequireComponent(typeof(NavMeshAgent))]
public class LaserSpiderUnit : MonoBehaviour, IUnit
{

    [Header("Unit Data")]
    public UnitStruct myUnit;
    public Weapon myWeapon;
    GameObject GunObj;
    CController myCtrl;
    NavMeshAgent myAgent;
    public GameObject SpiderTop;

    [Header("Spider Data")]
    Vector3 LookingVector;
    public float LookVectorRotation;
    float currentRotation;

    enum AISTATE { Wander, Attack };
    [SerializeField]
    AISTATE AiState;

    bool wanderMoving;
    bool wanderCoroutine;
    Vector3 wanderRandomDirection; // Where the spider will move when wandering
    Vector3 wanderRootPosition; // The point that the spider will rotate around
    GameObject attackTarget;
    float attackRange = 14;
    Animator myAnimator;
    Animator topAnimator;





    // Use this for initialization
    void Awake()
    {

        // Get the gun object in the child of this object
        Transform[] objs = GetComponentsInChildren<Transform>();
        foreach (Transform o in objs)
        {
            if (o.name == "GunChild")
                GunObj = o.gameObject;
        }

        myWeapon.Owner = this;

        myCtrl = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        topAnimator = SpiderTop.GetComponent<Animator>();
        myAgent = GetComponent<NavMeshAgent>();

        wanderRootPosition = transform.position;
        wanderRootPosition.y = 0;

        LookingVector = Vector2.right;

    }

    // Update is called once per frame
    void Update()
    {
        if (myUnit.CurrentHealth > 0)
        {
            topAnimator.SetFloat("lookX", LookingVector.x);
            topAnimator.SetFloat("lookY", LookingVector.y);

            switch (AiState)
            {
                case AISTATE.Wander:
                    Wander();
                    myAnimator.SetBool("Moving", GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f);
                    break;
                case AISTATE.Attack:
                    Attack();
                    myAnimator.SetBool("Moving", GlobalConstants.ZeroYComponent(myAgent.velocity).magnitude > 1f);
                    break;

            }

            doTopAnimations();
        }

    }

    /// <summary>
    /// Alright so here we take the looking vector, and rotate it the amount of degrees it needs to be rotated
    /// However, we don't want the looking vector to snap, we want to rotate towards where we want to look mk?
    /// </summary>
    void doTopAnimations()
    {
        // So lets rotate the looking vector by the current rotation
        //LookingVector = Quaternion.AngleAxis(currentRotation, Vector3.up) * LookingVector;
        LookingVector = Quaternion.Euler(0, 0, currentRotation) * Vector3.right;
        // And lerp the current rotation
        currentRotation = Mathf.Lerp(currentRotation, LookVectorRotation, 5 * Time.deltaTime);

        switch (AiState)
        {
            case AISTATE.Wander:
                LookVectorRotation = GlobalConstants.angleBetweenVec(wanderRandomDirection);
                break;

            case AISTATE.Attack:
                LookVectorRotation = GlobalConstants.angleBetweenVec(attackTarget.transform.position - transform.position);
                break;
        }

    }

    /// <summary>
    /// Not your usual wandering script. Move this unit around a set point in a random direction. Follow this pattern Move - Wait - Move - Wait etc
    /// Do not get too far away from said point, if you do move back towards it
    /// </summary>
    void Wander()
    {
        if (wanderMoving)  // If we want to move
            myCtrl.ApplyForce(wanderRandomDirection * myAgent.speed / 10); // Then move

        // Toggle back and forth between moving and not moving
        if (!wanderCoroutine)
        {
            wanderCoroutine = true;
            StartCoroutine(WanderWaiting());
        }

        // Check to see if a target is near you and if so attack them
        Collider[] chaseTaggedAlliedObjects = Physics.OverlapSphere(transform.position, 5);
        foreach (Collider c in chaseTaggedAlliedObjects)
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("Units")) // Any units are targets for spiders
            {
                // You also have to have vision of the target because if you don't then... well we don't want them to aggro through walls

                attackTarget = c.gameObject;
                AiState = AISTATE.Attack;
                return;
            }
        }
    }

    /// <summary>
    /// Ok first, check to see if you're in range
    /// If you're not in range, move till you're within range.
    /// If in range shoot dem boiz
    /// </summary>
    void Attack()
    {
        if (attackTarget != null)
        {
            Vector3 distVector = attackTarget.transform.position - transform.position;
            distVector = GlobalConstants.ZeroYComponent(distVector);

            // If in range
            if (distVector.magnitude < attackRange)
            {
                // Shootem 
                myAgent.Stop();

                myWeapon.FireWeapon(distVector);
            }
            else
            {
                // otherwise move towards the target 
                myAgent.Resume();
                myAgent.destination = attackTarget.transform.position;
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
        if (!wanderMoving) // If we're not moving, then we set this up. This will make the spider look before it moves
        {
            Vector2 v = UnityEngine.Random.insideUnitCircle.normalized;
            wanderRandomDirection = new Vector3(v.x, 0, v.y);

            if (Vector3.Distance(GlobalConstants.ZeroYComponent(transform.position), wanderRootPosition) > 10)
                wanderRandomDirection = (wanderRootPosition - GlobalConstants.ZeroYComponent(transform.position)).normalized;
        }


        // Lets us do this
        wanderCoroutine = false;
    }

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    public Weapon MyWeapon
    {
        get { return myWeapon; }
    }


    public void OnDeath()
    {
        StopCoroutine(DeathCoroutine());
        StartCoroutine(DeathCoroutine());
        myCtrl.enabled = false;
        Instantiate(Resources.Load("Prefabs/Particles/SmallerDeath"), transform.position + Vector3.up, Quaternion.identity);
        SpiderTop.SetActive(false);
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
        if (myUnit.CurrentHealth > 0)
        {
            myVisualizer.ShowMenu();
            myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
            if (myUnit.CurrentHealth <= 0)
            {
                OnDeath();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawIcon(wanderRootPosition, "SpiderRoot");
        if (AiState == AISTATE.Attack)
        {
            Vector3 distVector = attackTarget.transform.position - transform.position;
            distVector = GlobalConstants.ZeroYComponent(distVector);

            if (distVector.magnitude < attackRange)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, attackTarget.transform.position);
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
