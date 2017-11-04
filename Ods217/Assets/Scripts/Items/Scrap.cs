using System.Collections;
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

    public AudioSource myPickup;
    public AudioSource myBlip;
    public AudioClip[] blips; // An array of possible sounds that play when hitting something
    Rigidbody myRigidbody;

    public SphereCollider myNotTriggerCollider; // Since there are two sphere colliders on this object we need to distinguish which one is trigger and which one isnt
    Vector3 theForce;

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
        theForce = new Vector3(direc.x, 5, direc.y);


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
