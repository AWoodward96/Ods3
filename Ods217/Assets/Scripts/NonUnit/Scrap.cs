using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : MonoBehaviour {

    public int Value;
    public int myForce; 

    AudioSource mySource;
    Rigidbody myRigidbody;

    public SphereCollider myNotTriggerCollider;
    Vector3 theForce;

	// Use this for initialization
	void Awake () {
        myRigidbody = GetComponent<Rigidbody>();
        mySource = GetComponent<AudioSource>();

        SphereCollider[] mycolliders = GetComponents<SphereCollider>();
        for(int i = 0; i < mycolliders.Length;i++)
        {
            if (!mycolliders[i].isTrigger)
                myNotTriggerCollider = mycolliders[i];
        }

        // Send this object flying in a random direction
        Vector2 direc = Random.insideUnitCircle * myForce;
        theForce = new Vector3(direc.x, 50, direc.y);
      

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
            mySource.Play();

         
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
        myRigidbody.AddForce(theForce);
    }
 
    IEnumerator death()
    {
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }
}
