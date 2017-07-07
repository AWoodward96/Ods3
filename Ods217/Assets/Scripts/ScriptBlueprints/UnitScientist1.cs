using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

[RequireComponent(typeof(CController))]
public class UnitScientist1 : MonoBehaviour,IUnit {

    public UnitStruct myUnit; 
    GameObject GunObj;
    CController myCtrl;

    [Range(1,99)]
    public int SpecialPercentChance; 
    public int SpecialCount;
    public bool Special;


    public Vector3 LookingVector;

    bool paniced;
    int SpecialPercRoot;
    Animator myAnimator;

    GameObject PlayerObject;

    // Use this for initialization
    void Awake() {
        SpecialPercRoot = SpecialPercentChance;
        myAnimator = GetComponent<Animator>();

        PlayerObject = GameObject.FindGameObjectWithTag("Player");
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        handleAnimations();

        if (!PlayerObject)
        {
            PlayerObject = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            if(Input.GetMouseButton(0) && Vector3.Distance(PlayerObject.transform.position, this.transform.position) < 10 && !PlayerObject.GetComponent<CController>().Sprinting && !paniced)
            {
                paniced = true;
                StopAllCoroutines();
                Special = true; 
                myAnimator.SetTrigger("Paniced");
                StartCoroutine(CanIPleaseJustHaveALoopFromXFrameFunction());
            }
        }

	}

    void handleAnimations()
    {
        if (Special)
            return;

        float rnd = UnityEngine.Random.Range(1, 100);
        if(rnd < SpecialPercentChance)
        {
            int irnd = UnityEngine.Random.Range(0, SpecialCount);
            myAnimator.SetFloat("Special", (float)irnd);
            myAnimator.SetTrigger("SpecTrigger");
            SpecialPercentChance = SpecialPercRoot + ((UnityEngine.Random.Range(0, 1) == 1) ? 1 : -1);
            StartCoroutine(waitForAnimation());

            Special = true;
        }
    }

    IEnumerator waitForAnimation()
    {
        yield return new WaitForSeconds(2);
        Special = false;
    }

    IEnumerator CanIPleaseJustHaveALoopFromXFrameFunction()
    {
        yield return new WaitForSeconds(.1f);
        myAnimator.ResetTrigger("Paniced");
    }
    

    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    public Weapon MyWeapon
    {
       get{ return null; }
    }

    public void OnDeath()
    {
        throw new NotImplementedException();
    }

    public void OnHit(Weapon _FromWhatWeapon)
    {  
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
        if(myVisualizer)
            myVisualizer.ShowMenu();
    }

    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

}
