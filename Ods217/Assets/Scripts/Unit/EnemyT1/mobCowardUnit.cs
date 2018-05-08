using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))] 
[RequireComponent(typeof(AudioSource))]
public class mobCowardUnit : AIStandardUnit {
     
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

    public override void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = (segments + 1);
        line.useWorldSpace = false;

        ind_Usable = GetComponentInChildren<UsableIndicator>();
        ind_Usable.Preset = UsableIndicator.usableIndcPreset.Disarm;
        ind_Usable.Output = DisarmDelegate;

        audioSrc = GetComponent<AudioSource>();
        base.Start();
    }

    public override void FixedUpdate()
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

        if (base.AIState == EnemyAIState.Vulnerable && prevState != base.AIState && audioSrc != null && Clips.Length > 0)
        {
            audioSrc.clip = Clips[0];
            audioSrc.Play();
        }

        prevState = base.AIState;
        base.FixedUpdate();
    }



    public override void OnHit(int _damage)
    {
        if(audioSrc != null && Clips.Length > 0 && MyUnit.CurrentHealth > 0)
        { 
            audioSrc.clip = Clips[1];
            audioSrc.Play();
        }
        base.OnHit(_damage);
    }

    public override void OnMelee(int _damage)
    {
        if(!vuln)
            base.OnMelee(_damage);
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

    public virtual void CheckDistance()
    {
        ind_Usable.Disabled = !(base.AIState == EnemyAIState.Vulnerable);
    }

    public void DisarmDelegate()
    {
        GetComponent<Animator>().SetBool("Disarmed", true);
        base.AIState = EnemyAIState.Defeated;

        // Toss the weapon
        //usableWeapon w = Instantiate(DisarmedWeapon, transform.position, Quaternion.identity).GetComponent<usableWeapon>();
        base.TossWeapon(transform.position - playerRef.transform.position);

        //GetComponent<CController>().ApplyForce((transform.position - playerRef.transform.position).normalized * 20);
        base.myCC.ApplyForce((transform.position - playerRef.transform.position).normalized * 20);
    }

  
}
