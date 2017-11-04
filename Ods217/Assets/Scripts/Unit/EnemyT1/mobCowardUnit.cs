using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(UsableIndicator))]
[RequireComponent(typeof(AudioSource))]
public class mobCowardUnit : AIStandardUnit {

    bool vuln = false;
    float curTime;
    LineRenderer line; 

    UsableIndicator ind_Usable;
    PlayerScript playerref;

    [Space(30)]
    public int segments;
    public GameObject DisarmedWeapon;

    AIStandardUnit.EnemyAIState prevState;
    public AudioClip[] Clips;
    AudioSource audioSrc;

    public override void Update()
    { 
        // draw a circle on the ground
        if(vuln)
        {
            curTime += Time.deltaTime;
            if (curTime > base.AggroTime)
                curTime = base.AggroTime;

            // The circles size is based on how long it'll take to 
            CreatePoints(); 
        }

        vuln = (base.AIState == EnemyAIState.Vulnerable);
        line.enabled = vuln;
        CheckDistance();

        if (base.AIState == EnemyAIState.Vulnerable && prevState != base.AIState && audioSrc != null)
        {
            audioSrc.clip = Clips[0];
            audioSrc.Play();
        }

        prevState = base.AIState;
        base.Update();
    }

    public override void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = (segments + 1);
        line.useWorldSpace = false;

        ind_Usable = GetComponentInChildren<UsableIndicator>();
        audioSrc = GetComponent<AudioSource>();
        base.Start();
    }

    public override void OnHit(int _damage)
    {
        if(audioSrc != null)
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

    void CheckDistance()
    {
        // If we're vulnerable
        if (base.AIState == EnemyAIState.Vulnerable)
        {
            // Check to see if we can be disarmed and make it so
            Vector3 distToPlayer = playerRef.transform.position - transform.position;
            distToPlayer = GlobalConstants.ZeroYComponent(distToPlayer);

            ind_Usable.ind_Enabled = distToPlayer.magnitude < 2;


            if (Input.GetKeyDown(KeyCode.E) && distToPlayer.magnitude < 3.5f)
            {
                GetComponent<Animator>().SetBool("Disarmed", true);
                base.AIState = EnemyAIState.Defeated;

                // Toss the weapon
                //usableWeapon w = Instantiate(DisarmedWeapon, transform.position, Quaternion.identity).GetComponent<usableWeapon>();
                base.TossWeapon(transform.position - playerRef.transform.position);

                //GetComponent<CController>().ApplyForce((transform.position - playerRef.transform.position).normalized * 20);
                base.myCC.ApplyForce((transform.position - playerRef.transform.position).normalized * 20);
            }
        } else
        {
            ind_Usable.ind_Enabled = false;
        }
    }



    PlayerScript playerRef
    {
        get
        {
            if (playerref == null)
                playerref = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

            return playerref;
        }
    }
}
