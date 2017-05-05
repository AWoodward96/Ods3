using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unit Script
/// The primary script for the first type of enemy in the game:
/// The Jumping Spider
/// </summary>
[RequireComponent(typeof(CController))]
public class JumpingSpider : MonoBehaviour, IUnit
{

    // External references
    public UnitStruct myUnit;
    public JumpingSpiderAI AiStats;
    public Weapon myWeapon;
    ZoneScript myZone;
    Animator myAnimator;
    CController myCtrl;
    Light myLight;

    Color redLight = new Color(209f / 255f, 0, 0, 1);
    Color greenLight = new Color(6f / 255f, 209f / 255f, 0, 1);

    // Do we want this enemy to automatically aggro on the player
    public bool aggressive;
    public bool hiveMinded;

    enum AISTATE { Idle, Aggro, Shimmy, Prep, Jump, Dead };
    [SerializeField]
    AISTATE AIState;


    // coroutine bools --- Could be an array but that would be confusing 
    bool crt_Aggro = false;
    bool crt_Shimmy = false;
    bool crt_Prep = false;
    bool crt_Jump = false;

    List<GameObject> JumpObjectHit;

    public List<AudioClip> IdleSounds;
    public List<AudioClip> AggroSounds;
    public List<AudioClip> WindupSounds;
    public List<AudioClip> DeathSounds;
    AudioSource mySource;
    float sfxCD;
    float randomSFXCD;

    // Use this for initialization
    void Start()
    {
        myCtrl = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myLight = GetComponentInChildren<Light>();
        mySource = gameObject.AddComponent<AudioSource>();
        mySource.loop = false;
        mySource.playOnAwake = false;
        mySource.spatialBlend = 1;
        mySource.volume = .8f;

        JumpObjectHit = new List<GameObject>();

        AIState = AISTATE.Idle;

        randomSFXCD = UnityEngine.Random.Range(0, 4);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        handleSFX();
        updateVisuals();

        // Do something different for each AI
        switch (AIState)
        {
            case AISTATE.Idle: // Do nothing I guess? If at any point you can actually see the player aggro
                AIIdle();
                break;
            case AISTATE.Aggro: // Send up an ! mark and depending on the reaction time
                if (!crt_Aggro)
                {
                    SPFXManager.showSPFX(SPFXManager.SPFX.Exclamation, (transform.position + Vector3.up) + (Vector3.forward * .1f), new Vector3(0, 600, 0), 1.5f);
                    crt_Aggro = true;
                    sfxCD = randomSFXCD;
                    StartCoroutine(Aggro());
                }
                break;
            case AISTATE.Shimmy: // Ideally, move towards the player, and shimmy back and forth its a grand ol time
                AIShimmy();
                break;
            case AISTATE.Prep: // Get ready to jump
                if (!crt_Prep)
                {
                    crt_Prep = true;
                    sfxCD = randomSFXCD;
                    StartCoroutine(Prep());
                }
                break;
            case AISTATE.Jump: // Go Go Go!
                if (!crt_Jump)
                {
                    JumpObjectHit = new List<GameObject>();
                    crt_Jump = true;
                    Jump();
                    StartCoroutine(JumpCRT());
                }
                break;
            case AISTATE.Dead:
                StopCoroutine(JumpCRT());
                StopCoroutine(Prep());
                StopCoroutine(Aggro());
                crt_Jump = true;
                crt_Prep = true;
                crt_Aggro = true;
                break;
        }
    }

    void updateVisuals()
    {
        // Update the anim stats
        bool moving = GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f;
        bool frontvalue = myAnimator.GetBool("Front");
        myAnimator.SetBool("Moving", moving);
        myAnimator.SetBool("Jumping", crt_Jump && GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 2f);
        myAnimator.SetBool("Prep", (AIState == AISTATE.Prep));
        if (moving)
            myAnimator.SetBool("Front", myCtrl.Velocity.z < 1);

        // update the light
        float yValue = (AIState == AISTATE.Shimmy) ? -.7f : -.4f;
        yValue = (AIState == AISTATE.Prep) ? -.85f : yValue;

        float zValue = myAnimator.GetBool("Front") ? -0.1f : 0.1f;
        zValue = (AIState == AISTATE.Idle) ? -0.1f : zValue;
        myLight.color = (AIState == AISTATE.Idle || AIState == AISTATE.Aggro) ? greenLight : redLight;

        if (AIState == AISTATE.Dead)
            myLight.transform.localPosition = new Vector3(.5f, -.8f, -.1f);
        else
            myLight.transform.localPosition = new Vector3(0, yValue, zValue);


    }

