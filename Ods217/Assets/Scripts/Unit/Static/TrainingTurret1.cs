using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTurret1 : MonoBehaviour, IArmed {

    public UnitStruct myUnit;
    WeaponBase MyWeapon;
    HealthBar healthBar;
    public GameObject gunObj;

    public GameObject OriginPoint;
    public float StartingDegrees;
    public bool triggered;
     
    PlayerScript playerScript;
     

    // Use this for initialization
    void Start () {
        MyWeapon = gunObj.GetComponent<WeaponBase>();
        healthBar = GetComponentInChildren<HealthBar>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

        MyWeapon.heldData.DisableMulti();
        MyWeapon.myOwner = this; 
	}
	
	// Update is called once per frame
	void Update () {
        if (triggered)
        {
            Vector3 toPlayer = playerScript.gameObject.transform.position - gunObj.transform.position;
            gunObj.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(toPlayer));
            MyWeapon.FireWeapon(toPlayer);
        } 
	}


    public UnitStruct MyUnit
    {
        get
        {
            return myUnit;
        }

        set
        {
            myUnit = value;
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
            return healthBar;
        }
    }

    WeaponBase IArmed.myWeapon
    {
        get
        {
            return GetComponentInChildren<WeaponBase>();
        }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public void OnHit(int _damage)
    {
        myUnit.CurrentHealth -= _damage;
        healthBar.ShowMenu();

        if(myUnit.CurrentHealth <= 0)
        {
            GameObject obj = Resources.Load("Prefabs/Particles/deathPartParent") as GameObject;
            Instantiate(obj,transform.position,obj.transform.rotation);
            MyWeapon.ReleaseWeapon();
            gameObject.SetActive(false);
        }
    }

 
    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
