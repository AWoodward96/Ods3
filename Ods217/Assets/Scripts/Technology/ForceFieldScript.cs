using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ForceFieldScript : MonoBehaviour {

	UnitStruct myOwner;

	// These are both now obsolete, replaced by myOwner's stats
    //public float Health;
    //public int MaxHealth;

    public float RegenTime;
    private bool recentHit;
    SpriteRenderer Barrier; // The sprite that is surrounding the forcefield 

    float timeSinceHit;
    float regenCheck;

    [HideInInspector]
    public HealthBar myHealthBar;

    Material myMat;
    float disolveValue;

	// Use this for initialization
	void Start () {
		myOwner = transform.parent.GetComponent<IDamageable>().MyUnit;

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
            disolveValue += .03f;
            disolveValue = Mathf.Min(disolveValue, 1f);
            if (disolveValue < 0)
                disolveValue = 0;
            myMat.SetFloat("_Level", disolveValue);
        }

        timeSinceHit += Time.deltaTime;
        if (timeSinceHit > 3 && myOwner.CurrentEnergy != myOwner.MaxEnergy)
        {
            HealShield();
        }
    }

    public void RegisterHit(int _damage)
    {
        // Take a hit
        myOwner.CurrentEnergy -= _damage;

        // Turn on every barrier color
        Barrier.enabled = true;

        timeSinceHit = 0;

        if(myOwner.CurrentEnergy <= 0)
        {
            disolveValue = 0;
        }
    }

    void HealShield()
    {
        if (myOwner.MaxEnergy <= 0)
            return;

        // RegenTime should be how much shield is regenerated per second
        regenCheck = Time.deltaTime;
        float addedHealth = regenCheck * RegenTime;
		myOwner.CurrentEnergy += (int)Mathf.Ceil(addedHealth);

        // Turn on every barrier color
        Barrier.enabled = true;

        // And then if we're at full health turn it off
        if(myOwner.CurrentEnergy >= myOwner.MaxEnergy)
        {
            myOwner.CurrentEnergy = myOwner.MaxEnergy;
            StopAllCoroutines();
            StartCoroutine(startRecentHit());
        }

        disolveValue -= .03f;
        if (disolveValue < 0)
            disolveValue = 0;
        myMat.SetFloat("_Level", disolveValue);

        if (myHealthBar != null)
        {
            myHealthBar.ShowMenu();
        }
    }

    // This coroutine lets us have a bright barrier be shown for 1 second before it starts to fade
    IEnumerator startRecentHit()
    {
        yield return new WaitForSeconds(1);
        recentHit = true;
    }
}
