using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ExplosiveBox : MonoBehaviour, IDamageable, ISavable {

    UnitStruct myUnit;
    ZoneScript zone;
    public bool State;
    ParticleSystem mySystem;
    AudioSource src;
    public GameObject Unbroken;
    public GameObject Broken;

    BoxCollider myCol;

    public bool DealDamage;
    public int Damage;

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

    Vector3 defaultSize = new Vector3(4, 6, 4);
    Vector3 defaultPosition = new Vector3(0, 3, -2);
    Vector3 brokenSize = new Vector3(4, .2f, 4);
    Vector3 brokenPosition = new Vector3(2, .1f, -2);


	void Awake()
	{
		mySystem = GetComponentInChildren<ParticleSystem>();
		src = GetComponent<AudioSource>();
		myCol = GetComponent<BoxCollider>();
	}
	
 
    public UnitStruct MyUnit
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public ZoneScript myZone
    {
        get
        {
            return zone;
        }

        set
        {
            zone = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return State;
        }

        set
        {
            State = value;
            if(State)
            {
                if (src != null)
                    src.Play();

                if(mySystem != null)
                    mySystem.Play();

				if(DealDamage)
				{
					Explode();
				}
            }

            myCol.size = (State) ? brokenSize : defaultSize;
            myCol.center = (State) ? brokenPosition : defaultPosition;

            Unbroken.SetActive(!State);
            Broken.SetActive(State);
        }
    }
		
    public void OnHit(int _damage)
    {
        if(!Triggered)
        {
            Triggered = true;
        }
    }

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    }

	public void Explode()
	{
		Collider[] cols = Physics.OverlapBox(transform.position, new Vector3(6, 6, 6));
		for (int i = 0; i < cols.Length; i++)
		{
			IDamageable dmg = cols[i].GetComponent<IDamageable>();
			if (dmg != null)
			{
				dmg.OnHit(Damage);
			}

			CController cc = cols[i].GetComponent<CController>();
			if(cc != null)
			{
				cc.ApplyForce(GlobalConstants.ZeroYComponent(cols[i].transform.position - transform.position).normalized * 20);
			}

			FredrickBoss boss = cols[i].GetComponent<FredrickBoss>();
			if(boss != null)
			{
				boss.StunBoss();
			}

			// TODO: Is this the best way to do this?
			mobTank myTank = cols[i].GetComponent<mobTank>();
			if(myTank != null)
			{
				myTank.CheckAttack(transform.position, Damage);
			}
		}
	}

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(State);

		return data.ToString();
	}

	// TODO: I feel like I shouldn't have to get components in a load method; is there a way to ensure this runs after Start()?
	public void Load(string[] data)
	{
		State = bool.Parse(data[0].Trim());

		myCol = GetComponent<BoxCollider>();
		myCol.size = (State) ? brokenSize : defaultSize;
		myCol.center = (State) ? brokenPosition : defaultPosition;
		
		Unbroken.SetActive(!State);
		Broken.SetActive(State);
	}
}
