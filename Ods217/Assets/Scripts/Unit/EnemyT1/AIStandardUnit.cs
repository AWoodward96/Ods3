﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(CController))]
public class AIStandardUnit : MonoBehaviour, IMultiArmed, IPawn, ISavable {

    [Header("AIStandardUnit")]
    public UnitStruct UnitData;
    public StandardUnitAnim animationHandler;

    public enum EnemyAIState { Idle, Aggro, Vulnerable, Defeated, Stunned };
    public EnemyAIState AIState;
    EnemyAIState lastState;

    public float ArmingTime;
    public float AggroRange;
    public float AggroTime;
    float curAggroTime;

    public GameObject Weapon;

    [HideInInspector]
    public Vector3 LastSeen;
    public bool UpdateLookingVector = true;

    [HideInInspector]
    public PlayerScript player;
    public MovementAI moveAI;  
    [HideInInspector]
    public  CController myCC;
    [HideInInspector]
    public Animator myAnimator;
    protected EffectSystem myEmojis;

    public WeaponBase currentWeapon;
    public WeaponBase WeaponSlot1;
    public WeaponBase WeaponSlot2; 

    public ZoneScript Zone;

	[Header("ISavable Variables")]
	public int saveID = -1;

	[HideInInspector]
	public bool saveIDSet = false;

	public int SaveID
	{
		get
		{
			return saveID;
		}
		set
		{
			saveID = value;
		}
	}

	public bool SaveIDSet
	{
		get
		{
			return saveIDSet;
		}
		set
		{
			saveIDSet = value;
		}
	}
     

	// Use this for initialization
	public virtual void Start () {
        myCC = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myEmojis = GetComponentInChildren<EffectSystem>();
        moveAI = new MovementAI(gameObject, myCC);
         

        animationHandler.Initialize(gameObject, WeaponSlot1, WeaponSlot2, myAnimator); 
    }

    // Update is called once per frame
    public virtual void FixedUpdate () {
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
            case EnemyAIState.Defeated:
                DefeatedState();
                break; 
        } 
        moveAI.Update();

 
        UpdateAnimationController();

