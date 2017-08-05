using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unit Script
/// This is the player script! It's what's used to control the player pretty much all the time
/// </summary>
[RequireComponent(typeof(CController))]
public class PlayerScript : MonoBehaviour, IUnit
{
    // References
    GameObject GunObj;
    CController myCtrl;
    Animator myAnimator;
    SpriteRenderer myRenderer;
    FootStepScript myFootStep;
    //public Weapon myWeapon; // The weapon is a seperate game object so we have to manually drop it in
    IWeapon myWeapon;
    public UnitStruct myUnit; 

    public bool UsingItem;

    Vector3 RootPosition = new Vector3(-0.138f, -0.138f, 0);
    Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
    Vector3 HolsteredRotation = new Vector3(0, 0, -88);

    AudioSource myAudioSource;
    public AudioClip[] AudioClips;

    ForceFieldScript myForceField;
    bool updatedForcefield;

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



        if (myWeapon == null)
        {
            myWeapon = GetComponentInChildren<IWeapon>();
            myWeapon.Owner = this;
        }
        else
        {
            myWeapon.Owner = this;
        }

        // Set up references
        myCtrl = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myAudioSource = GetComponent<AudioSource>();
        myFootStep = GetComponent<FootStepScript>();
        myForceField = GetComponentInChildren<ForceFieldScript>();

