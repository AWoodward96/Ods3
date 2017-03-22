using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CController))]
public class PlayerScript : MonoBehaviour, IUnit
{
 
    GameObject GunObj;
    CController myCtrl;
    Animator myAnimator;
    SpriteRenderer myRenderer;
    GlobalConstants.Alligience myAlligience = GlobalConstants.Alligience.Ally;
    public Weapon myWeapon;
    public UnitStruct myUnit;

    Vector3 RootPosition = new Vector3(-0.138f, -0.138f, 0);
    Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
    Vector3 HolsteredRotation = new Vector3(0, 0, -88);

    AudioSource myAudioSource;
    public AudioClip[] AudioClips;

    // Use this for initialization
    void Awake()
    { 
        // Get the gun object in the child of this object
        Transform[] objs = GetComponentsInChildren<Transform>();
        foreach (Transform o in objs)
        {
            if (o.name == "GunChild")
                GunObj = o.gameObject;
        }

        myCtrl = GetComponent<CController>();


        if (myWeapon == null)
        {
            myWeapon = GetComponentInChildren<Weapon>();
            myWeapon.Owner = this;
        }
        else
        {
            myWeapon.Owner = this;
        }

        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myAudioSource = GetComponent<AudioSource>();

 
    }

    // Update is called once per frame
    void Update()
    {
        myInput();
        GunObject();
        Animations();

 
    }

    void GunObject()
    {
        // Do everything related to the gun object here (idealy)
        // Make the gun look at where your cursor is
        // At a rotation of 0 the gun points right 

        // Handle Gun 'animations' 
        // Where the cursor is in world space
        Vector3 CursorLoc = CamScript.CursorLocation;
        if (!myCtrl.Sprinting) // If we're not sprinting then the gun should rotate around the player relative to where the mouse is
        {
            // Now set up the rotating gun
            Vector3 toCursor = CursorLoc - transform.position; // This value will already have a 0'd y value :)
            toCursor = toCursor.normalized;
            // Alright now we need the angle between those two vectors and then rotate the object 
            GunObj.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(toCursor));
            // Flip the gun if it's on the left side of the player
            GunObj.GetComponentInChildren<SpriteRenderer>().flipY = (CursorLoc.x < transform.position.x);

            
            Vector3 pos = RootPosition;
            if (CursorLoc.z < transform.position.z)
                pos.z = -.01f;
            else
                pos.z = .01f;
            GunObj.transform.localPosition = pos;

        }
        else // If you're sprinting then loc the guns rotation at 20 degrees depending on which direction you're facing
        {
            if(myCtrl.Sprinting != myCtrl.SprintingPrev)
            {
                
            }
            //// Flip the gun if you're moving left
            GunObj.GetComponentInChildren<SpriteRenderer>().flipY = (myCtrl.Velocity.x < 0);

            GunObj.transform.rotation = Quaternion.Euler(0, 0, -88);
            GunObj.transform.localPosition = HolsteredPosition;

            Vector3 pos = GunObj.transform.localPosition;
            pos.z = .01f;
            GunObj.transform.localPosition = pos;
        }

    


    }


    void Animations()
    {
        // Handle Animations 
        Vector3 CursorLoc = CamScript.CursorLocation;

        // Handle looking left vs right via where the cursor is
        bool flip;
        if (myCtrl.Sprinting)
            flip = (myCtrl.Velocity.x < 0);
        else
            flip = (CursorLoc.x < transform.position.x);
        myRenderer.flipX = flip;

        // Handle looking up and down based on velocity
        myAnimator.SetBool("FaceFront", (CursorLoc.z < transform.position.z)); // Flips a bool switch based on if the cursor is above or below the character

       
        // If we're walking in a reverse direction we want to reverse our animation speed
        float spd = 1;

        if(!myCtrl.Sprinting)
        {
            // Handle negative speeds
            if (((myCtrl.Velocity.x > 0 && myRenderer.flipX) || (myCtrl.Velocity.x < 0 && !myRenderer.flipX)) && Mathf.Abs(myCtrl.Velocity.x) > Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;


            if (((myCtrl.Velocity.z > 0 && (CursorLoc.z < transform.position.z)) || (myCtrl.Velocity.z < 0 && (CursorLoc.z > transform.position.z))) && Mathf.Abs(myCtrl.Velocity.x) < Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;
        }



        // Handle walking and running bools
        myAnimator.SetFloat("Speed", spd);
        myAnimator.SetBool("Moving", (myCtrl.Velocity.magnitude > 1f));
        myAnimator.SetBool("Running", myCtrl.Sprinting);



    }

    void myInput()
    {
        if (Input.GetKey(KeyCode.A))
        {
            myCtrl.ApplyForce(Vector3.left * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
        }

        if (Input.GetKey(KeyCode.S))
        {
            myCtrl.ApplyForce(Vector3.back * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
        }

        if (Input.GetKey(KeyCode.D))
        {
            myCtrl.ApplyForce(Vector3.right * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
        }

        if (Input.GetKey(KeyCode.W))
        {
            myCtrl.ApplyForce(Vector3.forward * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
        }

        myCtrl.Sprinting = (Input.GetKey(KeyCode.LeftShift));


        if (Input.GetMouseButton(0) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)))
        {
            myWeapon.FireWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
        }

        if (Input.GetMouseButton(1) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)))
        {
            myWeapon.FireSecondary(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
        }


        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            myAudioSource.clip = AudioClips[0];
            myAudioSource.Play();
        }
 
        if (Input.GetKeyDown(KeyCode.R))
            myWeapon.ForceReload();
    }

    public void OnDeath()
    {

    }

    public void EnteredNewZone()
    {
        myWeapon.CurrentSecondaryClip = 0;
        
    }

    public void OnHit(Weapon _FromWhatWeapon)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
    }
    public UnitStruct MyUnit()
    {
        return myUnit;
    }

    public Weapon MyWeapon()
    {
        return myWeapon;
    }
}
