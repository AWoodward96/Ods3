using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unit Script
/// This is the player script! It's what's used to control the player pretty much all the time
/// </summary>
[RequireComponent(typeof(CController))]
public class PlayerScript : MonoBehaviour, IMultiArmed
{
    // References
    CController myCtrl;
    Animator myAnimator;
    SpriteRenderer myRenderer;
    FootStepScript myFootStep;
    public UnitStruct myUnit;

    ForceFieldScript myForceField;
    bool updatedForcefield;

    //Vector3 RootPosition = new Vector3(-.138f, -0.138f, 0); // The center of the player (where it would look like he's holding the weapon) is at this position, so we rotate everything around it
    //public Vector3 RootPosition = new Vector3(0, -.138f, 0); // The center of the player (where it would look like he's holding the weapon) is at this position, so we rotate everything around it
    Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
    Vector3 HolsteredRotation = new Vector3(0, 0, -88);
    Vector3 HolseredRotation2 = new Vector3(0, 0, -119);


    public bool InCombat;
    public bool Rolling;
    public int RollSpeed;

    Vector3 rollingDirection;
    int rollSoftCD; // the cooldown so you can't spam the roll
    bool movementType = false; // False:Sprint; True:Roll
    float combatCD;

    AudioSource myAudioSource;
    public AudioClip[] AudioClips;

    public bool AcceptInput; 

    public WeaponBase PrimaryWeapon;
    public WeaponBase SecondaryWeapon;
    public WeaponBase ActiveWeapon;

    // Use this for initialization
    void Awake()
    {
        // Set up references
        myCtrl = GetComponent<CController>();
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myAudioSource = GetComponent<AudioSource>();
        myFootStep = GetComponent<FootStepScript>();
        myForceField = GetComponentInChildren<ForceFieldScript>();

        updatedForcefield = false;
    }

    private void Start()
    {
        
    }