        updatedForcefield = false;
    }


    // Have to run everything through fixed update
    void FixedUpdate()
    {
        myInput();
        GunObject();
        Animations();

        if (myUnit.CurrentHealth < 0)
            OnDeath();

        if(!updatedForcefield)
        {
            updatedForcefield = true;
            UpdateForcefield();
        }
    }

    void GunObject()
    {
        // Do everything related to the gun object here (idealy)
        // Make the gun look at where your cursor is
        // At a rotation of 0 the gun points right 

        // Handle Gun 'animations' 
        // Where the cursor is in world space
        Vector3 CursorLoc = CamScript.CursorLocation;
        if (!myCtrl.Sprinting && !UsingItem) // If we're not sprinting then the gun should rotate around the player relative to where the mouse is
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

           
            Light gunShootFlare = GunObj.GetComponentInChildren<Light>();
            if(gunShootFlare != null)
                gunShootFlare.transform.localPosition = new Vector3(gunShootFlare.transform.localPosition.x, (CursorLoc.x < transform.position.x) ? -.2f : .2f, gunShootFlare.transform.localPosition.z);

        }
        else // If you're sprinting then loc the guns rotation at 20 degrees depending on which direction you're facing
        {

            //// Flip the gun if you're moving left
            GunObj.GetComponentInChildren<SpriteRenderer>().flipY = (myCtrl.Velocity.x < 0);

            GunObj.transform.rotation = Quaternion.Euler(HolsteredRotation);
            GunObj.transform.localPosition = HolsteredPosition;

            Vector3 pos = GunObj.transform.localPosition;

            if((myCtrl.Velocity.magnitude > 1f)) // If we're moving then always put it behind the player
            {
                pos.z = .01f;
                // If we're running primarily up then put it in front of the player sprite (behind the player, but towards the camera because we're running up)
                float zval = myCtrl.Velocity.z;
                float xval = myCtrl.Velocity.x;
                if (zval > 0 && Math.Abs(xval) < zval)
                    pos.z = -.01f;

            }else
            {
                if (UsingItem) // If we're using an item then put it behind the player
                    pos.z = .01f;
                else
                {
                    if (CursorLoc.z < transform.position.z) // Otherwise put it behind wherever the player is facing
                        pos.z = .01f;
                    else
                        pos.z = -.01f;
                }

            }

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
            flip = false;
        else
            flip = (CursorLoc.x < transform.position.x);
        myRenderer.flipX = flip;

        // Handle looking up and down based on velocity
        myAnimator.SetBool("FaceFront", (CursorLoc.z < transform.position.z)); // Flips a bool switch based on if the cursor is above or below the character


        // If we're walking in a reverse direction we want to reverse our animation speed
        float spd = 1;

     

        if (!myCtrl.Sprinting)
        {
            // Handle negative speeds
            if (((myCtrl.Velocity.x > 0 && myRenderer.flipX) || (myCtrl.Velocity.x < 0 && !myRenderer.flipX)) && Mathf.Abs(myCtrl.Velocity.x) > Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;


            if (((myCtrl.Velocity.z > 0 && (CursorLoc.z < transform.position.z)) || (myCtrl.Velocity.z < 0 && (CursorLoc.z > transform.position.z))) && Mathf.Abs(myCtrl.Velocity.x) < Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;
        }




        // Handle walking and running bools
        myAnimator.SetFloat("SpeedX", myCtrl.Velocity.x);
        myAnimator.SetFloat("SpeedY", myCtrl.Velocity.z);
        myAnimator.SetFloat("Speed", spd);
        myAnimator.SetBool("Moving", (myCtrl.Velocity.magnitude > 1f));
        myAnimator.SetBool("Running", myCtrl.Sprinting);

        if (myCtrl.Sprinting != myCtrl.SprintingPrev)
            myFootStep.stepCooldown = 0;

        myFootStep.Speed = (myCtrl.Sprinting) ? .2f : .45f;

    }

    void myInput()
    {
        // Basic Movement
        // This could look a lot nicer, but ultimately it gets the job done
        if (!myCtrl.Airborne && !UsingItem)
        {
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) // Left 
            {
                myCtrl.ApplyForce(Vector3.left * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Down
            {
                myCtrl.ApplyForce(Vector3.back * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) // Right
            {
                myCtrl.ApplyForce(Vector3.right * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Up
            {
                myCtrl.ApplyForce(Vector3.forward * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Down Left
            {
                myCtrl.ApplyForce(new Vector3(-.707f, 0, -.707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) // Down Right
            {
                myCtrl.ApplyForce(new Vector3(.707f, 0, -.707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Up Left
            {
                myCtrl.ApplyForce(new Vector3(-.707f, 0, .707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) // Up Right
            {
                myCtrl.ApplyForce(new Vector3(.707f, 0, .707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
        }


        // Set the springing bool equal to if we have the left shift key held down or not
        myCtrl.Sprinting = (Input.GetKey(KeyCode.LeftShift) && !UsingItem && !UpgradesManager.MenuOpen);

        // No shooting if the menu is open
        if (!MenuManager.MenuOpen && !DialogManager.InDialog && !UsingItem && !UpgradesManager.MenuOpen)
        {
            // Also no shooting if we're sprinting
            if (Input.GetMouseButton(0) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)))
            {
                myWeapon.FireWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
            }
            //// That goes for secondary fire as well
            //if (Input.GetMouseButton(1) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)))
            //{
            //    myWeapon.FireSecondary(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
            //}
        }


        // Handle sprinting
        if(myCtrl.Sprinting != myCtrl.SprintingPrev)
        {
            myAudioSource.clip = (myCtrl.Sprinting) ? AudioClips[0] : AudioClips[1];
            myAudioSource.Play();
        }

        // Handle reload
        if (Input.GetKeyDown(KeyCode.R))
            myWeapon.ForceReload();

        // Handle using a healthkit
        //if(Input.GetKeyDown(KeyCode.Q) && !UsingItem && GameManager.HealthKits > 0 && myUnit.CurrentHealth != myUnit.MaxHealth)
        //{
        //    myAnimator.SetFloat("Special", 1);
        //    UsingItem = true;
        //    MenuManager.instance.ShowHealthkit();
        //    myCtrl.Velocity = Vector3.zero;
        //    StartCoroutine(UseItemCRT());
        //}
    }

    public void OnDeath()
    {
        // This needs some work
        GameManager.instance.LoadLastSaveFile();
    }

    public void EnteredNewZone()
    {
        // Nothing 
        // This used to reset secondary ammo. We're no longer using secondary ammo, therefore
        // No dice.
        // Look to delete this method eventually
    }

    public void OnHit(IWeapon _FromWhatWeapon)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myVisualizer.ShowMenu();

        if (myForceField)
        {
            if (myForceField.Health > 0)
                myForceField.RegisterHit(_FromWhatWeapon.myWeaponInfo.bulletDamage);
            else
                myUnit.CurrentHealth -= _FromWhatWeapon.myWeaponInfo.bulletDamage;
        }
        else
            myUnit.CurrentHealth -= _FromWhatWeapon.myWeaponInfo.bulletDamage;
    }
    public UnitStruct MyUnit
    {
        get { return myUnit; }
    }

    public IWeapon MyWeapon
    {
        get { return myWeapon; }
    }


    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }


    void OnParticleCollision(GameObject other)
    {
        // I aactually have no idea how this shit works
        Destroy(other.gameObject);
    }

    IEnumerator UseItemCRT()
    {
        // Show the health visualizer
        myVisualizer.ShowMenu();
        yield return new WaitForSeconds(2);
        GameManager.HealthKits--; // Take away a health kit

        myUnit.CurrentHealth += 35; // Add the health
        if (myUnit.CurrentHealth > myUnit.MaxHealth)
            myUnit.CurrentHealth = myUnit.MaxHealth;

        // Ensure that we can see it
        myVisualizer.ShowMenu();

        // Exit out of the animation
        myAnimator.SetFloat("Special", 0);
        UsingItem = false;
    }

    public void UpdateForcefield()
    { 
        Upgrades myUpgrades = GameManager.instance.UpgradeData;

        float ForcefieldHealth = 0;
        float RegenSpeed = 0;

        if(myUpgrades.HasShield)
        {
            ForcefieldHealth += 20;
            RegenSpeed += 33;

            for(int i = 0; i < myUpgrades.ShieldHealth.Length; i ++)
            {
                ForcefieldHealth += 10;
            }

            for(int i = 0; i < myUpgrades.ShieldRegen.Length; i++)
            {
                RegenSpeed += 6;
            }

            
        }

        myForceField.Health = ForcefieldHealth;
        myForceField.MaxHealth = (int)ForcefieldHealth;
        myForceField.RegenTime = RegenSpeed;

        myVisualizer.ShowMenu();

    }
}
