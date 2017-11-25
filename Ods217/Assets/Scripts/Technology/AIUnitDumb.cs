using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CController))]
public class AIUnitDumb : MonoBehaviour, IArmed{


    public UnitStruct UnitData;

    public enum EnemyT1State { Idle, Arming, Aggro, Defeated };
    public EnemyT1State curState;
    public bool Clumsy;

    [Space(40)]
    public GameObject equippedWeaponObj;
    public GameObject tossedWeaponPrefab; // The weapon we'll initialize if this unit is diarmed
    IWeapon equippedWeapon;

    EffectSystem myEmojis;
    Animator myAnimator;
    SpriteRenderer myRenderer;

    public bool Aggrod; // Are we aggroed
    [Range(1, 50)]
    public float AggroRange = 5; // How far away can we aggro?
    [Range(0, 5)]
    public float AggroTime = 2; // How long does it take to aggro
    float curAggroTime; // And how long has it been since we've started aggroing

    PlayerScript player;
    CController myCC;


    public Vector3 LookingVector;
    Vector3 LastSeen;

    Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
    Vector3 HolsteredRotation = new Vector3(0, 0, 150);
    Vector3 RootPosition = new Vector3(-0.138f, -0.138f, 0);

    public ZoneScript Zone;

    // Use this for initialization
    void Start()
    {
        equippedWeapon = equippedWeaponObj.GetComponentInChildren<IWeapon>();
        equippedWeapon.Owner = this;
        myEmojis = GetComponentInChildren<EffectSystem>();
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myCC = GetComponent<CController>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (curState)
        {
            case EnemyT1State.Idle:
                stateIdle();
                break;
            case EnemyT1State.Arming:
                stateArming();
                break;
            case EnemyT1State.Aggro:
                stateAggro();
                break;
        }

        Animations();

    }

    public virtual void OnHit(int _damage)
    {
        UnitData.CurrentHealth -= _damage;
        myVisualizer.ShowMenu();

        // SHIT I was hit at a crucial moment
        if ((curState == EnemyT1State.Arming || curState == EnemyT1State.Idle) && Clumsy)
        {
            // Woops you tossed your weapon onto the ground
            Disarmed();
        }

        if (UnitData.CurrentHealth <= 0)
        {
            this.gameObject.SetActive(false);
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

    public virtual void stateIdle()
    {
        // The first thing we do is to check to see if we actually get out of being idle
        if (CheckAggro())
        {
            curAggroTime = 0;
            curState = EnemyT1State.Arming;

            if (myEmojis != null)
                myEmojis.Fire(EffectSystem.EffectType.Alert, transform.position + (Vector3.up * 4));
            return;
        }

        equippedWeaponObj.transform.localPosition = HolsteredPosition;
        equippedWeaponObj.transform.rotation = Quaternion.Euler(HolsteredRotation);



    }

    // The arming method, this is happening when the enemy is 'reaching for his weapon'
    // in reality it's just a more complicated idle method
    public virtual void stateArming()
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
        Vector3 distVec = playerRef.transform.position - transform.position;


        // Make the weapon look at the player
        equippedWeaponObj.GetComponentInChildren<SpriteRenderer>().flipY = (playerPos.x < transform.position.x);

        Vector3 pos = RootPosition;
        if (playerPos.z < transform.position.z)
            pos.z = -.01f;
        else
            pos.z = .01f;

        // Change the position and the rotation of the weapon
        equippedWeaponObj.transform.localPosition = pos;
        equippedWeaponObj.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(distVec.normalized));



        // Stay in range of the player (better AI would have keep a sweet spot, not too close not too far)
        if (distVec.magnitude > 10)
        {
            // Raycast to the player
            myCC.ApplyForce(distVec.normalized * myCC.Speed);
        }
        else
        {
            equippedWeapon.FireWeapon(distVec);
        }
    }

    public virtual void Disarmed()
    {
        if (myEmojis != null)
            myEmojis.Fire(EffectSystem.EffectType.Panic, transform.position + (Vector3.up * 4));

        curState = EnemyT1State.Defeated;

        // Disable the weapon you currently have
        equippedWeaponObj.SetActive(false);
        equippedWeapon = null;

        // Throw the actual object
        GameObject obj = (GameObject)Instantiate(tossedWeaponPrefab, transform.position, Quaternion.identity);
        usableWeapon uw = obj.GetComponent<usableWeapon>();
        uw.Toss(Vector3.forward * 3, transform.position);


    }

    private void OnDrawGizmosSelected()
    {
        // Draw aggro lines
        Gizmos.color = Color.red;
        Vector3 distVec = playerRef.transform.position - transform.position;
        Vector3 endPoint = transform.position + (distVec.normalized * AggroRange);
        Gizmos.DrawLine(transform.position, endPoint);

        Color c = Color.yellow;
        c.a = .2f;
        Gizmos.color = c;
        Gizmos.DrawLine(transform.position, playerRef.transform.position);
    }

    public virtual void Animations()
    {
        bool faceFront = true;
        bool flipSprites = false;
        Vector3 weaponPos = Vector3.zero;

        myAnimator.SetInteger("State", (int)curState);

        switch (curState)
        {
            case EnemyT1State.Idle:
                // looking vector is looking vector
                break;
            case EnemyT1State.Arming:
                // looking vector is towards the player
                LookingVector = playerRef.transform.position - transform.position;
                LookingVector.z = -1;
                break;
            case EnemyT1State.Aggro:
                LookingVector = playerRef.transform.position - transform.position;
                break;
        }

        faceFront = LookingVector.z <= 0;
        flipSprites = LookingVector.x <= 0;

        myRenderer.flipX = flipSprites;
        equippedWeaponObj.GetComponentInChildren<SpriteRenderer>().flipY = flipSprites;

        // Move the weapon in front or behind this unit
        weaponPos = RootPosition;


        if (curState == EnemyT1State.Aggro)
            // We want the gun in front
            weaponPos.z = (faceFront) ? -.01f : .01f;
        else
            // We want the gun in back
            weaponPos.z = (faceFront) ? .01f : -.01f;



        // Change the position and the rotation of the weapon
        equippedWeaponObj.transform.localPosition = weaponPos;


        myAnimator.SetBool("FaceFront", faceFront);
    }


    /// References ------------------------------------------------------------------------------------------------------------------------
    /// 

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

    public HealthBar myVisualizer
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
            return equippedWeapon;
        }
    }

    public bool Triggered
    {
        get
        {
            return Aggrod;
        }

        set
        {
            Aggrod = value;
        }
    }

    public void Activate()
    {

    }


    public virtual void TossWeapon()
    {

    }

    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
