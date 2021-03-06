﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script for collectable scrap objects
/// These will be living in canisters and when those cannisters are destroyed, these will pop out and fly in a random direction
/// </summary>
public class Scrap : MonoBehaviour
{

    public int Value; // How much the scrap is worth
    public int myForce;
	public float vaccuumRadius;
	public float vaccuumSpeed;
	bool isVaccuuming;
	float vaccuumingTime;

    public AudioSource myPickup;
    public AudioSource myBlip;
    public AudioClip[] blips; // An array of possible sounds that play when hitting something
    Rigidbody myRigidbody;

	public float maxAngularVelocity;

    public SphereCollider myNotTriggerCollider; // Since there are two sphere colliders on this object we need to distinguish which one is trigger and which one isnt
    Vector3 theForce;

	PlayerScript player;

    // Use this for initialization
    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>(); 

        // set up the collider parameters
        SphereCollider[] mycolliders = GetComponents<SphereCollider>();
        for (int i = 0; i < mycolliders.Length; i++)
        {
            if (!mycolliders[i].isTrigger)
                myNotTriggerCollider = mycolliders[i];
        }

        // Send this object flying in a random direction
        Vector2 direc = Random.insideUnitCircle * myForce;
		theForce = new Vector3(direc.x, 10 + (Random.value * 15), direc.y);


		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		isVaccuuming = false;
		vaccuumingTime = 0.0f;

		myRigidbody.maxAngularVelocity = maxAngularVelocity;
    }

	void FixedUpdate()
	{
		Vector3 myPosition = GlobalConstants.ZeroYComponent(transform.position);
		Vector3 playerPosition = GlobalConstants.ZeroYComponent(player.transform.position);

		if((myPosition - playerPosition).magnitude <= vaccuumRadius || isVaccuuming)
		{
			isVaccuuming = true;
			myRigidbody.velocity = (player.transform.position - transform.position).normalized * (vaccuumSpeed + (vaccuumingTime * vaccuumSpeed));

			vaccuumingTime += Time.fixedDeltaTime;
		}
	}

    // When it collides with a player add its value to the game managers scrap count and start it's destructive coroutine
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.ScrapCount += Value;
            MenuManager.instance.ShowScrap(); // Let the menu manager know that we've picked up some scrap
            myPickup.Play();


            // Remove it from vision
            GetComponent<SpriteRenderer>().enabled = false;
			transform.Find("DropShadow").GetComponent<SpriteRenderer>().enabled = false;
            foreach (SphereCollider sphere in GetComponents<SphereCollider>())
            {
                // Disable each collider
                sphere.enabled = false;
            }

            // Start death coroutine
            StartCoroutine(death());
        }
    }

    // Make the velocity of this object equal to the force
    public void Force()
    {
        myRigidbody.velocity = theForce;
		myRigidbody.angularVelocity = new Vector3(0.0f, (2.0f * Mathf.PI) + (Random.value * 2.0f), 0.0f);
    }

    // Destroy this game object 1 second after it's picked up so we don't have to worry about it
    IEnumerator death()
    {
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }

    // When this object collides with something else play a blip (Called when it bounces off of the floor etc)
    void OnCollisionEnter(Collision collision)
    {
        int newRnd = Random.Range(0, blips.Length);
        myBlip.clip = blips[newRnd]; 
        myBlip.Play();
    }
}
