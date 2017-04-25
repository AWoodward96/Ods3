using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : MonoBehaviour {

    public int Value;
    public int myForce; 

    AudioSource myPickup;
    AudioSource myBlip;
    public AudioClip[] blips;
    Rigidbody myRigidbody;

    public SphereCollider myNotTriggerCollider;
    Vector3 theForce;

	// Use this for initialization
	void Awake () {
        myRigidbody = GetComponent<Rigidbody>();
        AudioSource[] mySources = GetComponents<AudioSource>();

        
        foreach(AudioSource a in mySources)
        {
            if (a.clip.name == "blip")
                myBlip = a;
            else
                myPickup = a;
        }


        SphereCollider[] mycolliders = GetComponents<SphereCollider>();
        for(int i = 0; i < mycolliders.Length;i++)
        {
            if (!mycolliders[i].isTrigger)
                myNotTriggerCollider = mycolliders[i];
        }

        // Send this object flying in a random direction
        Vector2 direc = Random.insideUnitCircle * myForce;
        theForce = new Vector3(direc.x, 5, direc.y);
      

	}
	
	// Update is called once per frame
	void Update () { 

        //// enable both colliders
        //GetComponent<SpriteRenderer>().enabled = Active;
        //foreach (SphereCollider sphere in GetComponents<SphereCollider>())
        //{
        //    sphere.enabled = Active;
        //}
        


    }
 

    private void OnTriggerEnter(Collider other)
    { 
        if (other.tag == "Player")
        { 
            GameManager.ScrapCount += Value;
            MenuManager.instance.ShowScrap();
            myPickup.Play();


         
            GetComponent<SpriteRenderer>().enabled = false;
            foreach (SphereCollider sphere in GetComponents<SphereCollider>())
            {
                sphere.enabled = false;
            }


            StartCoroutine(death()); 
        }
    }

    public void Force()
    {
        myRigidbody.velocity = theForce;
       // myRigidbody.velocity(theForce);
    }
 
    IEnumerator death()
    {
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        int newRnd = Random.Range(0, blips.Length);
        myBlip.clip = blips[newRnd];

        myBlip.Play();
    }
}
