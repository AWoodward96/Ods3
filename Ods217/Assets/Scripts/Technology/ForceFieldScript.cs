using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ForceFieldScript : MonoBehaviour {

	UnitStruct myOwner;

	// These are both now obsolete, replaced by myOwner's stats
    //public float Health;
    //public int MaxHealth;

    private bool recentHit;
    SpriteRenderer Barrier; // The sprite that is surrounding the forcefield 

    float regenCheck;

    [HideInInspector]
    public HealthBar myHealthBar;

	EnergyManager myEnergy;

    Material myMat;
    float disolveValue;

	// Use this for initialization
	void Start () {
		myOwner = transform.parent.GetComponent<IDamageable>().MyUnit;
		myEnergy = transform.parent.GetComponent<EnergyManager>();

        Barrier = GetComponent<SpriteRenderer>();
        myMat = GetComponent<Renderer>().material;
        Barrier.enabled = false;
        
        
	}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (recentHit)
        {
            recentHit = false;
            Barrier.enabled = false;
        }

		if(myOwner.MaxEnergy > 0)
		{
			myMat.SetFloat("_Edges", (1 - (myOwner.CurrentEnergy / myOwner.MaxEnergy)) / 5);
		}

        if (myOwner.CurrentEnergy <= 0)
        {
            //disolveValue += .03f;
			disolveValue += myEnergy.RegenTime / 1000.0f;
            disolveValue = Mathf.Min(disolveValue, 1f);
            if (disolveValue < 0)
                disolveValue = 0;
            myMat.SetFloat("_Level", disolveValue);
        }
			
		if (myEnergy.timeSinceHit > myEnergy.ChargeDelay && myOwner.CurrentEnergy != myOwner.MaxEnergy)
        {
            HealShield();
        }
    }

    public void RegisterHit(int _damage)
    {
        // Take a hit
		myEnergy.ExpendEnergy(_damage);

        // Turn on every barrier color
        Barrier.enabled = true;

        if(myOwner.CurrentEnergy <= 0)
        {
            disolveValue = 0;
        }
    }

    void HealShield()
    {
        if (myOwner.MaxEnergy <= 0)
            return;

		myEnergy.ChargeEnergy();

        // Turn on every barrier color
        Barrier.enabled = true;

        // And then if we're at full health turn it off
        if(myOwner.CurrentEnergy >= myOwner.MaxEnergy)
        {
            StopAllCoroutines();
            StartCoroutine(startRecentHit());
        }

        //disolveValue -= .03f;
		disolveValue -= myEnergy.RegenTime / 1000.0f;
        if (disolveValue < 0)
            disolveValue = 0;
        myMat.SetFloat("_Level", disolveValue);
    }

    // This coroutine lets us have a bright barrier be shown for 1 second before it starts to fade
    IEnumerator startRecentHit()
    {
        yield return new WaitForSeconds(1);
        recentHit = true;
    }
}
