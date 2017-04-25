using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapCapsule : MonoBehaviour,INonUnit {


    public int NumberOfScrap;
    public GameObject Scrap;
    public AudioClip clip;

    List<Scrap> ScrapList;
    void Start()
    {
        // Make a new list
        ScrapList = new List<Scrap>();

        // Generate all the scrap you need
        for (int i = 0; i < NumberOfScrap; i++)
        {
            Scrap s = Instantiate(Scrap, transform.position, Scrap.transform.rotation).GetComponent<Scrap>();
            
            ScrapList.Add(s); 
        }

        // Disable the collisions between them alllll
       
        for(int x = 0; x < ScrapList.Count;x++)
        {
            for(int y = 0; y < ScrapList.Count; y++)
            {
                Physics.IgnoreCollision(ScrapList[x].myNotTriggerCollider, ScrapList[y].myNotTriggerCollider);
            }
        }

        // Then disable the object
        for(int i = 0; i < ScrapList.Count;i++)
            ScrapList[i].gameObject.SetActive(false);
    }
    
    // This object can't be powered
    public bool Powered
    {
        get
        {
            return false;
        }

        set
        {
            // do nothing
        }
    }

    public void OnEMP()
    {
        // break!
        BreakCapsule();
    }

    public void OnHit()
    {
        // break!
        BreakCapsule();
    }

    // The method we want to call for 99% of the interactions
    void BreakCapsule()
    {
        myAudioSystem.PlayAudioOneShot(GetComponent<AudioSource>(), transform.position);
        for(int i = 0; i <ScrapList.Count; i++)
        {
            ScrapList[i].gameObject.SetActive(true);
            ScrapList[i].Force();
        }
        Destroy(this.gameObject);
    }

    public bool Triggered
    {
        get
        { return false; }

        set
        {  // nothing 
        }
    }
}
