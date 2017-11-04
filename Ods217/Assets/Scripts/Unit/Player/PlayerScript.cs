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

    Vector3 RootPosition = new Vector3(-0.138f, -0.138f, 0); // The center of the player (where it would look like he's holding the weapon) is at this position, so we rotate everything around it
    Vector3 HolsteredPosition = new Vector3(0, .5f, 0);
    Vector3 HolsteredRotation = new Vector3(0, 0, -88);
    Vector3 HolseredRotation2 = new Vector3(0, 0, 90);


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


    IWeapon primaryWeapon;
    IWeapon secondaryWeapon;
    GameObject secondaryGunWeapon;
    GameObject primaryGunWeapon;
    GameObject activeWeapon;

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
        // Get the gun object in the child of this object 
        usableWeapon[] weapons = GetComponentsInChildren<usableWeapon>();
        if (weapons.Length > 0)
        {
            if (weapons[0] != null)
            {
                weapons[0].PickedUp(); 
            }
        }

        if (weapons.Length > 1)
        {
            if (weapons[1] != null)
            {
                weapons[1].PickedUp(); 
            }

        }

        if(activeWeapon == null)
        {
            myVisualizer.BuildAmmoBar();
        }
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

        if (secondaryWeapon == null)
        {
            activeWeapon = primaryGunWeapon;
        }

        // Handle Primary weapon animations
        if (activeWeapon != primaryGunWeapon)
        {
            //// Flip the gun if you're moving left
            primaryGunWeapon.GetComponentInChildren<SpriteRenderer>().flipY = (myCtrl.Velocity.x < 0);

            primaryGunWeapon.transform.rotation = Quaternion.Euler(HolsteredRotation);
            primaryGunWeapon.transform.localPosition = HolsteredPosition;

            Vector3 pos = primaryGunWeapon.transform.localPosition;

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

            primaryGunWeapon.transform.localPosition = pos;
        }


        // Handle Gun 'animations' 
        if (!myCtrl.Sprinting && InCombat) // If we're not sprinting then the gun should rotate around the player relative to where the mouse is
        {
            // Now set up the rotating gun
            Vector3 toCursor = CursorLoc - transform.position; // This value will already have a 0'd y value :)
            toCursor = toCursor.normalized;
            // Alright now we need the angle between those two vectors and then rotate the object 
            activeWeapon.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(toCursor));
            // Flip the gun if it's on the left side of the player
            activeWeapon.GetComponentInChildren<SpriteRenderer>().flipY = (CursorLoc.x < transform.position.x);


            Vector3 pos = RootPosition;
            if (CursorLoc.z < transform.position.z)
                pos.z = -.01f;
            else
                pos.z = .01f;
            activeWeapon.transform.localPosition = pos;


            Light gunShootFlare = activeWeapon.GetComponentInChildren<Light>();
            if (gunShootFlare != null)
                gunShootFlare.transform.localPosition = new Vector3(gunShootFlare.transform.localPosition.x, (CursorLoc.x < transform.position.x) ? -.2f : .2f, gunShootFlare.transform.localPosition.z);

        }
        else // If you're sprinting then loc the guns rotation at 20 degrees depending on which direction you're facing
        {
            //// Flip the gun if you're moving left
            activeWeapon.GetComponentInChildren<SpriteRenderer>().flipY = (myCtrl.Velocity.x < 0);

            activeWeapon.transform.rotation = Quaternion.Euler(HolsteredRotation);
            activeWeapon.transform.localPosition = ((primaryGunWeapon == activeWeapon) ? HolsteredPosition : HolseredRotation2);

            Vector3 pos = activeWeapon.transform.localPosition;

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

            activeWeapon.transform.localPosition = pos;

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
        myAnimator.SetFloat("SpeedX", myCtrl.Velocity.x);
        myAnimator.SetFloat("SpeedY", myCtrl.Velocity.z);
        myAnimator.SetFloat("Speed", spd);
        myAnimator.SetBool("Moving", (GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f));
        myAnimator.SetBool("Running", myCtrl.Sprinting);

        // Footstep data
        if (myCtrl.Sprinting != myCtrl.SprintingPrev)
            myFootStep.stepCooldown = 0;

        myFootStep.Speed = (myCtrl.Sprinting) ? .2f : .45f;


        // Handle in combat and out of combat
        myAnimator.SetLayerWeight(0, Convert.ToInt32(InCombat));
        myAnimator.SetLayerWeight(1, Convert.ToInt32(!InCombat));

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
        if (!MenuManager.MenuOpen && !DialogManager.InDialog && !UpgradesManager.MenuOpen)
        {
            // No shooting when we're in a no combat zone
            if (ZoneScript.ActiveZone.ZoneAggression != ZoneScript.AggressionType.NoCombat)
            {
                // Also no shooting if we're sprinting
                if (Input.GetMouseButton(0) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)) && Armed)
                {
                    if (!Rolling)
                    {
                        IWeapon weaponToFire = (activeWeapon == primaryGunWeapon) ? primaryWeapon : secondaryWeapon;
                        weaponToFire.FireWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
                        combatCD = 0;
                        if (!InCombat)
                        {
                            myAudioSource.clip = AudioClips[0];
                            myAudioSource.Play();
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
            IWeapon weaponToReload = (activeWeapon == primaryGunWeapon) ? primaryWeapon : secondaryWeapon;
            weaponToReload.ForceReload();
        }

        if (Input.GetMouseButtonDown(1)) // When we left click throw our weapon
        {
            TossWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
        }

    }

    public void TossWeapon(Vector3 _dir)
    {
        if (activeWeapon == null)
            return;

        if (secondaryGunWeapon == activeWeapon)
        {
            secondaryGunWeapon.transform.parent = null;
            secondaryGunWeapon.transform.position = transform.position;
            secondaryGunWeapon.GetComponent<usableWeapon>().Toss(_dir, transform.position);

            secondaryWeapon = null;
            secondaryGunWeapon = null;

           
            if (primaryGunWeapon != null)
                activeWeapon = primaryGunWeapon;

            myVisualizer.BuildAmmoBar();
        }
        else if (primaryGunWeapon == activeWeapon)
        {
            primaryGunWeapon.transform.parent = null;
            primaryGunWeapon.transform.position = transform.position;
            primaryGunWeapon.GetComponent<usableWeapon>().Toss(_dir, transform.position);

            primaryWeapon = null;
            primaryGunWeapon = null;

            if (secondaryGunWeapon != null)
                activeWeapon = secondaryGunWeapon;

            myVisualizer.BuildAmmoBar();
        }
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.LogAssertion("Mode Changed to Sprint");
            myCtrl.Speed = .75f;
            movementType = false;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.LogAssertion("Roll mode changed to: Movement Direction");
            myCtrl.Speed = 1.3f;
            movementType = true;
        }


        rollSoftCD++;

        //if (!Rolling && Input.GetKeyDown(KeyCode.Space) && !movementType && rollSoftCD > 40 && myCtrl.canMove)
        //{
        //    Vector3 dir = GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position);
        //    dir = dir.normalized;
        //    rollingDirection = dir;
        //    Rolling = true;
        //    myCtrl.Velocity = rollingDirection * RollSpeed;
        //    rollSoftCD = 0;
        //    myAnimator.SetTrigger("Rolling");
        //    StopAllCoroutines();
        //    StartCoroutine(RollCD());
        //}

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
            InCombat = true;
        }

        if (myForceField)
        {
            if (myForceField.Health > 0)
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

    IWeapon IArmed.myWeapon
    {
        get
        {
            if (primaryWeapon != null || secondaryWeapon != null)
                return (activeWeapon == primaryGunWeapon) ? primaryWeapon : secondaryWeapon;
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

        myForceField.Health = ForcefieldHealth;
        myForceField.MaxHealth = (int)ForcefieldHealth;
        myForceField.RegenTime = RegenSpeed;

        myVisualizer.ShowMenu();

    }

    public void Activate()
    {
        // Nothing in the player needs to be activated
        // Except maybe saving location data? Hmm
    }

    // This method is hell garbage, I'm very tired
    public void PickUpWeapon(GameObject _newWeapon)
    {
        // First get a reference to the weapon itself
        IWeapon newRef = _newWeapon.GetComponentInChildren<IWeapon>();
        if (newRef == null)
            return;

        combatCD = 0;
        InCombat = true;

        if (primaryWeapon == null)
        {
            SetPrimary(newRef, _newWeapon);
            myVisualizer.BuildAmmoBar(); // Let the visualizer 
            return;
        }


        if (primaryWeapon != null && secondaryWeapon == null)
        {
            SetSecondary(newRef, _newWeapon);
            myVisualizer.BuildAmmoBar(); // Let the visualizer 
            return;
        }

        // If we full toss the weapon that we're currently holding and replace it
        if (primaryWeapon != null && secondaryWeapon != null)
        {
            // Toss the current active weapon if you're already carrying 2 things 
            if (activeWeapon == secondaryGunWeapon)
            {
                secondaryGunWeapon.transform.parent = null;
                secondaryGunWeapon.transform.position = transform.position;
                secondaryGunWeapon.GetComponent<usableWeapon>().Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position),transform.position);
                SetSecondary(newRef, _newWeapon);
                myVisualizer.BuildAmmoBar();
                return;
            }

            if (activeWeapon == secondaryGunWeapon)
            {
                secondaryGunWeapon.transform.parent = null;
                secondaryGunWeapon.transform.position = transform.position;
                secondaryGunWeapon.GetComponent<usableWeapon>().Toss(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position),transform.position);
                SetPrimary(newRef, _newWeapon);
                myVisualizer.BuildAmmoBar();
                return;
            }
        }

    }

    void SetSecondary(IWeapon newRef, GameObject _newWeapon)
    {
        //  If we already have one weapon and can pick up another
        secondaryGunWeapon = _newWeapon; // Set the secondary equal to the new weapon
        secondaryGunWeapon.transform.parent = transform; // Set it's transform to the parent
        secondaryGunWeapon.transform.position = Vector3.zero; // Set it's position to 0
        secondaryWeapon = newRef; // Set the iweapon reference to the new iweapon
        secondaryWeapon.Owner = this; // Set it's owner
        _newWeapon.transform.localScale = new Vector3(1, 1, 1); // Make sure it's the right scale
        activeWeapon = _newWeapon; // If you picked it up chances are you want it equiped
    }

    void SetPrimary(IWeapon newRef, GameObject _newWeapon)
    {
        //  If we already have one weapon and can pick up another
        primaryGunWeapon = _newWeapon; // Set the secondary equal to the new weapon
        primaryGunWeapon.transform.parent = transform; // Set it's transform to the parent
        primaryGunWeapon.transform.position = Vector3.zero; // Set it's position to 0
        primaryWeapon = newRef; // Set the iweapon reference to the new iweapon
        primaryWeapon.Owner = this; // Set it's owner
        _newWeapon.transform.localScale = new Vector3(1, 1, 1); // Make sure it's the right scale
        activeWeapon = _newWeapon; // If you picked it up chances are you want it equiped
    }

    bool Armed
    {
        get { return (primaryWeapon != null || secondaryWeapon != null); }
    }
}
