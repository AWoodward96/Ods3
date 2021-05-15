using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBroT1 : MonoBehaviour, IArmed{


    [Header("Spider Data")]
    CController myCC;

    public UnitStruct UnitData;
    public int Damage = 5;

    Vector3 desiredPoint;
    Vector3 moveVec;

    GameObject Player;
    Animator ChasisAnim;
    Animator HeadAnim;

    ParticleSystem MunchPart;
    HealthBar HealthVis;

    Vector3 distToPlayer;

    public enum SpiderBroT1State { MovingToFood, Yell, Fleeing, Aggro, Eating}
    public SpiderBroT1State State;

    [Header("Spider Senses")]
    public SpiderSense mySpiderSense;
    public bool pathSelected;
    public bool mobbing;
    public int PathNum = 0;
    public int PathPoint = 0;
    public int FoodNum = 0;
    public int SpiderWeight = 10; 

    float AITimeout = 0;

    float fleeIntensity = 1;

    const int TOOCLOSEMIN = 5;
    const int TOOCLOSEMAX = 7;
    const int RALLYRANGE = 5;
    const int PROXIMITYWEIGHT = 10;
    const int PROXIMITYDISTMAX = 10;
    const int PROXIMITYDISTMIN = 5;
    const float ATTACKDISTANCE = 2.5f;
    const float SPEEDMULTIPLIER = .2f;
    const float PLAYIDLECHANCE = .002f;

    public bool attacking = false;

    ZoneScript z;

    [Space(20)]
    public AudioClip[] IdleChirp;
    public AudioClip[] AggroChirp;
    public AudioClip[] YellChirp;
    public AudioClip AlertChirp;
    public AudioClip MunchChirp;

    AudioSource mySource;

    [Space(10)]
    public bool DEBUG = false;

    // Use this for initialization
    void Start () {
        myCC = GetComponent<CController>();
        ChasisAnim = GetComponent<Animator>();
        mySource = GetComponent<AudioSource>();
        HealthVis = GetComponentInChildren<HealthBar>();

        MunchPart = GetComponentInChildren<ParticleSystem>();

        Transform g = transform.Find("Head");
        if(g != null)
        {
            HeadAnim = g.GetComponent<Animator>();
        } 

        if (mySpiderSense == null)
            Debug.Log("Spider Sense not set on: " + this.gameObject);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (mySpiderSense == null)
            return;

        if (z != ZoneScript.ActiveZone)
            return;


        switch (State)
        {
            case SpiderBroT1State.MovingToFood:
                // raycast to the desired point. Can you see it?
                Ray r = new Ray(transform.position, GlobalConstants.ZeroYComponent( desiredPoint - transform.position));
                if (Physics.Raycast(r, r.direction.magnitude, LayerMask.GetMask("Ground")))
                {
                    mySpiderSense.Food[FoodNum].AtFood.Remove(this.gameObject);
                    PathNum = 0;
                    PathPoint = 0;
                    FoodNum = 0;
                    pathSelected = false;
                }

                getBestPoint();
                InterruptSearch(); 
                Move();
                break;
            case SpiderBroT1State.Eating:
                InterruptSearch();
                Eat();
                break;
            case SpiderBroT1State.Fleeing:
                Flee();
                break;
            case SpiderBroT1State.Yell:
                Yell();
                break;
            case SpiderBroT1State.Aggro:  
                AggroMove();
                break;
        }

        HandleAudio();
         
	}

    private void Update()
    {
        float lookX = myCC.Velocity.x;
        float lookY = myCC.Velocity.z;
        bool isMoving = GlobalConstants.ZeroYComponent(myCC.Velocity).magnitude > .1f;

        distToPlayer = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

        if(MunchPart != null)
        {
            MunchPart.gameObject.SetActive((State == SpiderBroT1State.Eating) && !isMoving);
        }

        // If we have a chasis animator
        if (ChasisAnim != null)
        {
            ChasisAnim.SetBool("Moving", GlobalConstants.ZeroYComponent(myCC.Velocity).magnitude > .1f);
            ChasisAnim.SetBool("Yell", State == SpiderBroT1State.Yell);
            ChasisAnim.SetBool("Eating", (State == SpiderBroT1State.Eating) &&  !isMoving);

            float speed = (State == SpiderBroT1State.Fleeing) ? fleeIntensity : 1;

            if (State == SpiderBroT1State.Aggro)
                speed = mySpiderSense.Mob.Speed;

            if (isMoving)
            {
                if(State == SpiderBroT1State.Yell)
                { 
                    lookX = distToPlayer.x;
                    lookY = distToPlayer.z;
                }


                ChasisAnim.SetFloat("LookX", lookX);
                ChasisAnim.SetFloat("LookY", lookY);
            } else if(State == SpiderBroT1State.Eating)
            {
                // time to munch!
                Vector3 toFood = mySpiderSense.Food[FoodNum].Position - transform.position; // Zeroing y on this doesn't matter since we ignore the y anyway
                HeadAnim.SetFloat("LookY", toFood.z);
                HeadAnim.SetFloat("LookX", toFood.x);
            }

            ChasisAnim.SetFloat("Speed", speed);
        }


        if(HeadAnim != null)
        {
            // If we're moving then look where we are moving
            if(isMoving)
            {
                HeadAnim.SetFloat("LookX", lookX);
                HeadAnim.SetFloat("LookY", lookY);
            }else if(State == SpiderBroT1State.Eating)
            {
                // time to munch!
                Vector3 toFood = mySpiderSense.Food[FoodNum].Position - transform.position; // Zeroing y on this doesn't matter since we ignore the y anyway
                HeadAnim.SetFloat("LookY", toFood.z);
                HeadAnim.SetFloat("LookX", toFood.x);
            }

            HeadAnim.SetBool("Aggro", State == SpiderBroT1State.Aggro);
        }
    }

    void Move()
    {
        moveVec = GlobalConstants.ZeroYComponent(desiredPoint - transform.position);
        if (moveVec.magnitude > 1)
            moveVec = moveVec.normalized;

        myCC.ApplyForce(moveVec * myCC.Speed);
    }



    void AggroMove()
    {  
        if (attacking)
            return;

        if(distToPlayer.magnitude > ATTACKDISTANCE)
        {
            // Move towards the player
            moveVec = distToPlayer;


            List<IDamageable> theMob = mySpiderSense.Mob.Mob;
            Vector3 pushingRoom = Vector3.zero;
            for (int i = 0; i < theMob.Count; i++)
            {
                pushingRoom = transform.position - theMob[i].gameObject.transform.position;

                // As you move you want space between you and your bretheran
                if (pushingRoom.magnitude < PROXIMITYDISTMAX)
                {
                    // As you get closer to the player this needs to matter less so you can actually hit the player
                    moveVec += pushingRoom.normalized * (PROXIMITYWEIGHT - GlobalConstants.inverseMap(distToPlayer.magnitude, 0, 20, 0, PROXIMITYWEIGHT));
                }
            }
             

            myCC.ApplyForce(moveVec.normalized * mySpiderSense.Mob.Speed);
        }
        else
        {
            //if we're close enough to melee the player lets melee the player
            attacking = true;
            StartCoroutine(Attack());  
        }
        
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(.1f);
         
        myCC.ApplyForce(distToPlayer.normalized * 25);

        Collider[] cols = Physics.OverlapBox(transform.position + moveVec.normalized, new Vector3(2, 2, 2));
        for(int i = 0; i < cols.Length; i ++)
        {
            PlayerScript p = cols[i].GetComponent<PlayerScript>();
            if (p != null)
            {
                p.OnHit(Damage);
                break;
            }
        }

        // CD
        yield return new WaitForSeconds(.5f);
        attacking = false;

    }

    void Flee()
    {
        // If you haven't run away from the player then start the ai timeout
        if(moveVec.magnitude > TOOCLOSEMAX)
        {
            AITimeout += Time.deltaTime;
            if (AITimeout > 1)
            {
                State = SpiderBroT1State.MovingToFood;
            }
        }
        else
        {
            // run away from the player!
            AITimeout = 0;

            moveVec = GlobalConstants.ZeroYComponent(transform.position - playerRef.transform.position);
            fleeIntensity = GlobalConstants.inverseMap(moveVec.magnitude, 0, TOOCLOSEMAX, .3f, 2.5f);
            fleeIntensity = Mathf.Clamp(fleeIntensity, .3f, 2); 
            myCC.ApplyForce(moveVec.normalized * fleeIntensity);
        }
        
    }

    void Eat()
    {
        // Get a list of all the spiders at this food
        List<GameObject> d = mySpiderSense.Food[FoodNum].AtFood;
        
        // For one, move towards the food until you're close enough to it
        Vector3 toFood = GlobalConstants.ZeroYComponent(mySpiderSense.Food[FoodNum].Position - transform.position);
        moveVec = Vector3.zero;
        if (toFood.magnitude > 2)
            moveVec += toFood;

        Vector3 pushingRoom = Vector3.zero;
        // Make sure you spread out around the food
        for(int i = 0; i < d.Count; i ++)
        {
            if (d[i].gameObject == this.gameObject) 
                continue;  

            pushingRoom = GlobalConstants.ZeroYComponent(transform.position - d[i].gameObject.transform.position);

            // As you move you want space between you and your bretheran
            if (pushingRoom.magnitude < 2) // These values of 2 comapring magnitude just seem to work. Anything less than 2 runs into collision problems between the spiders colliders
            {
                // As you get closer to the player this needs to matter less so you can actually hit the player
                moveVec += pushingRoom.normalized;
            }
        }

        myCC.ApplyForce(moveVec.normalized);

        // update your weight
        SpiderWeight = mySpiderSense.Food[FoodNum].AtFood.IndexOf(this.gameObject);

       
    }

    void Yell()
    {
        moveVec = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);
         

        Vector3 playerFromFood = GlobalConstants.ZeroYComponent(playerRef.transform.position - mySpiderSense.Food[FoodNum].Position);
        desiredPoint = mySpiderSense.Food[FoodNum].Position + (playerFromFood.normalized * 2.5f);
        
        // move as if on a leash around the food
        myCC.ApplyForce(desiredPoint - transform.position); 

        if(moveVec.magnitude > TOOCLOSEMAX)
        {
            AITimeout += Time.deltaTime;
            if(AITimeout > 2)
            {
                State = SpiderBroT1State.MovingToFood;
            }
        }
        else
        {
            AITimeout = 0;
        }

    }


    public void StartAggro()
    {
        // If we're not already aggrod then play the aggro noise
        if (mySource != null && State != SpiderBroT1State.Aggro)
        { 
            mySource.clip = AlertChirp;
            mySource.pitch = 1;
            mySource.Play();
        }


        // Then set your own state to aggro
        State = SpiderBroT1State.Aggro;


        // Then tell the SpiderSense that you're mad
        mobbing = true;
        if(mySpiderSense.Mob == null || mySpiderSense.Mob.Leader == null)
        {
            mySpiderSense.Mob = new SpiderMob();
            mySpiderSense.Mob.Leader = this.gameObject;
            mySpiderSense.Mob.Mob.Add(this);
        }

        // Then let all your buds know that you mean business
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
                bro.State = SpiderBroT1State.Aggro;
            }
        }
    }


    /// <summary>
    /// Find which point is the best point to move to right now.
    /// First the spider will need to select a spidersense path to follow to its desired food
    /// </summary>
    void getBestPoint()
    {
        Vector3 point = Vector3.zero;
        float curMag = 100000;
        float foo = 0;

        // If we have not yet selected a spider sense path then we need to get that
        if (!pathSelected)
        {
            SpiderFood PathFood;
            for (int n = 0; n < mySpiderSense.Paths.Length; n++)
            {
                Vector3[] curPath = mySpiderSense.Paths[n].Points;
                PathFood = mySpiderSense.Food[mySpiderSense.Paths[n].myFood];
                for (int p = 0; p < curPath.Length; p++)
                {
                    foo = GlobalConstants.ZeroYComponent(curPath[p] - transform.position).magnitude;

                    // Only select this as your food and path if there are less then 3 spiders on that path
                    if (foo < curMag && PathFood.AtFood.Count < 3)
                    {
                        curMag = foo;
                        PathNum = n;
                        PathPoint = p;
                        FoodNum = mySpiderSense.Paths[n].myFood; 
                    } 
                }
            }

            SpiderWeight = mySpiderSense.Food[FoodNum].AtFood.Count;
            mySpiderSense.Food[FoodNum].AtFood.Add(this.gameObject); 
            pathSelected = true;
        }else // If we do have a path selected
        {
            // We need to move up the path 
            Vector3 distVec = GlobalConstants.ZeroYComponent(desiredPoint - transform.position);

            if (pathSelected && distVec.magnitude < 1f)
            {
                if (PathPoint > 0)
                    PathPoint -= 1;
                else
                    State = SpiderBroT1State.Eating; // Lets chow!
            } 

            // If we do have a path selected we should update our weight
            SpiderWeight = mySpiderSense.Food[FoodNum].AtFood.IndexOf(this.gameObject);

        }


        desiredPoint = mySpiderSense.Paths[PathNum].Points[PathPoint]; 
    }

    void HandleAudio()
    {
        if (mySource == null)
            return;

        if(State != SpiderBroT1State.Yell)
        {
            float rnd = UnityEngine.Random.Range(0f, 1f);
            float chance = PLAYIDLECHANCE;
            chance *= (State == SpiderBroT1State.Aggro) ? 2 : 1;

            if (rnd < chance)
            {
                bool AggroOrPassive = (State == SpiderBroT1State.Aggro);
                int index = UnityEngine.Random.Range(0, (AggroOrPassive) ? AggroChirp.Length : IdleChirp.Length);

                mySource.clip = (AggroOrPassive) ? AggroChirp[index] : IdleChirp[index];
                mySource.pitch = UnityEngine.Random.Range(.95f, 1.05f);
                mySource.Play();
            }
        }
        else
        { 
            if(!mySource.isPlaying && distToPlayer.magnitude < TOOCLOSEMIN)
            { 
                int index = UnityEngine.Random.Range(0, YellChirp.Length);
                mySource.clip = YellChirp[index];
                mySource.Play();
            }
        }

      
    }

    void InterruptSearch()
    {
        if (playerRef == null)
            return;
         
        // first things first can we even see the player right now
        Ray r = new Ray(transform.position,distToPlayer);
        if (Physics.Raycast(r, distToPlayer.magnitude, LayerMask.GetMask("Ground"))) 
            return; 

        // Ok we can see the player, is he too close?
        if (distToPlayer.magnitude < TOOCLOSEMIN)
        {
            // Hive mind --
            // If there are more then 3 spiders at this food then one of them will yell at you
            if(HiveMindPoximity())
            {
                if(SpiderWeight == 0)
                {
                    if(DEBUG) Debug.Log("SCREE");
                    State = SpiderBroT1State.Yell;
                }
            }else // If there isn't more then 3 then they're spooked and don't think they can take you so run!
            {
                CController playerCC = playerRef.GetComponent<CController>();

                if(GlobalConstants.ZeroYComponent(playerCC.Velocity).magnitude > .1f)
                { 
                    // Flee!
                    State = SpiderBroT1State.Fleeing;

                    // Reset the food pathing variables
                    if (mySpiderSense.Food[FoodNum].AtFood.Contains(this.gameObject))
                        mySpiderSense.Food[FoodNum].AtFood.Remove(this.gameObject);
                    pathSelected = false;
                    SpiderWeight = 10;
                }
            }



        } 
    }

    bool HiveMindPoximity()
    {
        List<GameObject> atFood = mySpiderSense.Food[FoodNum].AtFood;

        Vector3 toFood = GlobalConstants.ZeroYComponent(mySpiderSense.Food[FoodNum].Position - transform.position);
        if (toFood.magnitude > 4)
            return false;

        // well if there just isn't even enough spiders at this food then set it to false
        if(atFood.Count < 3)
        {
            if(DEBUG) Debug.Log("Not enough spiders at this food");
            return false;
        }

        // if it's still true
        int nearCount = 0;
        Vector3 foo = Vector3.zero;
        for(int i = 0; i < atFood.Count; i++) // Are enoguh of them close enough together?
        {
            foo = GlobalConstants.ZeroYComponent(transform.position - atFood[i].gameObject.transform.position);
            if (foo.magnitude < 4)
                nearCount++;
        }

        // If they're not close enough return false
        if (nearCount < 3)
        { 
            if (DEBUG) Debug.Log("Spiders at this food not close enough");
            return false;
        }

        return true;
    }

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    }

    public void OnHit(int _damage)
    {
        // Take a hit
        MyUnit.CurrentHealth -= _damage;
        if(HealthVis != null)
        {
            HealthVis.ShowMenu();
        } 
        

        if (MyUnit.CurrentHealth > 0)
        {
            // If you're still alive then alert your bretheran that you've been hit and that they need to get angry
            StartAggro();

        }
        else
        {
            if (mobbing)
            {
                mySpiderSense.Mob.Mob.Remove(this);
                if (mySpiderSense.Mob.Leader == this.gameObject)
                {
                    if (DEBUG) Debug.Log("Leader died getting new one");
                    mySpiderSense.Mob.GetNewLeader();
                }
            }
             
            if (mySpiderSense.Food[FoodNum].AtFood.Contains(this.gameObject))
                mySpiderSense.Food[FoodNum].AtFood.Remove(this.gameObject);

            gameObject.SetActive(false);
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
            return State == SpiderBroT1State.Aggro;
        }

        set
        {
            State = (value) ? SpiderBroT1State.Aggro : SpiderBroT1State.Fleeing;
        }
    }


    public HealthBar myVisualizer
    {
        get
        {
            return HealthVis;
        }
    }

    public WeaponBase myWeapon
    {
        get
        {
            return null;
        }
    }



    private void OnDrawGizmosSelected()
    {
        if(mySpiderSense != null)
        {
            if(pathSelected)
            {
                mySpiderSense.DrawT1(mySpiderSense.Paths[PathNum]);
            }

        }
    }
}
