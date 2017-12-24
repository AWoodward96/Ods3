using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CController))]
public class AIStandardUnit : MonoBehaviour, IMultiArmed {

    [Header("AIStandardUnit")]
    public UnitStruct UnitData;
    public StandardUnitAnim animationHandler;

    public enum EnemyAIState { Idle, Aggro, Vulnerable, Defeated };
    public EnemyAIState AIState;

    public float ArmingTime;
    public float AggroRange;
    public float AggroTime;
    float curAggroTime;

    public GameObject Weapon;

    [HideInInspector]
    public Vector3 LastSeen;

    [HideInInspector]
    public PlayerScript player;
    MovementAI moveAI;  
    [HideInInspector]
    public  CController myCC;
    Animator myAnimator;
    EffectSystem myEmojis;

    public WeaponBase currentWeapon;
    public WeaponBase WeaponSlot1;
    public WeaponBase WeaponSlot2;
    //public GameObject currentWeapon;
    //public IWeapon WeaponSlot1;
    //public IWeapon WeaponSlot2;
    //public GameObject WeaponObject1;
    //public GameObject WeaponObject2;

    public ZoneScript Zone;

	// Use this for initialization
	public virtual void Start () {
        myCC = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myEmojis = GetComponentInChildren<EffectSystem>();
        moveAI = new MovementAI(gameObject, myCC);
         

        animationHandler.Initialize(gameObject, WeaponSlot1, WeaponSlot2, myAnimator); 
    }

    // Update is called once per frame
    public virtual void Update () {
        // Don't do anything if you're not in his zone
        if (myZone != ZoneScript.ActiveZone)
            return;

        switch (AIState)
        {
            case EnemyAIState.Idle:
                IdleState(); 
                break;
            case EnemyAIState.Vulnerable:
                VulnState();
                break;
            case EnemyAIState.Aggro:
                AggroState();
                break;
        } 
        moveAI.Update();

 
        UpdateAnimationController();

        if (WeaponSlot2 == null)
            currentWeapon = WeaponSlot1;
    }

    public virtual void UpdateAnimationController()
    {

        if(AIState != EnemyAIState.Idle) // If we're not in idle state ...
        {
            Vector3 looking = Vector3.zero; // We should change the looking vector to look at the player
            if (SeePlayer()) // If we can see the player ...
            {
                looking = playerRef.transform.position - transform.position; // Look at him
                LastSeen = playerRef.transform.position;
            }
            else
            {
                // If we can't see the player then look at the last position he was at
                looking = LastSeen - transform.position;
            }


            if (AIState != EnemyAIState.Aggro)
                looking.z = -.1f;

            animationHandler.LookingVector = looking;
        } 

        animationHandler.velocity = myCC.Velocity;


        animationHandler.holdGun = AIState == EnemyAIState.Aggro;
        myAnimator.SetBool("Passive", AIState == EnemyAIState.Idle);


        animationHandler.gunObject1 = WeaponSlot1;
        animationHandler.gunObject2 = WeaponSlot2; 
        animationHandler.activeGunObject = currentWeapon;
        animationHandler.Update();
    }

    // Check if we can aggro onto the player
    public virtual bool CheckAggro()
    { 
        Vector3 distVec = playerRef.transform.position - transform.position;
        Ray r = new Ray(transform.position, distVec.normalized);

        if (distVec.magnitude <= AggroRange)
            return !Physics.Raycast(r, distVec.magnitude, LayerMask.GetMask("Ground"));
        else
            return false;
    }

    public virtual void IdleState()
    {
        // The first thing we do is to check to see if we actually get out of being idle
        if (CheckAggro())
        {
            curAggroTime = 0;
            AIState = EnemyAIState.Vulnerable;

            if (myEmojis != null)
                myEmojis.Fire(EffectSystem.EffectType.Alert, transform.position + (Vector3.up * 4));
            return;
        }
    }

