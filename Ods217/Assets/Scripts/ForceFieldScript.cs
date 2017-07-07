using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ForceFieldScript : MonoBehaviour {

    public int Health;
    public int MaxHealth;
    private bool recentHit;
    SpriteRenderer Barrier; // The sprite that is surrounding the forcefield 
    



	// Use this for initialization
	void Start () {
        Barrier = GetComponent<SpriteRenderer>(); 
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(recentHit)
        { 
            FadeOutRenderers(Barrier, .02f); 

            if (Barrier.color.a == 0  )
            {
                recentHit = false;
            }
        }
	}

    // a modular function that fades out our sprite renderers for us
    void FadeOutRenderers(SpriteRenderer _rend, float _step)
    {
        Color c = _rend.color;
        c.a -= _step;
        if (c.a < 0)
            c.a = 0;

        _rend.color = c;
    }

    public void RegisterHit(int _damage)
    {
        // Take a hit
        Health -= _damage;
        StopAllCoroutines();
        StartCoroutine(startRecentHit());

        // Turn on every barrier color
        Color c = Barrier.color;
        c.a = Mathf.Min(c.a + .2f, 1f);
        Barrier.color = c;
    }

    IEnumerator startRecentHit()
    {
        yield return new WaitForSeconds(1);
        recentHit = true;
    }
}
