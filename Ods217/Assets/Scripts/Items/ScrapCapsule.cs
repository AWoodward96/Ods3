using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A NonUnit Script
/// A container that explodes with scrap when hit with a bullet
/// </summary>
public class ScrapCapsule : MonoBehaviour, IDamageable
{ 

    public int NumberOfScrap; // How many scrap prefabs will we instantiate
    public GameObject Scrap; // Whats the scrap gameobject we'll instantiate
    public AudioClip clip; // Whats the sound we'll play when we're hit

    public UnitStruct myUnit;

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

        for (int x = 0; x < ScrapList.Count; x++)
        {
            for (int y = 0; y < ScrapList.Count; y++)
            {
                Physics.IgnoreCollision(ScrapList[x].myNotTriggerCollider, ScrapList[y].myNotTriggerCollider);
            }
        }

        // Then disable the object
        for (int i = 0; i < ScrapList.Count; i++)
            ScrapList[i].gameObject.SetActive(false);
    }
    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    }


    public void OnHit(int _Damage)
    {
        // break!
        BreakCapsule();
    }

    // The method we want to call for 99% of the interactions
    void BreakCapsule()
    {
        myAudioSystem.PlayAudioOneShot(GetComponent<AudioSource>(), transform.position);
        for (int i = 0; i < ScrapList.Count; i++)
        {
            ScrapList[i].gameObject.SetActive(true);
            ScrapList[i].Force();
        }
        Instantiate(Resources.Load("Prefabs/Particles/ScrapContainerDeath"), transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public bool Triggered
    {
        get
        { return false; }

        set
        {  // nothing 
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


    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