    // Have to run everything through fixed update
    void FixedUpdate()
    {
        if (AcceptInput)
            myInput();

        GunObject();
        Animations();

        if (myUnit.CurrentHealth < 0)
            OnDeath();

        if (!updatedForcefield)
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
        if (!Armed)
            return;

        // Where the cursor is in world space
        Vector3 CursorLoc = CamScript.CursorLocation;

        if (SecondaryWeapon == null)
        {
            ActiveWeapon = PrimaryWeapon;
        }

        GameObject rotateMe = ActiveWeapon.RotateObject;
        WeaponBase holsterMe = null;
        GameObject holsterObj = null;
        if(PrimaryWeapon != null && SecondaryWeapon != null)
        {
            holsterMe = (ActiveWeapon == PrimaryWeapon) ? SecondaryWeapon : PrimaryWeapon;
            holsterObj = (ActiveWeapon == PrimaryWeapon) ? SecondaryWeapon.RotateObject : PrimaryWeapon.RotateObject;
        }

        // Handle holster weapon animations
        if (holsterMe != null)
        {
            //// Flip the gun if you're moving left
            holsterObj.GetComponent<SpriteRenderer>().flipY = (myCtrl.Velocity.x < 0);

            holsterObj.transform.rotation = Quaternion.Euler(HolsteredRotation);
            holsterMe.transform.localPosition = HolsteredPosition;

            Vector3 pos = holsterMe.transform.localPosition;

            if ((GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f)) // If we're moving then always put it behind the player
            {
                pos.z = .01f;
                // If we're running primarily up then put it in front of the player sprite (behind the player, but towards the camera because we're running up)
                float zval = myCtrl.Velocity.z;
                float xval = myCtrl.Velocity.x;
                if (zval > 0 && Math.Abs(xval) < zval)
                    pos.z = -.01f;

            }
            else
            {

                if (CursorLoc.z < transform.position.z) // Otherwise put it behind wherever the player is facing
                    pos.z = .01f;
                else
                    pos.z = -.01f;

            }

            holsterMe.transform.localPosition = pos;
        }


        // Handle Gun 'animations' 
        if (!myCtrl.Sprinting && InCombat) // If we're not sprinting then the gun should rotate around the player relative to where the mouse is
        {

            Vector3 pos = Vector3.zero;
            if (CursorLoc.z < transform.position.z)
                pos.z = -.01f;
            else
                pos.z = .01f;
            ActiveWeapon.transform.localPosition = pos;
             
            if(ActiveWeapon.heldData.holdType == HeldWeapon.HoldType.Hold)
            {
                rotateMe.transform.rotation = Quaternion.identity;
                rotateMe.GetComponentInChildren<SpriteRenderer>().flipY = false;
                return;
            }


            // Now set up the rotating gun
            Vector3 toCursor = GlobalConstants.ZeroYComponent(CursorLoc - transform.position); // This value will already have a 0'd y value :)
            toCursor = toCursor.normalized;
            // Alright now we need the angle between those two vectors and then rotate the object 
            rotateMe.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(toCursor));
            // Flip the gun if it's on the left side of the player
            rotateMe.GetComponentInChildren<SpriteRenderer>().flipY = (CursorLoc.x < transform.position.x);
        }
        else // If you're sprinting then loc the guns rotation at 20 degrees depending on which direction you're facing
        {
            Vector3 pos = HolsteredPosition;

            if ((GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f)) // If we're moving then always put it behind the player
            {
                pos.z = .01f;
                // If we're running primarily up then put it in front of the player sprite (behind the player, but towards the camera because we're running up)
                float zval = myCtrl.Velocity.z;
                float xval = myCtrl.Velocity.x;
                if (zval > 0 && Math.Abs(xval) < zval)
                    pos.z = -.01f;

            }
            else
            {

                if (CursorLoc.z < transform.position.z) // Otherwise put it behind wherever the player is facing
                    pos.z = .01f;
                else
                    pos.z = -.01f;

            }

            ActiveWeapon.transform.localPosition = pos;

            if (ActiveWeapon.heldData.holdType == HeldWeapon.HoldType.Hold)
            {
                rotateMe.transform.rotation = Quaternion.identity;
                rotateMe.GetComponentInChildren<SpriteRenderer>().flipY = false;
                return;
            }


            //// Flip the gun if you're moving left
            rotateMe.GetComponentInChildren<SpriteRenderer>().flipY = (myCtrl.Velocity.x < 0);
            rotateMe.transform.localRotation = Quaternion.Euler((PrimaryWeapon == ActiveWeapon) ? HolsteredRotation : HolseredRotation2);

        }


    }


    // Handle Animations 
    void Animations()
    {
        // First get a reference to the cursor location
        Vector3 CursorLoc = CamScript.CursorLocation;

        // Based on the cursor location, make the player look in that direction
        // Handle looking left vs right via where the cursor is
        bool flip;
        if (myCtrl.Sprinting || !InCombat)
            flip = false;
        else
            flip = (CursorLoc.x < transform.position.x);
        myRenderer.flipX = flip;

        // Handle looking up and down based on velocity
        myAnimator.SetBool("FaceFront", (CursorLoc.z < transform.position.z)); // Flips a bool switch based on if the cursor is above or below the character


        // If we're walking in a reverse direction we want to reverse our animation speed
        float spd = 1;  // This float controls that speed. To be clear, this is the animation speed, not the speed of the player. A - speed indicates an animation playing in reverse.
        if (!myCtrl.Sprinting)
        {
            // Handle negative speeds
            if (((myCtrl.Velocity.x > 0 && myRenderer.flipX) || (myCtrl.Velocity.x < 0 && !myRenderer.flipX)) && Mathf.Abs(myCtrl.Velocity.x) > Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;

            if (((myCtrl.Velocity.z > 0 && (CursorLoc.z < transform.position.z)) || (myCtrl.Velocity.z < 0 && (CursorLoc.z > transform.position.z))) && Mathf.Abs(myCtrl.Velocity.x) < Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;
        }




        // Handle walking and running bools
        Vector3 toCursor = GlobalConstants.ZeroYComponent( CamScript.CursorLocation - transform.position);
        myAnimator.SetFloat("SpeedX", myCtrl.Velocity.x);
        myAnimator.SetFloat("SpeedY", myCtrl.Velocity.z);
        myAnimator.SetFloat("LookX", toCursor.x);
        myAnimator.SetFloat("LookY", toCursor.z);
        myAnimator.SetFloat("Speed", spd);
        myAnimator.SetBool("Moving", (GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f));
        myAnimator.SetBool("Running", myCtrl.Sprinting);
        myAnimator.SetBool("InCombat", InCombat);

        // Footstep data
        if (myCtrl.Sprinting != myCtrl.SprintingPrev)
            myFootStep.stepCooldown = 0;

        myFootStep.Speed = (myCtrl.Sprinting) ? .2f : ((InCombat) ? .45f : .35f);
 
        combatCD += Time.deltaTime;
        if (combatCD > 5 && InCombat)
        {
            if (ZoneScript.ActiveZone.ZoneAggression != ZoneScript.AggressionType.OnlyCombat)
            {
                InCombat = false;
                myAudioSource.clip = AudioClips[1];
                myAudioSource.Play();
            }
        }

    }

    void myInput()
    {
        // Basic Movement
        // This could look a lot nicer, but ultimately it gets the job done
        if (!myCtrl.Airborne && !Rolling)
        {
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) // Left                                        Why does this section look so terrible?
            {
                myCtrl.ApplyForce(Vector3.left * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Down                                   As in why is it an else-if tree?
            {
                myCtrl.ApplyForce(Vector3.back * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) // Right                                  Well it fixes a problem when it comes to holding --
            {
                myCtrl.ApplyForce(Vector3.right * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Up                                     -- two keys a the same time. W/o this, if you were --
            {
                myCtrl.ApplyForce(Vector3.forward * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Down Left                               -- to hold down W and A at the same time your net --
            {
                myCtrl.ApplyForce(new Vector3(-.707f, 0, -.707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) // Down Right                              -- speed in that direction would be faster then just --
            {
                myCtrl.ApplyForce(new Vector3(.707f, 0, -.707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // Up Left                                 -- holding down one or the other. This felt wrong for --
            {
                myCtrl.ApplyForce(new Vector3(-.707f, 0, .707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) // Up Right                                -- this type of game, hense the if-else tree.
            {
                myCtrl.ApplyForce(new Vector3(.707f, 0, .707f) * (myCtrl.Speed + (myCtrl.Sprinting ? myCtrl.SprintSpeed : 0)));
            }
        }


        // Set the springing bool equal to if we have the left shift key held down or not
        if(!movementType)
            myCtrl.Sprinting = (Input.GetKey(KeyCode.LeftShift) && !UpgradesManager.MenuOpen);

        // No shooting if the menu is open
		if (!MenuManager.MenuOpen && !UpgradesManager.MenuOpen)
        {
            // No shooting when we're in a no combat zone
            if (ZoneScript.ActiveZone.ZoneAggression != ZoneScript.AggressionType.NoCombat)
            {
                // Also no shooting if we're sprinting
                if (Input.GetMouseButton(0) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)) && Armed)
                {
                    if (!Rolling)
                    { 
                        WeaponBase weaponToFire = (ActiveWeapon == PrimaryWeapon) ? PrimaryWeapon : SecondaryWeapon;
                        weaponToFire.FireWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
                        combatCD = 0;
 
                        if (!InCombat)
                        {
                            myAudioSource.clip = AudioClips[0];
                            myAudioSource.Play();
                            combatCD = 0;
                            InCombat = true;
                        }
                    }
                }
            }
        }

        handleRolling();

        // Handle sprinting
        if (InCombat && myCtrl.Sprinting != myCtrl.SprintingPrev)
        {
            myAudioSource.clip = (myCtrl.Sprinting) ? AudioClips[0] : AudioClips[1];
            myAudioSource.Play();
        }

        // Handle reload
        if (Input.GetKeyDown(KeyCode.R) && Armed)
        {
			// Is reload code even necessary if we're using an energy system?

            /*WeaponBase weaponToReload = (ActiveWeapon == PrimaryWeapon) ? PrimaryWeapon : SecondaryWeapon;
            weaponToReload.ForceReload();*/
        }

        if (Input.GetMouseButtonDown(1)) // When we left click throw our weapon
        {
            TossWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
        }
 
    }

    public void TossWeapon(Vector3 _dir)
    {
        if (ActiveWeapon == null)
            return;

        ActiveWeapon.heldData.Toss(_dir, transform.position);
        if (SecondaryWeapon == ActiveWeapon)
        {
            // Call the toss method
            ActiveWeapon = PrimaryWeapon;
            SecondaryWeapon = null;
        } else if(PrimaryWeapon == ActiveWeapon)
        {
            ActiveWeapon = SecondaryWeapon;
            PrimaryWeapon = null;
        }

        myVisualizer.BuildAmmoBar();

     
    }

    public void OnDeath()
    {
        // This needs some work
        // 
        GameObject obj = Resources.Load("Prefabs/Particles/playerDeath") as GameObject;
        Instantiate(obj, transform.position, obj.transform.rotation); 
        this.gameObject.SetActive(false);

    }

 
    public void EnteredNewZone()
    {
        if (ZoneScript.ActiveZone.ZoneAggression == ZoneScript.AggressionType.NoCombat)
        {
            InCombat = false;
        }
    }

    void handleRolling()
    {
        rollSoftCD++;
 
        if (!Rolling && Input.GetKeyDown(KeyCode.Space) && movementType && rollSoftCD > 40 && myCtrl.canMove)
        {
            Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            dir = dir.normalized;
            rollSoftCD = 0;
            rollingDirection = dir;
            Rolling = true;
            myCtrl.Velocity = rollingDirection * RollSpeed;
            myAnimator.SetTrigger("Rolling");
            StopAllCoroutines();
            StartCoroutine(RollCD());
        }

        if (Rolling)
        {
            myCtrl.ApplyForce(rollingDirection * RollSpeed);
        }
    }

    IEnumerator RollCD()
    {
        yield return new WaitForFixedUpdate();
        Rolling = false;
    }

    public void OnHit(int _damage)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myVisualizer.ShowMenu();
        if (!InCombat)
        {
            myAudioSource.clip = AudioClips[0];
            myAudioSource.Play();
            combatCD = 0;
            InCombat = true;
        }

        if (myForceField)
        {
			if (myUnit.CurrentEnergy > 0)
                myForceField.RegisterHit(_damage);
            else
            {
                myAudioSource.clip = AudioClips[2];
                myAudioSource.Play();
                myUnit.CurrentHealth -= _damage;
            }
        }
        else
        {
            myAudioSource.clip = AudioClips[2];
            myAudioSource.Play();
            myUnit.CurrentHealth -= _damage;
        }
    }


    public HealthBar myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthBar>();
        }
    }

    WeaponBase IArmed.myWeapon
    {
        get
        {
            if (PrimaryWeapon != null || SecondaryWeapon != null)
                return (ActiveWeapon == PrimaryWeapon) ? PrimaryWeapon : SecondaryWeapon;
            else
                return null;
        }
    }


    public bool Triggered
    {
        get
        {
            // This is throw away here.
            // The player will never be 'triggered'
            return false;
        }

        set
        {
            // blank
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

    public void UpdateForcefield()
    {
        Upgrades myUpgrades = GameManager.instance.UpgradeData;

        float ForcefieldHealth = 0;
        float RegenSpeed = 0;

        if (myUpgrades.HasShield)
        {
            ForcefieldHealth += 20;
            RegenSpeed += 33;

            for (int i = 0; i < myUpgrades.ShieldHealth.Length; i++)
            {
                ForcefieldHealth += 10;
            }

            for (int i = 0; i < myUpgrades.ShieldRegen.Length; i++)
            {
                RegenSpeed += 6;
            }


        }

		//myUnit.CurrentEnergy = (int)ForcefieldHealth;
		//myUnit.MaxEnergy = (int)ForcefieldHealth;
        //myForceField.RegenTime = RegenSpeed;

        myVisualizer.ShowMenu();

    }

    public void Activate()
    {
        // Nothing in the player needs to be activated
        // Except maybe saving location data? Hmm
    }

    
    public void PickUpWeapon(WeaponBase _newWeapon)
    {
        // First get a reference to the weapon itself
        if (_newWeapon == null)
            return;

        combatCD = 0;
        InCombat = true;
        _newWeapon.myOwner = this;
		_newWeapon.myShield = GetComponentInChildren<ForceFieldScript>();
		_newWeapon.myEnergy = GetComponent<EnergyManager>();

        if (PrimaryWeapon == null)
        {
            PrimaryWeapon = _newWeapon; 
            ActiveWeapon = PrimaryWeapon;
            myVisualizer.BuildAmmoBar(); // Let the visualizer 
            return;
        }


        if (PrimaryWeapon != null && SecondaryWeapon == null)
        {
            SecondaryWeapon = _newWeapon; 
            ActiveWeapon = SecondaryWeapon;
            myVisualizer.BuildAmmoBar(); // Let the visualizer 
            return;
        }

        // If we're full, toss the weapon that we're currently holding and replace it
        if (PrimaryWeapon != null && SecondaryWeapon != null)
        {
            ActiveWeapon.myOwner = null;
            ActiveWeapon.heldData.Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position), transform.position);
            // Toss the current active weapon if you're already carrying 2 things 
            if (ActiveWeapon == PrimaryWeapon)
            {
                PrimaryWeapon = _newWeapon;
                ActiveWeapon = PrimaryWeapon;
                myVisualizer.BuildAmmoBar();
                return;
            }

            if (ActiveWeapon == SecondaryWeapon)
            { 
                SecondaryWeapon = _newWeapon;
                ActiveWeapon = SecondaryWeapon;
                myVisualizer.BuildAmmoBar();
                return;
            }

        }
        
    }

    public void SetWeaponsOwner(WeaponBase w)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A forced removal of a weapon that's either lost its use or isn't avaible anymore
    /// </summary>
    public void RemoveWeapon(WeaponBase _Base)
    {
        if (_Base == PrimaryWeapon)
        { PrimaryWeapon = null; ActiveWeapon = SecondaryWeapon; }

        if (_Base == SecondaryWeapon)
        { SecondaryWeapon = null; ActiveWeapon = PrimaryWeapon; }

        if(ActiveWeapon != null)
            ActiveWeapon.ResetShootCD();

        myVisualizer.BuildAmmoBar();
    }

    bool Armed
    {
        get { return (PrimaryWeapon != null || SecondaryWeapon != null); }
    }


    public ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
