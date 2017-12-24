using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(CController))]
public class EnemyT1  {
     
    public MovementAI moveAI;

    public enum EnemyT1State { Idle, Vulnerable, Aggro, Defeated };
    public EnemyT1State curState; 

    GameObject myObject;
    GameObject equippedWeaponObj;
    IWeapon equippedWeapon;
     
    Animator myAnimator;
    SpriteRenderer myRenderer;

    float curAggroTime;
    public float AggroRange = 5;
    public float AggroTime = 2;

    PlayerScript player;
    CController myCC;
    EffectSystem myEmojis;

    public Vector3 LookingVector;
    Vector3 LastSeen;

    Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
    Vector3 HolsteredRotation = new Vector3(0, 0, 150);
    Vector3 RootPosition = new Vector3(-0.138f, -0.138f, 0);

    // Use this for initialization
    public EnemyT1(GameObject _obj, IArmed _owner, CController _mycc, GameObject _WeaponObj)
    {
        myObject = _obj;
        equippedWeaponObj = _WeaponObj;
        equippedWeapon = _WeaponObj.GetComponentInChildren<IWeapon>();
        equippedWeapon.Owner = _owner; 
        myAnimator = _obj.GetComponent<Animator>();
        myRenderer = _obj.GetComponent<SpriteRenderer>();
        myEmojis = _obj.GetComponentInChildren<EffectSystem>();
        myCC = _mycc;
        moveAI = new MovementAI(_obj, myCC);
    }

    // Update is called once per frame
    public void Update()
    {
        switch(curState)
        {
            case EnemyT1State.Idle:
                stateIdle();
                break;
            case EnemyT1State.Vulnerable:
                stateVuln();
                break;
            case EnemyT1State.Aggro:
                stateAggro();
                break;
        }

        Animations();
        moveAI.Update();

    }

    //public void OnHit(int _damage)
    //{
    //    UnitData.CurrentHealth -= _damage;
    //    myVisualizer.ShowMenu();

    //    // SHIT I was hit at a crucial moment
    //    if((curState == EnemyT1State.Arming || curState == EnemyT1State.Idle) && Clumsy)
    //    {
    //        // Woops you tossed your weapon onto the ground
    //        Disarmed();
    //    }

    //    if(UnitData.CurrentHealth <= 0)
    //    {
    //        this.gameObject.SetActive(false);
    //    }
    //}


    // Check if we can aggro onto the player
    bool CheckAggro()
    {
 
        Vector3 distVec = playerRef.transform.position - myObject.transform.position;
        Ray r = new Ray(myObject.transform.position, distVec.normalized);

        if (distVec.magnitude <= AggroRange)
            return !Physics.Raycast(r, distVec.magnitude, LayerMask.GetMask("Ground"));
        else
            return false;
    }

    public virtual void stateIdle()
    {
        // The first thing we do is to check to see if we actually get out of being idle
        if (CheckAggro())
        {
            curAggroTime = 0;
            curState = EnemyT1State.Vulnerable;

            if (myEmojis != null)
                myEmojis.Fire(EffectSystem.EffectType.Alert, myObject.transform.position + (Vector3.up * 4));
            return;
        }

        equippedWeaponObj.transform.localPosition = HolsteredPosition;
        equippedWeaponObj.transform.rotation = Quaternion.Euler(HolsteredRotation);
        
         

    }

    // The arming method, this is happening when the enemy is 'reaching for his weapon'
    // in reality it's just a more complicated idle method
    public virtual void stateVuln()
    { 
        curAggroTime += Time.deltaTime;

        if (curAggroTime + 1.55f >= AggroTime)
            myAnimator.SetTrigger("Arming");

        if (curAggroTime >= AggroTime)
        {
            curState = EnemyT1State.Aggro;
        }
    }


