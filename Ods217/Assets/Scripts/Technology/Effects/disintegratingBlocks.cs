using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

public class disintegratingBlocks : MonoBehaviour {

    public BoxCollider TriggerCollider;
    public BoxCollider FloorCollider;
    public float TimeToBreak;
    public float TimeToReset;

    AudioSource mySource;

    Animator[] myAnimator;
    SpriteRenderer[] myRenders;
    float currentTime;
    bool inside;
    bool dead;

	// Use this for initialization
	void Start () {
        TriggerCollider.isTrigger = true;
        FloorCollider.isTrigger = false;

        myAnimator = GetComponentsInChildren<Animator>();
        myRenders = GetComponentsInChildren<SpriteRenderer>();
        mySource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if(inside)
        {
            currentTime += Time.deltaTime;

            setAnimator(true);

            if(currentTime>TimeToBreak)
            {
                inside = false;
                dead = true;
                currentTime = 0;
                //setRenderers(false);
            }
        }

        if(dead)
        {
            currentTime += Time.deltaTime;
            if(currentTime > TimeToReset)
            {
                dead = false;
                currentTime = 0;
                //setRenderers(true);

                setAnimator(false);
            }
        }

        FloorCollider.enabled = !dead;
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !dead)
        {
            inside = true;
            currentTime = 0;
            if(mySource)
            {
                mySource.Play();
            }   
        }
    }

    void setRenderers(bool _val)
    {
        foreach (SpriteRenderer rend in myRenders)
            rend.enabled = _val;
    }

    void setAnimator(bool _val)
    {
        foreach (Animator anim in myAnimator)
            anim.SetBool("State", _val);
    }
}