    public virtual void AggroState()
    { 

        // First get some references
        Vector3 playerPos = playerRef.transform.position;
        Vector3 distVec = playerRef.transform.position - transform.position;
 

        // If we haven't started pathing somewhere
        if (moveAI.ActionComplete)
        {
            float chances = .5f;
            if (moveAI.lastType == MovementAI.MovementAIType.Wander)
                chances = .25f;

            if (moveAI.lastType == MovementAI.MovementAIType.Wait)
                chances = .75f;

            // Flip that weighted coin
            float val = UnityEngine.Random.value;
            bool rnd = (val < chances);
            if (rnd)
                moveAI.Wander(1, myCC.Speed);
            else
                moveAI.Wait(2);

        }

        if (moveAI.curType == MovementAI.MovementAIType.Wait)
        {
            // Raycast to the player
            if (SeePlayer())
            {
                LastSeen = player.transform.position;
                myWeapon.FireWeapon(distVec);
            }
        }

    }


    public virtual void VulnState()
    {
        // AKA the arming state 
        curAggroTime += Time.deltaTime; 

        if(curAggroTime >= AggroTime - ArmingTime)
        {
            myAnimator.SetBool("Aggressive", true);
        }
        
        if (curAggroTime >= AggroTime)
        {
            myAnimator.SetBool("Aggressive", true);
            AIState = EnemyAIState.Aggro;
        }
    }
     

