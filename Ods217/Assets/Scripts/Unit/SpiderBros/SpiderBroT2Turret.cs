using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A nerfed version of the Spider Bro T2 
/// This spider is a much stupider version of the normal T2 spider, only following a set movement
/// </summary>
public class SpiderBroT2Turret : MonoBehaviour, IArmed { 

    public bool triggered;
    public Vector3 StartPos;
    public Vector3 EndPos;

    ZoneScript z;


    [Header("Spider Data")]

    public UnitStruct UnitData = new UnitStruct("Spider Bro V2", 40, 40, 25, 25);

    public int Damage = 5;

    public bool attacking = false;
    public float PeekWeight = 0;
    float AITimeout = 0;
    float curWaitCount = 1;

    public WeaponBase MyWeapon;
     
    GameObject Player;
    CController myCC; 
    EnergyManager myManager;

    Animator ChasisAnim;
    Animator HeadAnim;
    AudioSource ChargeLaserSource;

      
    public bool DEBUG = false;

    const float PATROLSPEED = .3F; 

    public AudioClip WeaponChargeClip;

    ParticleSystem.EmissionModule SuperPowerupSpherePartEmission;

    bool FireAtWill = false;

    // Use this for initialization
    void Start()
    {
        myCC = GetComponent<CController>(); 

        myManager = GetComponent<EnergyManager>();

        Transform g = transform.Find("Head");
        if (g != null)
        {
            HeadAnim = g.GetComponent<Animator>();
        }

        Transform s = transform.Find("ChargeSource");
        if (s != null)
        {
            ChargeLaserSource = s.GetComponent<AudioSource>();
        }

        ParticleSystem[] syss = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in syss)
        {
            if (system.gameObject.name == "SuperPowerupSphere")
            {
                SuperPowerupSpherePartEmission = system.emission;
            }
        }

        ChasisAnim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {  
        if (z != ZoneScript.ActiveZone)
            return;

        Persue();
    }

    private void Update()
    {
        float lookX = myCC.Velocity.x;
        float lookY = myCC.Velocity.z;
        bool isMoving = GlobalConstants.ZeroYComponent(myCC.Velocity).magnitude > .1f;

        Vector3 distToPlayer = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

        // Handle the head animator
        if (HeadAnim != null)
        { 
            HeadAnim.SetBool("Aggro", true);
             
            lookX = distToPlayer.x;
            lookY = distToPlayer.z; 

            HeadAnim.SetFloat("LookX", lookX);
            HeadAnim.SetFloat("LookY", lookY);
        }

        if (ChasisAnim != null)
        {
            ChasisAnim.SetBool("Moving", isMoving);
        }

        MyWeapon.RotateObject.transform.localRotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(distToPlayer.normalized));
    }


    void Persue()
    { 

        Vector3 distanceVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);

        if (FireAtWill)
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

        }
        else
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


        Vector3 _move; 
        _move = ((triggered) ? EndPos : StartPos) - transform.position;
        _move.y = 0;

        if(_move.magnitude > 1)
        { 
            _move = _move.normalized;
            myCC.ApplyForce(_move);
        }

    }

    IEnumerator ChargeWeapon()
    {
        if (ChargeLaserSource != null)
            ChargeLaserSource.Play();

        SuperPowerupSpherePartEmission.enabled = true;

        yield return new WaitForSeconds(2.3f);
        FireAtWill = false;

        SuperPowerupSpherePartEmission.enabled = false;

    }

    void StartYelling()
    {
        Camera.main.GetComponent<CamScript>().AddEffect(CamScript.CamEffect.Shake, 3);
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
            return triggered;
        }

        set
        {
            triggered = value;
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

 
}
