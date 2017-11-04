using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CController))]
public class AIStandardUnit : MonoBehaviour, IMultiArmed {

    public UnitStruct UnitData;

    public enum EnemyAIState { Idle, Aggro, Vulnerable, Defeated };
    public EnemyAIState AIState;

    public float ArmingTime;
    public float AggroRange;
    public float AggroTime;
    float curAggroTime;

    public GameObject Weapon;

    Vector3 LastSeen;

    PlayerScript player;
    MovementAI moveAI;  
    [HideInInspector]
    public  CController myCC;
    Animator myAnimator;
    public StandardUnitAnim animationHandler;
    EffectSystem myEmojis;

    public GameObject currentWeapon;
    public IWeapon WeaponSlot1;
    public IWeapon WeaponSlot2;
    public GameObject WeaponObject1;
    public GameObject WeaponObject2;

	// Use this for initialization
	public virtual void Start () {
        myCC = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myEmojis = GetComponentInChildren<EffectSystem>();
        moveAI = new MovementAI(gameObject, myCC);



        // Get the gun object in the child of this object 
        usableWeapon[] weapons = GetComponentsInChildren<usableWeapon>();
        if (weapons.Length > 0)
        {
            if (weapons[0] != null)
            {
                weapons[0].PickedUp(this);
            }
        }

        if (weapons.Length > 1)
        {
            if (weapons[1] != null)
            {
                weapons[1].PickedUp(this); 
            } 
        }
        //myWeapon.Owner = this;

        animationHandler = new StandardUnitAnim(gameObject, WeaponObject1,WeaponObject2, myAnimator);

        Vector3 RootPosition = new Vector3(-0.138f, -0.138f, 0); // The center of the player (where it would look like he's holding the weapon) is at this position, so we rotate everything around it
        Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
        Vector3 HolsteredRotation = new Vector3(0, 0, -88);
        animationHandler.HeldPosition = RootPosition;
        animationHandler.HolsteredPosition = HolsteredPosition;
        animationHandler.HolsteredRotation = HolsteredRotation;


    }

    // Update is called once per frame
    public virtual void Update () {
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

        if (WeaponObject2 == null)
            currentWeapon = WeaponObject1;
    }

    public virtual void UpdateAnimationController()
    {
        Vector3 looking = Vector3.zero;

        if (SeePlayer())
        {
            looking = playerRef.transform.position - transform.position;
            LastSeen = playerRef.transform.position;
        }
        else
        {
            looking = LastSeen - transform.position;
        }

        if (AIState != EnemyAIState.Aggro) 
            looking.z = -.1f;

        animationHandler.LookingVector = looking;


        animationHandler.velocity = myCC.Velocity;


        animationHandler.holdGun = AIState == EnemyAIState.Aggro;
        myAnimator.SetBool("Passive", AIState == EnemyAIState.Idle);


        animationHandler.gunObject1 = WeaponObject1;
        animationHandler.gunObject2 = WeaponObject2; 
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

    bool SeePlayer()
    {
        Vector3 distVec = player.transform.position - transform.position;
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

    public IWeapon myWeapon
    {
        get
        {
            if (WeaponSlot1 != null || WeaponSlot2 != null)
                return (currentWeapon == WeaponObject1) ? WeaponSlot1 : WeaponSlot2;
            else
                return null;
        }
    }

    public void TossWeapon(Vector3 _dir)
    {
        StartCoroutine(toss(_dir));
    }

    IEnumerator toss(Vector3 _dir)
    {
        yield return new WaitForEndOfFrame();
        if (WeaponObject2 == currentWeapon)
        {
            WeaponObject2.transform.parent = null;
            WeaponObject2.transform.position = transform.position;
            WeaponObject2.GetComponent<usableWeapon>().Toss(_dir, transform.position);

            WeaponSlot2 = null;
            WeaponObject2 = null;
            currentWeapon = null;

            if (WeaponObject1 != null)
                currentWeapon = WeaponObject1;

            myVisualizer.BuildAmmoBar();
        }
        else if (WeaponObject1 == currentWeapon)
        {
            WeaponObject1.transform.parent = null;
            WeaponObject1.transform.position = transform.position;
            WeaponObject1.GetComponent<usableWeapon>().Toss(_dir, transform.position);

            WeaponSlot1 = null;
            WeaponObject1 = null;
            currentWeapon = null;

            if (WeaponObject2 != null)
                currentWeapon = WeaponObject2;

            myVisualizer.BuildAmmoBar();
        }
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
            this.gameObject.SetActive(false);
        }
    }

  
     public void PickUpWeapon(GameObject _newWeapon)
    {
        // First get a reference to the weapon itself
        IWeapon newRef = _newWeapon.GetComponentInChildren<IWeapon>();
        if (newRef == null)
            return;
 
        if (WeaponSlot1 == null)
        {
            SetPrimary(newRef, _newWeapon);
            myVisualizer.BuildAmmoBar(); // Let the visualizer 
            return;
        }


        if (WeaponSlot1 != null && WeaponSlot2 == null)
        {
            SetSecondary(newRef, _newWeapon);
            myVisualizer.BuildAmmoBar(); // Let the visualizer 
            return;
        }

        // If we full toss the weapon that we're currently holding and replace it
        if (WeaponSlot1 != null && WeaponSlot2 != null)
        {
            // Toss the current active weapon if you're already carrying 2 things 
            if (currentWeapon == WeaponObject2)
            {
                WeaponObject2.transform.parent = null;
                WeaponObject2.transform.position = transform.position;
                WeaponObject2.GetComponent<usableWeapon>().Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position), transform.position);
                SetSecondary(newRef, _newWeapon);
                myVisualizer.BuildAmmoBar();
                return;
            }

            if (currentWeapon == WeaponObject2)
            {
                WeaponObject2.transform.parent = null;
                WeaponObject2.transform.position = transform.position;
                WeaponObject2.GetComponent<usableWeapon>().Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position), transform.position);
                SetPrimary(newRef, _newWeapon);
                myVisualizer.BuildAmmoBar();
                return;
            }
        }

    }

    void SetSecondary(IWeapon newRef, GameObject _newWeapon)
    {
        //  If we already have one weapon and can pick up another
        WeaponObject2 = _newWeapon; // Set the secondary equal to the new weapon
        WeaponObject2.transform.parent = transform; // Set it's transform to the parent
        WeaponObject2.transform.position = Vector3.zero; // Set it's position to 0
        WeaponSlot2 = newRef; // Set the iweapon reference to the new iweapon
        WeaponSlot2.Owner = this; // Set it's owner
        _newWeapon.transform.localScale = new Vector3(1, 1, 1); // Make sure it's the right scale
        currentWeapon = _newWeapon; // If you picked it up chances are you want it equiped
    }

    void SetPrimary(IWeapon newRef, GameObject _newWeapon)
    {
        //  If we already have one weapon and can pick up another
        WeaponObject1 = _newWeapon; // Set the secondary equal to the new weapon
        WeaponObject1.transform.parent = transform; // Set it's transform to the parent
        WeaponObject1.transform.position = Vector3.zero; // Set it's position to 0
        WeaponSlot1 = newRef; // Set the iweapon reference to the new iweapon
        WeaponSlot1.Owner = this; // Set it's owner
        _newWeapon.transform.localScale = new Vector3(1, 1, 1); // Make sure it's the right scale
        currentWeapon = _newWeapon; // If you picked it up chances are you want it equiped
    }

}
