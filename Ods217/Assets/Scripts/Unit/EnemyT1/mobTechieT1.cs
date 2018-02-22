using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mobTechieT1 : AIStandardUnit {

    bool vuln = false;
    float curTime;
    LineRenderer line;

    UsableIndicator ind_Usable; 

    [Space(20)]
    [Header("mobCowardUnit")]
    int segments = 20; 

    AIStandardUnit.EnemyAIState prevState;
    public AudioClip[] Clips;
    AudioSource audioSrc;

    public GameObject DronePrefab;
    public mobDroneT1 DroneReference;
    CharacterController myCharacterController;

    public override void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = (segments + 1);
        line.useWorldSpace = false;
         
        audioSrc = GetComponent<AudioSource>();
        myCharacterController = GetComponent<CharacterController>();
        base.Start();
    }

    public override void Update()
    { 
        if (myZone != ZoneScript.ActiveZone && AIState == EnemyAIState.Defeated)
            this.gameObject.SetActive(false);

        // draw a circle on the ground
        if (vuln)
        {
            curTime += Time.deltaTime;
            if (curTime > base.AggroTime)
                curTime = base.AggroTime;

            // The circles size is based on how long it'll take to 
            CreatePoints();
        }

        vuln = (base.AIState == EnemyAIState.Vulnerable); 
        line.enabled = vuln; 

        if (base.AIState == EnemyAIState.Vulnerable && prevState != base.AIState && audioSrc != null && Clips.Length > 0)
        {
            audioSrc.clip = Clips[0];
            audioSrc.Play();
        }


        // On going into aggro mode release the drone n have fun
        if(base.AIState != prevState && base.AIState == EnemyAIState.Aggro)
        {
            DroneReference = Instantiate(DronePrefab, transform.position,Quaternion.identity).GetComponent<mobDroneT1>();
            DroneReference.Activated = true;
            DroneReference.myZone = ZoneScript.ActiveZone;
            DroneReference.GetComponent<Rigidbody>().velocity = GlobalConstants.ZeroYComponent( playerRef.transform.position - transform.position);

            Physics.IgnoreCollision(DroneReference.GetComponent<Collider>(), GetComponent<Collider>());
        }


         
        prevState = base.AIState;
        base.Update();
    }

    private void FixedUpdate()
    {
        if (DroneReference != null)
        {
            if (DroneReference.Activated && DroneReference.UnitData.CurrentHealth > 0)
            {
                DroneReference.UpdateDrone();
            }

            if(DroneReference.UnitData.CurrentHealth <= 0)
            {
                myAnimator.SetBool("Disarmed", true);
                base.AIState = EnemyAIState.Defeated;
            }
        }
    }



    public override void OnHit(int _damage)
    {
        if (audioSrc != null && Clips.Length > 0)
        {
            audioSrc.clip = Clips[1];
            audioSrc.Play();
        }
        base.OnHit(_damage);
    }

    void CreatePoints()
    {
        float x = 0f;
        float ccheight = GetComponent<CharacterController>().height;
        float y = -(ccheight / 2) + .1f;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * (base.AggroTime - curTime + .5f);
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * (base.AggroTime - curTime + .5f);

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }

 
 
 
    public override void AggroState()
    {
        // Do nothing for now
    }

    public override void DefeatedState()
    {
        //// Run away from the player  
        //Vector3 moveVec = GlobalConstants.ZeroYComponent(transform.position - playerRef.transform.position);
         
        //myCC.ApplyForce(moveVec.normalized); 

        // Stand still because of bug

    }

    public override WeaponBase myWeapon
    {
        get
        {
            // This mob does not have a weapon, instead he uses drones
            return null;
        }
    }

}
