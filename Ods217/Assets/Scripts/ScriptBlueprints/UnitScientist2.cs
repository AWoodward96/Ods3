using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

[RequireComponent(typeof(CController))]
public class UnitScientist2 : MonoBehaviour,IUnit {

    public UnitStruct myUnit; 
    GameObject GunObj;
    CController myCtrl;

    //[Range(1,99)]
    //public int SpecialPercentChance; 
    //public int SpecialCount;
    //public bool Special;
    

    public Vector3 LookingVector;

    Animator myAnimator;
    CharacterController myCharacterController;
    BoxCollider myBoxCollider;
    SpriteRenderer myRenderer;
    GameObject PlayerObject;
    ForceFieldScript FFScript;
    DropShadowScript dShadowScript;

    // Use this for initialization
    void Awake() {
        myAnimator = GetComponent<Animator>();
        myCharacterController = GetComponent<CharacterController>();
        myBoxCollider = GetComponent<BoxCollider>();
        dShadowScript = GetComponentInChildren<DropShadowScript>();
        myRenderer = GetComponent<SpriteRenderer>();
        FFScript = GetComponentInChildren<ForceFieldScript>();
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (!PlayerObject)
        {
            PlayerObject = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            LookingVector = (PlayerObject.transform.position - transform.position).normalized;
            myAnimator.SetFloat("SpeedX", LookingVector.x); 
        }

        if (myUnit.CurrentHealth <= 0)
            OnDeath();

    }
    

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    public IWeapon MyWeapon
    {
       get{ return null; }
    }

    public void OnDeath()
    {
        myAnimator.SetBool("Death", true);
        myCharacterController.height = .1f;
        myCharacterController.center = new Vector3(myCharacterController.center.x, -1.15f, myCharacterController.center.z);
        // hardcoded
        myBoxCollider.center = new Vector3(0.4105735f, -1.242836f, 0.3205181f);
        myBoxCollider.size = new Vector3(3.35992f, 0.6082693f, 0.49f);
 

        dShadowScript.transform.localScale = Vector3.Lerp(dShadowScript.transform.localScale, new Vector3(17, dShadowScript.transform.localScale.y,dShadowScript.transform.localScale.z), 2f * Time.deltaTime);
        dShadowScript.Center = Vector3.Lerp(dShadowScript.Center, new Vector3(.4f,0,0),2f * Time.deltaTime);
    }

    public void OnHit(IWeapon _FromWhatWeapon)
    {  
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        if(myVisualizer)
            myVisualizer.ShowMenu();

        if(FFScript)
        {
            if(FFScript.Health > 0)
                FFScript.RegisterHit(_FromWhatWeapon.myWeaponInfo.bulletDamage);
            else
                myUnit.CurrentHealth -= _FromWhatWeapon.myWeaponInfo.bulletDamage;
        }
        else
            myUnit.CurrentHealth -= _FromWhatWeapon.myWeaponInfo.bulletDamage;   
    }


    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

}
