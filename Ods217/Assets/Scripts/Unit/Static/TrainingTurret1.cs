using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTurret1 : MonoBehaviour, IArmed {

    public UnitStruct myUnit;
    IWeapon MyWeapon;
    HealthBar healthBar;
    public GameObject gunObj;

    public GameObject OriginPoint;
    public float StartingDegrees;
    public bool triggered;
     
    PlayerScript playerScript;
     

    // Use this for initialization
    void Start () {
        MyWeapon = gunObj.GetComponent<IWeapon>();
        healthBar = GetComponentInChildren<HealthBar>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>(); 

        MyWeapon.Owner = this; 
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

    IWeapon IArmed.myWeapon
    {
        get
        {
            return GetComponentInChildren<IWeapon>();
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
            gameObject.SetActive(false);
        }
    }


    public virtual void TossWeapon()
    {

    }

}