    public ZoneScript MyZone
    {
        get { return myZone; }
        set { myZone = value; }
    }

    public HealthVisualizer myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthVisualizer>();
        }
    }

    void AIIdle()
    {
        if (ZoneScript.ActiveZone == myZone) // You can't aggro if the player isn't even in the zone
        {
            Collider[] potentialTargets = Physics.OverlapSphere(transform.position, 12);
            foreach (Collider c in potentialTargets)
            {
                if (c.gameObject.layer == LayerMask.NameToLayer("Units")) // Any units are targets for spiders
                {
                    if (c.name.ToUpper().Contains("SPIDER"))
                        continue;

                    // You also have to have vision of the target because if you don't then... well we don't want them to aggro through walls  
                    Vector3 myZeroPosition = GlobalConstants.ZeroYComponent(transform.position);
                    Vector3 targetZeroPosition = GlobalConstants.ZeroYComponent(c.transform.position);
                    // Raycast to see if we can jump at the target
                    Vector3 distVector = c.transform.position - transform.position;
                    Ray r = new Ray(transform.position, distVector);
                    if (!Physics.Raycast(r, distVector.magnitude, LayerMask.GetMask("Ground"))) // If we didn't hit a wall on our raycast
                    {
                        AiStats.Target = c.gameObject;
                        AIState = AISTATE.Aggro;
                    }
                    return;
                }
            }
        }
    }



    IEnumerator Aggro()
    {
        yield return new WaitForSeconds(AiStats.ReactionSpeed);
        if (AIState == AISTATE.Dead)
            yield break;

        AIState = AISTATE.Shimmy;
        crt_Aggro = false; // normally we wouldn't care about this. but just to be safe
    }

    IEnumerator Shimmy()
    {
        yield return new WaitForSeconds(1);
        if (AIState == AISTATE.Dead)
            yield break;
        float rnd = UnityEngine.Random.Range(0f, 1f);
        if (rnd >= AiStats.PrepThreshold)
            AIState = AISTATE.Prep;

        crt_Shimmy = false;
    }

    IEnumerator Prep()
    {
        yield return new WaitForSeconds(AiStats.PrepSpeed);
        if (AIState == AISTATE.Dead)
            yield break;
        AIState = AISTATE.Jump;
        crt_Prep = false;
    }

    IEnumerator JumpCRT()
    {
        yield return new WaitForSeconds(1);
        if (AIState == AISTATE.Dead)
            yield break;
        crt_Jump = false;
        AIState = AISTATE.Shimmy;
    }

    void AIShimmy()
    {
        // move towards the player  (slowly)
        // Also move left and right based on the shimmy shimmy
        Vector3 TargetPos = AiStats.Target.transform.position;
        Vector3 distToPlayer = TargetPos - transform.position;
        Vector3 y0_unitDistToPlayer = GlobalConstants.ZeroYComponent(distToPlayer).normalized;

        myCtrl.ApplyForce(y0_unitDistToPlayer.normalized * myCtrl.Speed / 3);

        // apply the shimmy vector force to move the spider back and forth
        Vector3 ShimmyVector = new Vector3(-y0_unitDistToPlayer.z, 0, y0_unitDistToPlayer.x);
        ShimmyVector = ShimmyVector.normalized;
        AiStats.shimmyValue = 0 + (Mathf.Sin(Time.time)) / 2;
        myCtrl.ApplyForce(ShimmyVector * AiStats.shimmyValue);

        // Start the coroutine to start hoppin
        if (!crt_Shimmy)
        {
            crt_Shimmy = true;
            StartCoroutine(Shimmy());
        }

    }

    void Jump()
    {
        //PHYSICS PHYSICS PHYSICS PHYSICS
        float currentGravity = GlobalConstants.Gravity;
        float angle = 70 * Mathf.Deg2Rad; // To radian because Unity's sin and cos functions randomly take radians. like why?


        Vector3 targetPos = AiStats.Target.transform.position;
        //float angle = Vector3.Angle(GlobalConstants.ZeroYComponent(targetPos - transform.position), targetPos - transform.position)  * Mathf.Deg2Rad; 
        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);
        if ((planarTarget - planarPosition).magnitude < 3)
            planarTarget += (planarTarget - planarPosition).normalized; // We want to move past the player

        float distance = Mathf.Max(Vector3.Distance(planarTarget, planarPosition), 1);
        float yOffset = transform.position.y - targetPos.y;

        //PHYSICS PHYSICS PHYSICS PHYSICS
        float initialVel = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * currentGravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
        Vector3 vel = new Vector3(0, initialVel * Mathf.Sin(angle), initialVel * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (targetPos.x < transform.position.x ? -1 : 1);
        Vector3 finalVel = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * vel;

        myCtrl.ApplyForce(finalVel * 7);
        // Ok so this finally apply force isn't physics, it's making my CC script work
        // If I don't multiply the finalVel, the spider will just make a pitaful hop towards the player and barely even get off the ground
        // As long as we cap the spiders max velocity at a high enough number, this should work just fine for what we want to do

    }


    public void OnDeath()
    {
        StopCoroutine(DeathCoroutine());
        StartCoroutine(DeathCoroutine());
        AIState = AISTATE.Dead;
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
        // Hello future me stop reading these comments and get back to work (4/24/17)
        if (myUnit.CurrentHealth > 0)
        {
            myVisualizer.ShowMenu();
            myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
            if (myUnit.CurrentHealth <= 0)
            {
                OnDeath();
            }

            GameObject Target = _FromWhatWeapon.Owner.gameObject;
            if (AIState == AISTATE.Idle)
            {
                // Raycast to the player and see if you can see him if so aggro on him 
                Vector3 distVector = Target.transform.position - transform.position;
                Ray r = new Ray(transform.position, distVector);
                if (!Physics.Raycast(r, distVector.magnitude, LayerMask.GetMask("Ground"))) // If we didn't hit a wall on our raycast
                {
                    // Well lets attack him!
                    AiStats.Target = Target;
                    AIState = AISTATE.Aggro;

                }
            }


            if (hiveMinded)
            {
                // Aggro the rest of the spiders in the zone 
                foreach (GameObject enemy in ZoneScript.ActiveZone.Enemies)
                {
                    JumpingSpider thisscript = enemy.GetComponent<JumpingSpider>();
                    if (thisscript != null && thisscript != this && thisscript.AIState == AISTATE.Idle)
                    {
                        thisscript.AiStats.Target = Target;
                        thisscript.AIState = AISTATE.Aggro;
                    }
                }
            }


        }
    }

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    public Weapon MyWeapon
    {
        get { return myWeapon; }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (AIState == AISTATE.Jump && myUnit.CurrentHealth > 0) // If we're jumping and alive
        {
            if (!JumpObjectHit.Contains(hit.gameObject) && hit.gameObject.GetComponent<IUnit>() != null) // If the list of things we've hit during this jump doesn't include what we just hit and it has an IUnit
            {
                // Well check to make sure it aint a spider
                if (!hit.gameObject.name.Contains("Spider")) // Rudimentary but it works
                {
                    IUnit u = hit.gameObject.GetComponent<IUnit>();
                    JumpObjectHit.Add(hit.gameObject);
                    u.OnHit(myWeapon);
                }
            }
        }
    }

    void handleSFX()
    {
        if (AggroSounds.Count == 0 || IdleSounds.Count == 0 || WindupSounds.Count == 0 || DeathSounds.Count == 0)
            return;


        sfxCD += Time.deltaTime;
        if (sfxCD > randomSFXCD)
        {
            sfxCD = 0;
            randomSFXCD = UnityEngine.Random.Range(0f, 4f);
            int random = 0;
            mySource.Stop();
            switch (AIState)
            {
                case AISTATE.Aggro:
                    random = UnityEngine.Random.Range(0, AggroSounds.Count - 1);
                    mySource.clip = AggroSounds[random];
                    break;
                case AISTATE.Idle:
                    random = UnityEngine.Random.Range(0, IdleSounds.Count - 1);
                    mySource.clip = IdleSounds[random];
                    break;
                case AISTATE.Shimmy:
                    random = UnityEngine.Random.Range(0, IdleSounds.Count - 1);
                    mySource.clip = IdleSounds[random];
                    break;
                case AISTATE.Prep:
                    random = UnityEngine.Random.Range(0, WindupSounds.Count - 1);
                    mySource.clip = WindupSounds[random];
                    break;
                case AISTATE.Dead:
                    random = UnityEngine.Random.Range(0, DeathSounds.Count - 1);
                    mySource.clip = DeathSounds[random];
                    break;
            }
            mySource.Play();
        }
    }
}

[Serializable]
public struct JumpingSpiderAI
{

    [Range(.1f, 2)]
    public float ReactionSpeed;
    [Range(.1f, 2)]
    public float PrepSpeed;
    [Range(.1f, 1f)]
    public float PrepThreshold;

    public GameObject Target;

    //[HideInInspector] 
    public float shimmyValue;
}
