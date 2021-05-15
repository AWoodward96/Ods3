using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMover : MonoBehaviour, IPermanent {

    public bool Active;
    public Vector3 Direction = new Vector3(1,0,0);
    public float Speed = .1f;
    public int Health = 3;
    int MaxHealth = 3;
    ZoneScript z;

    public GameObject destroyMe;	// What wall is to be destroyed by the box mover?
    public GameObject RespawnPosition;
    GameObject CenterPosition;
    GameObject BottomPosition;

    bool trapped = false;
    bool grounded = true;


    public bool DEBUG = false;
    float DT = 0; // Need a constant reference to DeltaTime

    LayerMask Ground;
    LayerMask Player;
    Vector3 KillCast = new Vector3(.56f, 1f, .75f); // .56 is the player colliders radius - .75 isn't terribly important
    Vector3 StopCast = new Vector3(.1f, 1f, .75f);
    Vector3 SelfKillCast = new Vector3(4, 1, 2);

    ParticleSystem aoePoof;
    ParticleSystem movingPoof;

    UsableIndicator ind_Usable;

    // Use this for initialization
    void Start () {
        Ground = LayerMask.GetMask("Ground");
        Player = LayerMask.GetMask("Units");
        MaxHealth = Health;

        CenterPosition = transform.Find("CenterPoint").gameObject;
        BottomPosition = transform.Find("BottomPoint").gameObject;

        movingPoof = transform.Find("SlitterPoof").GetComponent<ParticleSystem>();
        aoePoof = transform.Find("AOEPoof").GetComponent<ParticleSystem>();

        ind_Usable = GetComponentInChildren<UsableIndicator>();
        ind_Usable.Output = OnInteract;

        
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        DT = Time.deltaTime;

        // Physics doesn't care if you are active or not
        HandleFalling();

        movingPoof.gameObject.SetActive(Active && grounded);

        if (Active)
        {

            if (grounded)
            {
                // handle the selfkill
                SelfKill();

                // handle moving
                MoveZone();

                // handle the killzone
                KillZone();

                // handle the selfkill
                SelfKill();

                // handle riding
                RideOrDie();
            } 

        }
	}

    void OnInteract()
    {
        Active = true;
        ind_Usable.Disabled = true;
    }

    void HandleFalling()
    {
        // cast down to see if we're on the ground or not
        Vector3 cast = SelfKillCast;
        cast.x *= transform.localScale.x;
        cast.z *= transform.localScale.z;

        Collider[] cols = Physics.OverlapBox(BottomPosition.transform.position, cast, transform.rotation, Ground, QueryTriggerInteraction.Ignore);
        if((cols.Length == 1 && cols[0] == GetComponent<Collider>()) || cols.Length == 0)
        {
            grounded = false;
            transform.position += Vector3.down * GlobalConstants.Gravity * 2 * DT;
        }
        else
        {
            if(!grounded)
            {
                grounded = true;
                aoePoof.Play(); 
            }
        }

        // Also if there's a unit underneath there then fuckin kill him
        cols = Physics.OverlapBox(BottomPosition.transform.position, cast, transform.rotation, Player, QueryTriggerInteraction.Ignore);
        if(cols.Length > 0)
        {
            for(int i = 0; i < cols.Length; i++)
            {
                IDamageable dmg = cols[i].GetComponent<IDamageable>();
                if(dmg != null)
                {
                    if (DEBUG) Debug.Log("Killed: " + cols[i].gameObject + " because it was under me.");
                    dmg.OnHit(10000);
                    dmg.OnHit(10000);
                }
            }
        }
    }

    void RideOrDie()
    {
        // Allow any unit to ride this bad boy for miles
        Vector3 cast = SelfKillCast;
        cast.x *= transform.localScale.x;
        cast.z *= transform.localScale.z;
        cast.y = 4;

        Collider[] cols = Physics.OverlapBox(CenterPosition.transform.position + (Vector3.up * 2), cast, transform.rotation, Player, QueryTriggerInteraction.Ignore);
        if (cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].transform.position += (Direction * Speed * DT); 
            }
        }
    }

    void MoveZone()
    {
        // if we're not trapped then move
        if (trapped)
            return;

        // if there is not anything in front of us move!
        StopCast.x = Speed * DT;
        Collider[] cols = Physics.OverlapBox(transform.position, StopCast, Quaternion.identity, Ground, QueryTriggerInteraction.Ignore);

        // If the collider isn't just itself or the amount is 0
        if ((cols.Length == 1 && cols[0] == GetComponent<Collider>()) || cols.Length < 1)
        {
            transform.position += Speed * Direction * DT;
        }
        else
        {
            for(int i = 0; i < cols.Length; i ++)
            {
                if(cols[i].gameObject == destroyMe)
                {
                    destroyMe.SetActive(false);
                }

                if (DEBUG) Debug.Log("Hit taken from: " + GlobalConstants.GetGameObjectPath(cols[i].transform));
            }

            // Take a hit and jump back a bit
            transform.position -= Direction.normalized;

            Health--;
            // If it has no health destroy it
            if (Health <= 0)
            { 
                movingPoof.gameObject.SetActive(false);
                StartCoroutine(KillCRT());
            }
        }

    }


    // The crt that runs when this object dies
    IEnumerator KillCRT()
    {
        transform.position = RespawnPosition.transform.position;

        MeshRenderer rend = GetComponent<MeshRenderer>();
        rend.enabled = true; 

        Active = false;
         
        for(int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(.1f);
            rend.enabled = !rend.enabled; 
        }
         
        rend.enabled = true;
        ind_Usable.Disabled = false;
        Health = MaxHealth;

    }


    void KillZone()
    {
        // If any unit gets in the way of the people mover kill them
        KillCast.x = .56f + (Speed * DT);
        Collider[] cols = Physics.OverlapBox(transform.position, KillCast, Quaternion.identity, Player,QueryTriggerInteraction.Ignore);
        if (cols.Length > 0)
        {

            for (int i = 0; i < cols.Length; i++)
            {
                IDamageable d = cols[i].GetComponent<IDamageable>();
                if(d != null)
                {
                    if (DEBUG) Debug.Log("Killed: " + cols[i].gameObject + " because it was in my killzone.");
                    d.OnHit(1000);
                    d.OnHit(1000);
                }
            }
        }
    }
     
    void SelfKill()
    {  
        Vector3 cast = SelfKillCast;
        cast.x *= transform.localScale.x;
        cast.z *= transform.localScale.z;

        Collider[] cols = Physics.OverlapBox(CenterPosition.transform.position, cast, transform.rotation, Ground, QueryTriggerInteraction.Ignore);
        if (cols.Length > 0)
        {
            // Check if its us
            if (cols.Length == 1 && cols[0] == GetComponent<Collider>())
            {
                trapped = false;
                return;
            } 

            
            for(int i = 0; i < cols.Length; i++)
            {
                if(DEBUG) Debug.Log("We're being trapped by: " + GlobalConstants.GetGameObjectPath(cols[i].transform)); 
                movingPoof.gameObject.SetActive(false);
                StartCoroutine(KillCRT());
            }

            // If it isn't us then we're fuckin stuck man
            trapped = true;
        } else
            trapped = false;
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
            return Active;
        }

        set
        {
            Active = value;
        }
    }
}