    // The update method called when we're aggrod
    public virtual void stateAggro()
    {
        // First get some references
        Vector3 playerPos = playerRef.transform.position;
        Vector3 distVec = playerRef.transform.position - myObject.transform.position;


        // Make the weapon look at the player
        equippedWeaponObj.GetComponentInChildren<SpriteRenderer>().flipY = (LookingVector.x < myObject.transform.position.x);

        Vector3 pos = RootPosition;
        if (LookingVector.z < myObject.transform.position.z)
            pos.z = -.01f;
        else
            pos.z = .01f;

        // Change the position and the rotation of the weapon
        equippedWeaponObj.transform.localPosition = pos; 
        equippedWeaponObj.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(LookingVector.normalized));

        // If we haven't started pathing somewhere
        if(moveAI.ActionComplete)
        {
            float chances = .5f;
            if (moveAI.lastType == MovementAI.MovementAIType.Wander)
                chances = .1f;

            if (moveAI.lastType == MovementAI.MovementAIType.Wait)
                chances = .9f;
             
            // Flip that weighted coin
            float val = UnityEngine.Random.value;
            bool rnd = (val < chances);
            if (rnd)
                moveAI.Wander(1, myCC.Speed);
            else
                moveAI.Wait(2); 
                
        }

        if(moveAI.curType == MovementAI.MovementAIType.Wait)
        {
            // Raycast to the player
            if(SeePlayer())
            {
                equippedWeapon.FireWeapon(distVec);
            }
        }

    }

    public virtual void Disarmed()
    { 
        curState = EnemyT1State.Defeated;

        // Disable the weapon you currently have
        equippedWeaponObj.SetActive(false);
        equippedWeapon = null; 
    }
 
    private void OnDrawGizmosSelected()
    { 
        // Draw aggro lines
        Gizmos.color = Color.red;
        Vector3 distVec = playerRef.transform.position - myObject.transform.position;
        Vector3 endPoint = myObject.transform.position + (distVec.normalized * AggroRange);
        Gizmos.DrawLine(myObject.transform.position, endPoint);

        Color c = Color.yellow;
        c.a = .2f;
        Gizmos.color = c;
        Gizmos.DrawLine(myObject.transform.position, playerRef.transform.position); 
    }

    public virtual void Animations()
    {
        bool faceFront = true;
        bool flipSprites = false;
        Vector3 weaponPos = Vector3.zero;

        myAnimator.SetInteger("State", (int)curState);

        switch(curState)
        {
            case EnemyT1State.Idle:
                // looking vector is looking vector
                break;
            case EnemyT1State.Vulnerable:
                // looking vector is towards the player
                if (SeePlayer())
                {
                    LookingVector = playerRef.transform.position - myObject.transform.position;
                    LastSeen = playerRef.transform.position;
                }
                else
                    LookingVector = LastSeen - myObject.transform.position;
                LookingVector.z = -1;
                break;
            case EnemyT1State.Aggro:
                if (SeePlayer())
                {
                    LookingVector = playerRef.transform.position - myObject.transform.position;
                    LastSeen = playerRef.transform.position;
                }
                else
                    LookingVector = LastSeen - myObject.transform.position;

                break; 
        }

        faceFront = LookingVector.z <= 0;
        flipSprites = LookingVector.x <= 0;

        myRenderer.flipX = flipSprites;
        equippedWeaponObj.GetComponentInChildren<SpriteRenderer>().flipY = flipSprites;

        // Move the weapon in front or behind this unit
        weaponPos = RootPosition;


        if(curState == EnemyT1State.Aggro) 
            // We want the gun in front
            weaponPos.z = (faceFront) ? -.01f : .01f; 
        else
            // We want the gun in back
            weaponPos.z = (faceFront) ? .01f : -.01f;

 

        // Change the position and the rotation of the weapon
        equippedWeaponObj.transform.localPosition = weaponPos;


        myAnimator.SetBool("FaceFront", faceFront);
    }

    bool SeePlayer()
    {
        Vector3 distVec = player.transform.position - myObject.transform.position;
        return !Physics.Raycast(myObject.transform.position, distVec, distVec.magnitude, LayerMask.GetMask("Ground"));
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



}