    PlayerScript playerRef
    {
        get
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

            return player;
        }
    }

    public bool SeePlayer()
    {
        Vector3 distVec = playerRef.transform.position - transform.position;
        return !Physics.Raycast(transform.position, distVec, distVec.magnitude, LayerMask.GetMask("Ground"));
    }


    public virtual UnitStruct MyUnit
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

    public virtual HealthBar myVisualizer
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
            return currentWeapon; 
        }
    }

    public void TossWeapon(Vector3 _dir)
    {
        StartCoroutine(toss(_dir));
    }

    IEnumerator toss(Vector3 _dir)
    {
        yield return new WaitForEndOfFrame();

 

        currentWeapon.heldData.Toss(_dir, transform.position);
        if (WeaponSlot2 == currentWeapon)
        {
            // Call the toss method
            currentWeapon = WeaponSlot1;
            WeaponSlot2 = null;
        }
        else if (WeaponSlot1 == currentWeapon)
        {
            currentWeapon = WeaponSlot2;
            WeaponSlot1 = null;
        }

        myVisualizer.BuildAmmoBar();

        //if (WeaponSlot1 == currentWeapon)
        //{

        //    WeaponObject2.transform.parent = null;
        //    WeaponObject2.transform.position = transform.position;
        //    WeaponObject2.GetComponent<usableWeapon>().Toss(_dir, transform.position);

        //    WeaponSlot2 = null;
        //    WeaponObject2 = null;
        //    currentWeapon = null;

        //    if (WeaponObject1 != null)
        //        currentWeapon = WeaponObject1;

        //    myVisualizer.BuildAmmoBar();
        //}
        //else if (WeaponObject1 == currentWeapon)
        //{
        //    Debug.Log("Tossing2");
        //    WeaponObject1.transform.parent = null;
        //    WeaponObject1.transform.position = transform.position;
        //    WeaponObject1.GetComponent<usableWeapon>().Toss(_dir, transform.position);

        //    WeaponSlot1 = null;
        //    WeaponObject1 = null;
        //    currentWeapon = null;

        //    if (WeaponObject2 != null)
        //        currentWeapon = WeaponObject2;

        //    myVisualizer.BuildAmmoBar();
        //}
    }

    public virtual bool Triggered
    {
        get
        {
            return false;
        }

        set
        {

        }
    }

    public virtual void Activate()
    {

    }
 

    public virtual void OnHit(int _damage)
    {
        if (myZone != ZoneScript.ActiveZone) // Don't let them take damage if you're not in their scene
            return;

        UnitData.CurrentHealth -= _damage;
        myVisualizer.ShowMenu();
 
        if(AIState == EnemyAIState.Idle)
        {
            curAggroTime = 0;
            AIState = EnemyAIState.Vulnerable;

            if (myEmojis != null)
                myEmojis.Fire(EffectSystem.EffectType.Alert, transform.position + (Vector3.up * 4));
        }

        if (UnitData.CurrentHealth <= 0)
        {
            GameObject obj = Resources.Load("Prefabs/Particles/deathPartParent") as GameObject;
            Instantiate(obj, transform.position, obj.transform.rotation);

            if(currentWeapon != null)
                currentWeapon.ReleaseWeapon();
             

            this.gameObject.SetActive(false);
        }
    }

  
     public void PickUpWeapon(WeaponBase _newWeapon)
    {
        //// First get a reference to the weapon itself
        if (_newWeapon == null)
            return;

        _newWeapon.myOwner = this; // Set it's owner
        if (WeaponSlot1 == null)
        {
            WeaponSlot1 = _newWeapon; 
            myVisualizer.BuildAmmoBar(); // Let the visualizer know
            currentWeapon = WeaponSlot1;
            return;
        }

        if(WeaponSlot1 != null && WeaponSlot2 == null)
        {
            WeaponSlot2 = _newWeapon; 
            myVisualizer.BuildAmmoBar(); // Let the visualizer know
            currentWeapon = WeaponSlot2;
            return;
        }

        // If we're full, toss the weapon that we're currently holding and replace it
        if(WeaponSlot1 != null && WeaponSlot2 != null)
        {
            // Toss the current active weapon
            currentWeapon.myOwner = null;
            currentWeapon.heldData.Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position), transform.position);
            if(currentWeapon == WeaponSlot2)
            {
                WeaponSlot2 = _newWeapon;
                currentWeapon = WeaponSlot2;
                myVisualizer.BuildAmmoBar();
            }

            if (currentWeapon == WeaponSlot1)
            {
                WeaponSlot1 = _newWeapon;
                currentWeapon = WeaponSlot1;
                myVisualizer.BuildAmmoBar();
            }

        }
       

    }

    //void SetSecondary(IWeapon newRef, GameObject _newWeapon)
    //{
    //    //  If we already have one weapon and can pick up another
    //    WeaponObject2 = _newWeapon; // Set the secondary equal to the new weapon
    //    WeaponObject2.transform.parent = transform; // Set it's transform to the parent
    //    WeaponObject2.transform.position = Vector3.zero; // Set it's position to 0
    //    WeaponSlot2 = newRef; // Set the iweapon reference to the new iweapon
    //    _newWeapon.transform.localScale = new Vector3(1, 1, 1); // Make sure it's the right scale
    //    currentWeapon = _newWeapon; // If you picked it up chances are you want it equiped
    //}

    //void SetPrimary(IWeapon newRef, GameObject _newWeapon)
    //{
    //    //  If we already have one weapon and can pick up another
    //    WeaponObject1 = _newWeapon; // Set the secondary equal to the new weapon
    //    WeaponObject1.transform.parent = transform; // Set it's transform to the parent
    //    WeaponObject1.transform.position = Vector3.zero; // Set it's position to 0
    //    WeaponSlot1 = newRef; // Set the iweapon reference to the new iweapon
    //    WeaponSlot1.Owner = this; // Set it's owner
    //    _newWeapon.transform.localScale = new Vector3(1, 1, 1); // Make sure it's the right scale
    //    currentWeapon = _newWeapon; // If you picked it up chances are you want it equiped
    //}

    public virtual void Reset()
    {
        animationHandler.HolsteredRotation = new Vector3(0, 0, -88);
        animationHandler.HolsteredPosition = new Vector3(0, .5f, 0);
        animationHandler.HeldPosition = new Vector3(-0.138f, -0.138f, 0);
        AggroTime = 2;
        ArmingTime = 1;
        AggroRange = 15;
        UnitData.MaxHealth = 30;
        UnitData.CurrentHealth = 30;
    }

    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }


    //void SetWeaponsOwner(WeaponBase w)
    //{
    //    w.heldData.Pickup(this);
    //}
}