        if (WeaponSlot2 == null)
            currentWeapon = WeaponSlot1;
    }

    public virtual void UpdateAnimationController()
    {

        if(AIState != EnemyAIState.Idle && UpdateLookingVector) // If we're not in idle state ...
        {
            Vector3 looking = Vector3.zero; // We should change the looking vector to look at the player
            if (SeePlayer()) // If we can see the player ...
            {
                looking = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position); // Look at him
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
        Vector3 distVec = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);
 

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
                moveAI.Wander(UnityEngine.Random.Range(.5f,1), myCC.Speed);
            else
                moveAI.Wait(UnityEngine.Random.Range(.5f, 1));

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
     

    public virtual PlayerScript playerRef
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

    public virtual WeaponBase myWeapon
    {
        get
        {
            if (WeaponSlot1 == null && WeaponSlot2 == null)
                return null;

            return currentWeapon; 
        }
    }

    public void TossWeapon(Vector3 _dir)
    {
        StartCoroutine(toss(_dir));
    }

    IEnumerator crtMeleeStunned()
    {
        yield return new WaitForSeconds(1.5f); 
        myAnimator.SetBool("Stunned", false);
        AIState = lastState;
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
     
    public virtual void DefeatedState()
    {
        // Nothing for now
    }

    public virtual void OnMelee(int _damage)
    {
        OnHit(_damage);
        lastState = AIState;
        AIState = EnemyAIState.Stunned;
        if(MyUnit.CurrentHealth > 0)
        {
            StartCoroutine(crtMeleeStunned()); 
            myAnimator.SetBool("Stunned", true);
        }

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
            CorpseScript cs = GetComponentInChildren<CorpseScript>(true);
            if(cs != null)
            {
                cs.DropCorpse(transform.position, animationHandler.flipSprites);
            }else
            { 
                GameObject obj = Resources.Load("Prefabs/Particles/deathPartParent") as GameObject;
                Instantiate(obj, transform.position, obj.transform.rotation); 
            }


            if (currentWeapon != null)
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
            currentWeapon = WeaponSlot1;
            return;
        }

        if(WeaponSlot1 != null && WeaponSlot2 == null)
        {
            WeaponSlot2 = _newWeapon;  
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
            }

            if (currentWeapon == WeaponSlot1)
            {
                WeaponSlot1 = _newWeapon;
                currentWeapon = WeaponSlot1; 
            }

        }
       

    }
 
    public void SwapWeapons()
    {
        currentWeapon = (currentWeapon == WeaponSlot1) ? WeaponSlot2 : WeaponSlot1;
    }

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


    public void RemoveWeapon(WeaponBase _Weapon)
    {
        if (_Weapon == WeaponSlot1)
        { WeaponSlot1 = null; currentWeapon = WeaponSlot2; } 

        if (_Weapon == WeaponSlot2)
        { WeaponSlot2 = null; currentWeapon = WeaponSlot1; }  

        if(myWeapon != null)
            myWeapon.ResetShootCD();
         
    }

    public void MoveTo(Vector3 _destination)
    {
        moveAI.MoveTo(_destination);
    }

    public void Look(Vector3 _look)
    { 
        Vector3 lookTo = GlobalConstants.ZeroYComponent(_look - transform.position); 
        animationHandler.LookingVector = lookTo;
    }

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(gameObject.activeInHierarchy);

		data.WriteLine(transform.position);

		data.WriteLine(MyUnit.CurrentHealth);

		if(WeaponSlot1 != null)
		{
			if(WeaponSlot1.name.IndexOf(' ') != -1)
			{
				data.WriteLine(WeaponSlot1.name.Substring(0, WeaponSlot1.name.IndexOf(' ')));
			}
			else
			{
				data.WriteLine(WeaponSlot1.name);
			}
		}
		else
		{
			data.WriteLine("null");
		}
		
		if(WeaponSlot2 != null)
		{
			if(WeaponSlot2.name.IndexOf(' ') != -1)
			{
				data.WriteLine(WeaponSlot2.name.Substring(0, WeaponSlot2.name.IndexOf(' ')));
			}
			else
			{
				data.WriteLine(WeaponSlot2.name);
			}
		}
		else
		{
			data.WriteLine("null");
		}
		
		if(currentWeapon != null)
		{
			if(currentWeapon.name.IndexOf(' ') != -1)
			{
				data.WriteLine(currentWeapon.name.Substring(0, currentWeapon.name.IndexOf(' ')));
			}
			else
			{
				data.WriteLine(currentWeapon.name);
			}
		}
		else
		{
			data.WriteLine("null");
		}
		
		return data.ToString();
	}

	public void Load(string[] data)
	{
		//Debug.Log(gameObject.name);
		gameObject.SetActive(bool.Parse(data[0].Trim()));

		// Parse transform.position!
		transform.position = GlobalConstants.StringToVector3(data[1]);
		
		MyUnit.CurrentHealth = int.Parse(data[2].Trim());
		
		if(WeaponSlot1 != null)
		{
			Destroy(WeaponSlot1.gameObject);
			WeaponSlot1 = null;
		}
		if(data[3] != "null")
		{
			WeaponSlot1 = (Instantiate(Resources.Load("Prefabs/Weapon/" + data[3]), transform) as GameObject).GetComponent<WeaponBase>();
			WeaponSlot1.gameObject.name = data[3];
		}
		
		if(WeaponSlot2 != null)
		{
			Destroy(WeaponSlot2.gameObject);
			WeaponSlot2 = null;
		}
		if(data[4] != "null")
		{
			WeaponSlot2 = (Instantiate(Resources.Load("Prefabs/Weapon/" + data[4]), transform) as GameObject).GetComponent<WeaponBase>();
			WeaponSlot2.gameObject.name = data[4];
		}
		
		if(data[5] != "null")
		{
			if(WeaponSlot1 != null && data[5] == WeaponSlot1.name)
			{
				currentWeapon = WeaponSlot1;
			}
			else
			{
				currentWeapon = WeaponSlot2;
			}
		}
	}

    public void SetAggro(bool _b)
    {
        AIState = (_b) ? EnemyAIState.Aggro : EnemyAIState.Idle;

    }

    public CController cc
    {
        get { return myCC; }
    }
}
