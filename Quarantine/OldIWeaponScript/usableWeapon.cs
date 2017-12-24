using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UsableIndicator))]
public class usableWeapon : MonoBehaviour {
     
    public AudioClip SoundToPlayWhenTriggered; 
    AudioSource mySource;

    public enum TossType { Toss, Place };
    public TossType WillToss;
    public enum HoldType { CircleAim, Hold }
    public HoldType holdType;

    [Range(.1f, 10)]
    public float Range;
    public bool Interactable;

    UsableIndicator ind_Interactable;
    GameObject ind_interactableSprite;
    GameObject Player;
    GameObject dropShadowObject;
    Rigidbody myRigidBody;

    public GameObject weaponObject;
    IWeapon myWeapon;

    public bool isBeingUsed;
    Collider myCollider;

    bool initialized = false;

    // Use this for initialization
    void Start () {
        ind_Interactable = GetComponent<UsableIndicator>();
        ind_interactableSprite = transform.Find("MyIndicator").gameObject;
        myCollider = GetComponent<Collider>();
        myRigidBody = GetComponent<Rigidbody>();
        dropShadowObject = GetComponentInChildren<DropShadowScript>().gameObject;
        

        mySource = GetComponent<AudioSource>();
        mySource.playOnAwake = false;
        mySource.spatialBlend = 1;

        mySource.clip = SoundToPlayWhenTriggered;

        myWeapon = GetComponentInChildren<IWeapon>();
        if (myWeapon == null)
            myWeapon = GetComponent<IWeapon>();

        initialized = true;
    }
	
	// Update is called once per frame
	void Update () {
        // If we don't have a weapon then fuck what are we doing this can't work!
        if (myWeapon == null)
            return;

        if(!isBeingUsed)
        {
            if (Player == null) // Ensure that we have the player
            {
                Player = GameObject.FindGameObjectWithTag("Player");
                Physics.IgnoreCollision(myCollider, Player.GetComponent<Collider>(), true);
            }

            // Check to make sure we can even do this calculation
            // At this point we'll know if there's a player in the scene at all
            if (Player != null)
            {
                Vector3 dist = transform.position + (Vector3.up * 2) - Player.transform.position;
                Interactable = (dist.magnitude <= Range);
                ind_Interactable.ind_Enabled = Interactable;
                ind_interactableSprite.transform.rotation = Quaternion.Euler(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z);
            }


            // If we get input that we want to interact, and we're able to interact with it
            if (Input.GetKeyDown(KeyCode.E) && Interactable)
            {
                PickedUp();
            }
        }

        dropShadowObject.SetActive(!isBeingUsed);
        myRigidBody.isKinematic = isBeingUsed;
        myCollider.enabled = !isBeingUsed;
    }

    /// <summary>
    /// Thow this weapon with reckless abandon!
    /// </summary>
    /// <param name="dir">In what direction will this be tossed. Magnitutde does not matter</param>
    /// <param name="pos">Where this will be tossed from</param>
    public void Toss(Vector3 dir, Vector3 pos)
    {
        // We've been tossed!
        isBeingUsed = false;

        // Reset the transform
        if(WillToss == TossType.Toss)
        {
            myRigidBody = GetComponent<Rigidbody>();
            myRigidBody.isKinematic = false;
            myRigidBody.velocity = dir.normalized;
            myRigidBody.AddForce(dir.normalized * 500);
            myRigidBody.AddTorque(new Vector3(0, 500, 0) * ((Random.Range(0, 1) == 1) ? 1 : -1));
        }


        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(1, 1, 1);
        transform.position = pos;
        weaponObject.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
         
         
    }

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position + (Vector3.up * 2), Range);
    } 

    public void PickedUp()
    {
        if (!initialized)
            Start();

        // First things first, toggle the flag to say we're being used
        isBeingUsed = true; 

        // Reset the objects rotation and scale 
        weaponObject.transform.localScale = new Vector3(1, 1, 1);
        weaponObject.transform.localRotation = Quaternion.identity;

        // Turn off the usable indicator
        ind_Interactable.ind_Enabled = false;

        // Play a sound if we want to
        if (SoundToPlayWhenTriggered != null)
        {
            mySource.clip = SoundToPlayWhenTriggered;
            mySource.Play();
        }

        // Tell the player that it's picked up an object
        if (Player == null) // Ensure that we have the player 
        { 
            Player = GameObject.FindGameObjectWithTag("Player");
            Physics.IgnoreCollision(myCollider, Player.GetComponent<Collider>(), true);
        }

        PlayerScript ps = Player.GetComponent<PlayerScript>();
       // ps.PickUpWeapon(this.gameObject); 
    }

    public void PickedUp(IMultiArmed unit)
    {
        if (!initialized)
            Start();

        // First things first, toggle the flag to say we're being used
        isBeingUsed = true;

        // Reset the objects rotation and scale 
        weaponObject.transform.localScale = new Vector3(1, 1, 1);
        weaponObject.transform.localRotation = Quaternion.identity;

        // Turn off the usable indicator
        ind_Interactable.ind_Enabled = false;

        // Play a sound if we want to
        if (SoundToPlayWhenTriggered != null)
        {
            mySource.clip = SoundToPlayWhenTriggered;
            mySource.Play();
        }

        // Tell the player that it's picked up an object
        if (Player == null) // Ensure that we have the player 
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            Physics.IgnoreCollision(myCollider, Player.GetComponent<Collider>(), true);
        }

        //unit.PickUpWeapon(this.gameObject); 
    }
}
