using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ForceFieldScript : MonoBehaviour {

    public float Health;
    public int MaxHealth;
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

        myMat.SetFloat("_Edges", (1 - (Health / MaxHealth)) / 5);
        if (Health <= 0)
        {
            disolveValue += .03f;
            disolveValue = Mathf.Min(disolveValue, 1f);
            if (disolveValue < 0)
                disolveValue = 0;
            myMat.SetFloat("_Level", disolveValue);
        }

        timeSinceHit += Time.deltaTime;
        if (timeSinceHit > 3 && Health != MaxHealth)
        {
            HealShield();
        }
    }

    public void RegisterHit(int _damage)
    {
        // Take a hit
        Health -= _damage;

        // Turn on every barrier color
        Barrier.enabled = true;

        timeSinceHit = 0;

        if(Health <= 0)
        {
            disolveValue = 0;
        }
    }

    void HealShield()
    {
        if (MaxHealth <= 0)
            return;

        // RegenTime should be how much shield is regenerated per second
        regenCheck = Time.deltaTime;
        float addedHealth = regenCheck * RegenTime;
        Health += addedHealth;

        // Turn on every barrier color
        Barrier.enabled = true;

        // And then if we're at full health turn it off
        if(Health >= MaxHealth)
        {
            Health = MaxHealth;
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
