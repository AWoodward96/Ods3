using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBroT2 : MonoBehaviour, IArmed{


    [Header("Spider Data")]

    public UnitStruct UnitData = new UnitStruct("Spider Bro V2",40,40,25,25);

    public enum SpiderBroT2State { Idle, Patrolling, Working, Aggro, Yell }
    public SpiderBroT2State State;

    public int Damage = 5;
    public int PatrolNum = 0;
    public int CurrentPatrolIndex = -1;
    public bool WillWait = true;
    public int WillWaitCount = 3;
    int prevPatrolIndex = -1;
    int patrolDirection = 1;

    public bool attacking = false;
    public float PeekWeight = 0;
    float AITimeout = 0;
    float curWaitCount = 1;

    public WeaponBase MyWeapon;


    Vector3 desiredPoint;
    Vector3 moveVec;
    Vector3 lastSeenPlayer;

    GameObject Player;
    CController myCC;
    ZoneScript z;
    EnergyManager myManager;

    Animator ChasisAnim;
    Animator HeadAnim;
    AudioSource ChargeLaserSource;


    [Header("Spider Senses")]
    public SpiderSense mySpiderSense;  
      
    public bool DEBUG = false;

    const float PATROLSPEED = .3F; 
    const float WAITTIMEOUT = 2;
    const int RALLYRANGE = 15;
    const float AGGRORANGE = 20;
    const float DESIREDDISTANCE = 8;

    public AudioClip WeaponChargeClip;
     
    ParticleSystem.EmissionModule SuperPowerupSpherePartEmission;

    bool FireAtWill = false;

    // Use this for initialization
    void Start () {
        myCC = GetComponent<CController>();
        if (mySpiderSense == null)
            Debug.Log("Spider Sense not set on: " + this.gameObject);

        myManager = GetComponent<EnergyManager>();

        Transform g = transform.Find("Head");
        if (g != null)
        {
            HeadAnim = g.GetComponent<Animator>();
        }

        Transform s = transform.Find("ChargeSource");
        if(s != null)
        {
            ChargeLaserSource = s.GetComponent<AudioSource>();
        }

        ParticleSystem[] syss = GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem system in syss)
        {
            if(system.gameObject.name == "SuperPowerupSphere")
            {
                SuperPowerupSpherePartEmission = system.emission;
            }
        }

        ChasisAnim = GetComponent<Animator>();
       
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (mySpiderSense == null)
            return;

        if (z != ZoneScript.ActiveZone)
            return;

        switch (State)
        {
            case SpiderBroT2State.Patrolling:
                CheckAggro();
                Patrol();
                break;
            case SpiderBroT2State.Idle:
                CheckAggro();
                curWaitCount = 1;
                AITimeout += Time.deltaTime;
                if (AITimeout > 2)
                {
                    State = SpiderBroT2State.Patrolling;
                    AITimeout = 0;
                } 
                break;
            case SpiderBroT2State.Yell:
                AggroFriends();
                AITimeout += Time.deltaTime;
                if(AITimeout > 2)
                {
                    State = SpiderBroT2State.Aggro;
                } 
                break;
            case SpiderBroT2State.Aggro:
                Persue();
                break;
        }


         
	}

    private void Update()
    {
        float lookX = myCC.Velocity.x;
        float lookY = myCC.Velocity.z;
        bool isMoving = GlobalConstants.ZeroYComponent(myCC.Velocity).magnitude > .1f;

        Vector3 distToPlayer = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

        // Handle the head animator
        if(HeadAnim != null)
        {
            bool aggrod = State == SpiderBroT2State.Aggro;
            HeadAnim.SetBool("Aggro",aggrod);

            if (aggrod)
            {
                lookX = distToPlayer.x;
                lookY = distToPlayer.z;
            }

            HeadAnim.SetFloat("LookX", lookX);
            HeadAnim.SetFloat("LookY", lookY);
        }

        if(ChasisAnim != null)
        {
            ChasisAnim.SetBool("Moving", isMoving);
        }

        MyWeapon.RotateObject.transform.localRotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(distToPlayer.normalized)); 
    }


    void Persue()
    {
        myCC.Speed = Mathf.Max(mySpiderSense.Mob.Speed * .5f,PATROLSPEED);

        Vector3 distanceVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

        if(FireAtWill)
        { 
            // Shoot at the player and predict his position
            CController cc = playerRef.GetComponent<CController>();
            Vector3 predictedPos = GlobalConstants.ZeroYComponent(GlobalConstants.predictedPosition(playerRef.transform.position, transform.position, cc.Velocity, 40));
            predictedPos += new Vector3(UnityEngine.Random.Range(-PeekWeight, PeekWeight), 0, UnityEngine.Random.Range(-PeekWeight, PeekWeight)); // add some dirt to the shot 
            MyWeapon.FireWeapon(GlobalConstants.ZeroYComponent(predictedPos - transform.position));

            if (MyUnit.CurrentEnergy < 1)
            {
                StartCoroutine(ChargeWeapon()); 
            }

        }else  
        {
            // Check if we can see the player
            Ray r = new Ray(transform.position, distanceVector);
            if (!Physics.Raycast(r, distanceVector.magnitude, LayerMask.GetMask("Ground")))
            {
                FireAtWill = true;
            }
            else // If we can't see the player then reset the peek weight
                PeekWeight = 1;
        }

        // Lerp the peek weight back down to 0
        // This will mean if you peak out at a spider, he has a very low chance of hitting you initially
        PeekWeight = Mathf.Lerp(PeekWeight, 0, Time.deltaTime);

        // Position at a distance that is close to the player but not too close
        if (distanceVector.magnitude > DESIREDDISTANCE)
            myCC.ApplyForce(distanceVector.normalized * myCC.Speed);
        else if (distanceVector.magnitude < DESIREDDISTANCE - 1)
            myCC.ApplyForce(GlobalConstants.ZeroYComponent(transform.position - Player.transform.position).normalized * myCC.Speed);

    }

    IEnumerator ChargeWeapon()
    {
        if(ChargeLaserSource != null)
            ChargeLaserSource.Play();
         
            SuperPowerupSpherePartEmission.enabled = true; 

        yield return new WaitForSeconds(2.3f);
        FireAtWill = false;
         
            SuperPowerupSpherePartEmission.enabled = false;

    }
 
    void CheckAggro()
    {
        Vector3 distanceVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

        if (distanceVector.magnitude > AGGRORANGE)
            return;

        Ray r = new Ray(transform.position, distanceVector);
        if (!Physics.Raycast(r, distanceVector.magnitude, LayerMask.GetMask("Ground")))
        {
            State = SpiderBroT2State.Yell;
            StartYelling(); 
        }


    } 

    void StartYelling()
    {
        Camera.main.GetComponent<CamScript>().AddEffect(CamScript.CamEffect.Shake, 3);
    }

    void AggroFriends()
    { 
        // Let all your buds know it's time to ride
        Collider[] c = Physics.OverlapSphere(transform.position, RALLYRANGE, LayerMask.GetMask("Units"));
        for (int i = 0; i < c.Length; i++)
        {
            SpiderBroT1 bro = c[i].GetComponent<SpiderBroT1>();
            if (bro != null && bro != this)
            {
                if (mySpiderSense.Mob.Mob.Contains(bro))
                    continue;

                bro.mobbing = true;
                mySpiderSense.Mob.Mob.Add(bro);
                bro.State = SpiderBroT1.SpiderBroT1State.Aggro;
            }
        }
    }

    void Patrol()
    {
        Vector3[] patrolPoints = mySpiderSense.Patrols[PatrolNum].Points;

        // Check if we need to find out closest point
        if (CurrentPatrolIndex == -1)
        {
            CurrentPatrolIndex = getClosestPoint(patrolPoints);
            prevPatrolIndex = CurrentPatrolIndex;
        }

        if(CurrentPatrolIndex == -1) // if it's still -1 then jump to idle
        {
            State = SpiderBroT2State.Idle;
            return;
        }

        // Set up the right speed
        myCC.Speed = PATROLSPEED;

        // Ok so we should have our starting point lets get moving
        Vector3 pnt = patrolPoints[CurrentPatrolIndex];
        moveVec = GlobalConstants.ZeroYComponent(pnt) - GlobalConstants.ZeroYComponent(transform.position); 
         

        if (moveVec.magnitude < 1f)
        {
            // First see if we are done moving first of all 
            if(curWaitCount > WillWaitCount && WillWait)
            { 
                State = SpiderBroT2State.Idle; 
            }else
            {
                // Otherwise increase the wait chance then find the next point
                curWaitCount ++;

                // We're close enough to hit the point with this last move
                // BUT after this frame we need to find the next patrol point 
                if (mySpiderSense.Patrols[PatrolNum].Web) // If it's a web grab a random point that is close enough
                {
                    Vector3[] newPoints = getClosestPoint(patrolPoints, mySpiderSense.Patrols[PatrolNum].WebMaxDist);
                    if (newPoints.Length == 0)
                    {
                        State = SpiderBroT2State.Idle;
                        return;
                    }

                    int rnd = UnityEngine.Random.Range(0, newPoints.Length);
                    int newindex = -1;
                    for (int i = 0; i < patrolPoints.Length; i++)
                    {
                        if (newPoints[rnd] == patrolPoints[i])
                        {
                            newindex = i;
                            break;
                        }

                    }

                    // Update the index's
                    prevPatrolIndex = CurrentPatrolIndex;
                    CurrentPatrolIndex = newindex;
                }
                else
                {
                    // Handle looping
                    bool loop = mySpiderSense.Patrols[PatrolNum].Loop;
                    CurrentPatrolIndex += patrolDirection;
                    if(CurrentPatrolIndex >= patrolPoints.Length)
                    {
                        if (loop)
                            CurrentPatrolIndex = 0;
                        else
                        {
                            CurrentPatrolIndex = patrolPoints.Length - 2;
                            patrolDirection *= -1;
                        }
                    }

                    // We have an extra clause if we're not in a loop and the cur point is < 0
                    if(CurrentPatrolIndex < 0 && !loop)
                    {
                        CurrentPatrolIndex = 1;
                        patrolDirection *= -1;
                    }
                } 
            } 
        }else
        {
            moveVec = moveVec.normalized;
        }

        myCC.ApplyForce(moveVec * myCC.Speed);

    }

    /// <summary>
    /// Get the best available point closest to you at the moment
    /// </summary>
    /// <param name="points"></param>
    /// <returns>An index of the closest point in the points array</returns>
    int getClosestPoint(Vector3[] points)
    {
        float closest = 10000;
        int index = -1;
        for(int i = 0; i < points.Length; i ++)
        {
            Vector3 dst =  GlobalConstants.ZeroYComponent(transform.position - points[i]);
            if(dst.magnitude < closest)
            {
                index = i;
                closest = dst.magnitude;
            }
        }

        return index;
    }

    /// <summary>
    /// Returns an array of points that you can go to from your current position. 
    /// Does not include the previous point nor your current point.
    /// </summary>
    /// <param name="points">An array of available points on the web.</param>
    /// <param name="_dst">The maximum distance threshold for close enough points.</param>
    /// <returns>An array of points that are less then _dst units away from you.</returns>
    Vector3[] getClosestPoint(Vector3[] points, float _dst)
    {
        List<Vector3> possiblePoints = new List<Vector3>();

        for(int i = 0; i < points.Length; i ++)
        {
            if (i == CurrentPatrolIndex || i == prevPatrolIndex) // Don't go to your current index and don't go to your previous index
                continue;

            Vector3 dst = GlobalConstants.ZeroYComponent(points[i] - transform.position);
            if(dst.magnitude <= _dst)
            {
                possiblePoints.Add(points[i]);
            } 
        }

         
        return possiblePoints.ToArray();
    }
     

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    }

    public void OnHit(int _damage)
    {
        // Take a hit
        MyUnit.CurrentHealth -= _damage;



        if (MyUnit.CurrentHealth <= 0)
        {
            MyWeapon.ReleaseWeapon();
            gameObject.SetActive(false);
        }
        else
        {
            if(State != SpiderBroT2State.Yell && State != SpiderBroT2State.Aggro)
            { 
                State = SpiderBroT2State.Yell;
                StartYelling();
            }
        }

    }

    GameObject playerRef
    {
        get
        {
            if (Player == null)
                Player = GameObject.FindGameObjectWithTag("Player");

            return Player;

        }
    }

    public UnitStruct MyUnit
    {
        get
        {
            return UnitData;
        }

        set
        {
            UnitData = value;
        }
    }

    public ZoneScript myZone
    {
        get
        {
            return z;
        }

        set
        {
            z = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return State == SpiderBroT2State.Aggro;
        }

        set
        {
            State = (value) ? SpiderBroT2State.Aggro : SpiderBroT2State.Idle;
        }
    }

    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

    public WeaponBase myWeapon
    {
        get
        {
            return MyWeapon;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(mySpiderSense != null)
        {
            if(mySpiderSense.Patrols.Length-1 >= PatrolNum)
                mySpiderSense.DrawT2(mySpiderSense.Patrols[PatrolNum]);
        }
    }
}
