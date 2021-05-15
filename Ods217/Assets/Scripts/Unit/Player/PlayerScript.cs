using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// A Unit Script
/// This is the player script! It's what's used to control the player pretty much all the time
/// </summary>
[RequireComponent(typeof(CController))]
public class PlayerScript : MonoBehaviour, IMultiArmed, ISavable, IPawn
{
    const float ZFRONT = -0.01f;
    const float ZBACK = 0.01f;

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
    Vector3 HolsteredRotation2 = new Vector3(0, 0, -119);

    public Vector3 LookingVector;



    public bool InCombat; 
    public bool Punching;
	public int punchDamage = 5;
	public float punchCD = 0.33f;
    [HideInInspector]
    public float combatCD;

    AudioSource myAudioSource;
    public AudioClip[] AudioClips;

	public bool Stunned;
    public bool AcceptInput;
    bool UsingItem;

    public WeaponBase PrimaryWeapon;
    public WeaponBase SecondaryWeapon;
    public WeaponBase ActiveWeapon;

    public GameObject vis;

	[Header("ISavable Variables")]
	public int saveID = -1;

	[HideInInspector]
	public bool saveIDSet = false;

	public int SaveID
	{
		get
		{
			return saveID;
		}
		set
		{
			saveID = value;
		}
	}

	public bool SaveIDSet
	{
		get
		{
			return saveIDSet;
		}
		set
		{
			saveIDSet = value;
		}
	}

    MovementAI moveAI;

	EffectSystem myEmojis;
    PlayerWeaponAnim GunAnimations;


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
		myEmojis = GetComponentInChildren<EffectSystem>();

        moveAI = new MovementAI(this.gameObject, myCtrl);

        updatedForcefield = false;
		Stunned = false;
    }

    // Have to run everything through fixed update
    void FixedUpdate()
    {
        if (AcceptInput && !UsingItem)
        {
            myFixedInput();
            moveAI.ActionComplete = true; // no moving me anymore
        }
        else
            moveAI.Update();

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

	// Put input code here
	// There's a chance that FixedUpdate will duplicate the input, causing unintended effects
	void Update()
	{
		if (AcceptInput && !UsingItem && !Punching)
			myInput();
	}

    void GunObject()
    {
        // Do everything related to the gun object here (idealy)
        // Make the gun look at where your cursor is
        // At a rotation of 0 the gun points right 
        if (!Armed)
            return;
         

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
            HandleHolsteredWeapon(holsterMe.gameObject, holsterObj, false); 
        }

        HandleActiveWeapon(rotateMe);

    }

    /// <summary>
    /// Handles the animations for the weapon object that is not actively in your hands
    /// </summary>
    /// <param name="_parentObject">The object that the weapons script is on. This object is in charge of if the weapon is behind or in front of the player.</param>
    /// <param name="_weaponObject">The ojbect that is 'in the players hands'. This object is the one that rotates based on where you're looking. In this case it should be in charge of its holstered rotation.</param>
    /// <param name="_activeOverride">If true, then we need to mess with some of the constants so we dont have planar overlap</param>
    void HandleHolsteredWeapon(GameObject _parentObject, GameObject _weaponObject, bool _activeOverride)
    {
        float localZBACK = ZBACK + ((_activeOverride) ? .001f : 0f);
        float localZFRONT = ZFRONT + ((_activeOverride) ? -.001f : 0f); ;

        WeaponBase Base = _parentObject.GetComponent<WeaponBase>();
        OverrideHolsteredPos overrideHolster = _weaponObject.GetComponent<OverrideHolsteredPos>(); 

        // Effect the sprite only if you are not holdType hold.
        // Otherwise this info would be controlled on a case by case basis
        if (Base.heldData.holdType != HeldWeapon.HoldType.Hold)
        {
            //// Flip the gun if you're moving left
            bool willFlip = (myCtrl.Velocity.x < 0);
            if (overrideHolster != null)
                willFlip = (overrideHolster.LockYFlip) ? false : willFlip;

            _weaponObject.GetComponent<SpriteRenderer>().flipY = willFlip;


            Vector3 selectedrotation = (_activeOverride) ? HolsteredRotation2 : HolsteredRotation;
            _weaponObject.transform.rotation = Quaternion.Euler((overrideHolster == null) ? selectedrotation : overrideHolster.newRotation); // If override exists then use the override, otherwise use the selected preset rotations
        }

        _parentObject.transform.localPosition = (overrideHolster == null) ? HolsteredPosition : overrideHolster.newPosition;

        Vector3 pos = _parentObject.transform.localPosition;

        // First check if we're punching. Because if we're punching then that's that.
        if(Punching)
        { 
            pos.z = localZBACK; 
            _parentObject.transform.localPosition = pos;
            return;
        }


        // First check if we're aggressive or not
        if (InCombat && !myCtrl.Sprinting)
        {
            // If we're in combat then the transform position is based on where you are looking
            pos.z = (isFront(LookingVector.z)) ? localZBACK : localZFRONT;
            _parentObject.transform.localPosition = pos;
            return;
        }


        // Ok finally if we're moving or sprinting then control it based on how we're moving
        bool isMoving = (GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f);  
        if (isMoving)
        { 
            // If we're moving primarily up then put it in front of the player sprite (behind the player, but towards the camera because we're running up)
            float zval = myCtrl.Velocity.z;
            float xval = myCtrl.Velocity.x;
            pos.z = (zval > 0 && Math.Abs(xval) < zval) ? localZFRONT : localZBACK;
            _parentObject.transform.localPosition = pos;
        }
        else
        {
            // if we're not moving then we need to put the weapon behind the player based on where they're looking
            pos.z = (isFront(LookingVector.z)) ? localZBACK : localZFRONT;
            _parentObject.transform.localPosition = pos;
        }
 
    }

    /// <summary>
    /// Handles the animation for the weapon object in your hand
    /// </summary>
    /// <param name="_weaponObject">The ojbect that is 'in the players hands'. This object is the one that rotates based on where you're looking.</param>
    void HandleActiveWeapon(GameObject _weaponObject)
    {
        // if we're in combat stance
        bool inCombatStance = (!myCtrl.Sprinting && InCombat && !UsingItem && !Punching);
        bool lookFront = (isFront(LookingVector.z));


        Vector3 pos = Vector3.zero;
        Vector3 toCursor = GlobalConstants.ZeroYComponent(LookingVector).normalized;

        MeleeWeapon MeleeRef = ActiveWeapon.GetComponent<MeleeWeapon>();
     

        if (inCombatStance)
        {
            // If we're in the combat stance then we need to rotate the weapon based on where we're looking
            // Unlike the holstered weapon, this weapon should be in front when we're looking front, and be in back when we're looking behind us
            pos.z = (lookFront) ? ZFRONT : ZBACK;
            ActiveWeapon.transform.localPosition = pos;


            // Some weapons won't rotate when we're holding them 
            if (ActiveWeapon.heldData.holdType == HeldWeapon.HoldType.Hold)
            {
                // Set the position and break out early  
                return; 
            }

            // If it's a melee weapon then don't bother rotating or flipping it
            if (MeleeRef != null)
            { 
                return;
            }


            // Flip the gun if it's on the left side of the player
            _weaponObject.GetComponentInChildren<SpriteRenderer>().flipY = (LookingVector.x < 0);

       

            // Otherwise we have a weapon that needs to rotate based on where you're looking
            _weaponObject.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(toCursor));


            return;
        }


        // Ok so we're not in a combat stance now what
        HandleHolsteredWeapon(ActiveWeapon.gameObject, _weaponObject,true);

        return;
 

    }

    /// Handle Animations 
    void Animations()
    {
        // First get a reference to the cursor location
        if(!CutsceneManager.InCutscene)
            LookingVector = GlobalConstants.ZeroYComponent(CamScript.CursorLocation - transform.position);

        // Based on the cursor location, make the player look in that direction
        // Handle looking left vs right via where the cursor is
        bool flip;
        if (myCtrl.Sprinting || !InCombat)
            flip = false;
        else
            flip = (LookingVector.x < 0);
        myRenderer.flipX = flip;

        // Handle looking up and down based on velocity
        bool facefront = (LookingVector.z < 0);
        if (UsingItem)
            facefront = true;
        myAnimator.SetBool("FaceFront", facefront); // Flips a bool switch based on if the cursor is above or below the character


        // If we're walking in a reverse direction we want to reverse our animation speed
        float spd = 1;  // This float controls that speed. To be clear, this is the animation speed, not the speed of the player. A - speed indicates an animation playing in reverse.
        if (!myCtrl.Sprinting)
        {
            // Handle negative speeds
            if (((myCtrl.Velocity.x > 0 && myRenderer.flipX) || (myCtrl.Velocity.x < 0 && !myRenderer.flipX)) && Mathf.Abs(myCtrl.Velocity.x) > Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;

            if (((myCtrl.Velocity.z > 0 && (LookingVector.z <0)) || (myCtrl.Velocity.z < 0 && (LookingVector.z > 0))) && Mathf.Abs(myCtrl.Velocity.x) < Mathf.Abs(myCtrl.Velocity.z))
                spd *= -1;
        } 


        // Handle walking and running bools
        Vector3 toCursor = GlobalConstants.ZeroYComponent(CamScript.CursorLocation - transform.position);
        myAnimator.SetFloat("SpeedX", myCtrl.Velocity.x);
        myAnimator.SetFloat("SpeedY", myCtrl.Velocity.z);
        myAnimator.SetFloat("LookX", LookingVector.x);
        myAnimator.SetFloat("LookY", LookingVector.z);
        myAnimator.SetFloat("Speed", spd);
        myAnimator.SetBool("Moving", (GlobalConstants.ZeroYComponent(myCtrl.Velocity).magnitude > 1f));
        myAnimator.SetBool("Running", myCtrl.Sprinting);
        myAnimator.SetBool("InCombat", InCombat);

        // Footstep data
        if (myCtrl.Sprinting != myCtrl.SprintingPrev)
            myFootStep.stepCooldown = 0;

        myFootStep.Speed = (myCtrl.Sprinting) ? .2f : ((InCombat) ? .45f : .35f);
 
        if(!CutsceneManager.InCutscene) 
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

	/// For input that is physics-based; goes in FixedUpdate
    void myFixedInput()
    {
        // Basic Movement
        // This could look a lot nicer, but ultimately it gets the job done
		if (!myCtrl.Airborne && !Punching)
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
         

    } 


	/// For input that is frame-based; goes in Update
	void myInput()
	{
		WeaponBase weaponToFire = (ActiveWeapon == PrimaryWeapon) ? PrimaryWeapon : SecondaryWeapon;
		MeleeWeapon myMelee = null;

		if(weaponToFire != null)
		{
			myMelee = weaponToFire.GetComponent<MeleeWeapon>();
		}

		// No shooting if the menu is open
		//bool shot = false;
		if (!MenuManager.MenuOpen && !MenuManager.OtherMenuOpen)
		{ 
			// No shooting when we're in a no combat zone
			if (ZoneScript.ActiveZone.ZoneAggression != ZoneScript.AggressionType.NoCombat)
            { 
                // Also no shooting if we're sprinting
                if (Input.GetMouseButton(0) && (!Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.LeftShift) && myCtrl.Velocity.magnitude < .2)) && Armed)
				{ 
					weaponToFire.FireWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
					combatCD = 0;
					//shot = true;

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

		// Handle sprinting
		if (InCombat && myCtrl.Sprinting != myCtrl.SprintingPrev)
		{
			myAudioSource.clip = (myCtrl.Sprinting) ? AudioClips[0] : AudioClips[1];
			myAudioSource.Play();
		}
 
		if (Input.GetMouseButtonDown(1) && (myMelee == null || !myMelee.isFiring)) // When we left click throw our weapon
		{
			TossWeapon(GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(transform.position));
		}

		// We don't want the player to be able to switch if they only have one weapon /  none at all
		if(Input.GetKeyDown(KeyCode.Q) && PrimaryWeapon != null && SecondaryWeapon != null && (myMelee == null || !myMelee.isFiring))
		{
            if (ActiveWeapon.heldData.DropOnInactive)
            {
                TossWeapon(Vector3.zero); 
            }
            else
			    SwitchWeapons();
		}

		// Use a healthpack (currently functions as a full heal)
		if(Input.GetKeyDown(KeyCode.R) && GameManager.HealthKits > 0 && !UsingItem)
		{
            MenuManager.instance.ShowHealthkit();
            if(MyUnit.CurrentHealth != MyUnit.MaxHealth)
            { 
                myCtrl.Velocity = Vector3.zero;
                StartCoroutine(UseItemCRT());
                myAnimator.SetFloat("Special", 1);
                UsingItem = true;
            }else
            {
                myAudioSource.clip = AudioClips[4];
                myAudioSource.Play();
            }
		}


        // PUNCH
        // cast a box to see if we deal damage
        Vector3 toCursor =  CamScript.CursorLocation - transform.position;   

		if (Input.GetKeyDown(KeyCode.E) && !Punching && (myMelee == null || !myMelee.isFiring))
        {
            // Don't move the player if there's an EINDC
            if (!UsableIndicator.IsAvailable)
                myCtrl.ApplyForce(toCursor.normalized * 4);

            // don't punch if we want to talk
            if (UsableIndicator.IsAvailable && (UsableIndicator.Grab.ind.Preset != UsableIndicator.usableIndcPreset.Disarm)) 
                return;

            // Do all the animations 
            myAnimator.SetFloat("Special", (UnityEngine.Random.Range(0, 1f) < .5) ? 2 : 3);
            InCombat = true;
            combatCD = 0;
            Punching = true;
            StartCoroutine(PunchingCRT());

            // run the check to see if we hit anything
            Collider[] c = Physics.OverlapSphere(transform.position, 2);
            for(int i = 0; i < c.Length; i++)
            { 
                GameObject obj = c[i].gameObject;
                if (obj == this.gameObject)
                    continue;

                IDamageable dmg = obj.GetComponent<IDamageable>();
                if (dmg == null)
                    continue;

                // Check to see if object is behind you
                Vector3 toTarget = (obj.transform.position - transform.position).normalized;
                if(Vector3.Dot(toTarget,toCursor) > 0)
					dmg.OnMelee(punchDamage); // deal damage
            }
        }

        // update looking vector
        LookingVector = GlobalConstants.ZeroYComponent(CamScript.CursorLocation - transform.position);

        // Set the springing bool equal to if we have the left shift key held down or not 
		myCtrl.Sprinting = (Input.GetKey(KeyCode.LeftShift) && !Punching && (myMelee == null || !myMelee.isFiring)); // has to happen last
    }

    IEnumerator PunchingCRT()
    {
		yield return new WaitForSeconds(punchCD);
        Punching = false;
        myAnimator.SetFloat("Special", 0);
    }

    public void TossWeapon(Vector3 _dir)
    {
        if (ActiveWeapon == null)
            return;

		// Only toss if player has both weapons
		if(PrimaryWeapon == null || SecondaryWeapon == null)
		{
			return;
		}
			
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
         
    }

    public void OnDeath()
    {
        // This needs some work 
        CutsceneManager.instance.ResetCutscene(); // So we don't softlock when we die in a cutscene
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

    public void OnHit(int _damage)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myVisualizer.ShowMenu();
        combatCD = 0;

        if (!InCombat)
        {
            myAudioSource.clip = AudioClips[0];
            myAudioSource.Play();
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
         
        myVisualizer.ShowMenu();

    }
		
    IEnumerator UseItemCRT()
    {
        // Show the health visualizer
        myVisualizer.ShowMenu();
        yield return new WaitForSeconds(.5f);

        // Start the particle and play the sfx
        Instantiate(Resources.Load("Prefabs/Particles/HealPart") as GameObject, transform.position + (Vector3.up * 4), Quaternion.identity);
        myAudioSource.clip = AudioClips[3];
        myAudioSource.Play();

        yield return new WaitForSeconds(.75f);
        myAudioSource.clip = AudioClips[3];
        myAudioSource.Play();

        yield return new WaitForSeconds(.75f);
        myAudioSource.clip = AudioClips[3];
        myAudioSource.Play();

        yield return new WaitForSeconds(1f);
        GameManager.HealthKits--;

        myUnit.CurrentHealth += 30; // Add the health
        if (myUnit.CurrentHealth > myUnit.MaxHealth)
            myUnit.CurrentHealth = myUnit.MaxHealth;

        // Ensure that we can see it 
        myVisualizer.ShowMenu();

        // Exit out of the animation
        myAnimator.SetFloat("Special", 0);
        UsingItem = false; 
    }

    public void PickUpWeapon(WeaponBase _newWeapon)
    {
        // First get a reference to the weapon itself
        if (_newWeapon == null)
            return;

		// If we already have the weapon, we don't need to pick it up again (fixes the weapon dispenser bug)
		if(_newWeapon == PrimaryWeapon || _newWeapon == SecondaryWeapon)
		{
			return;
		}

        combatCD = 0;

        _newWeapon.myOwner = this;
		_newWeapon.myShield = GetComponentInChildren<ForceFieldScript>();
		_newWeapon.myEnergy = GetComponent<EnergyManager>();

        // If primary weapon slot is open put the new weapon there
        if (PrimaryWeapon == null)
        {
            PrimaryWeapon = _newWeapon; 
            ActiveWeapon = PrimaryWeapon; 
            return;
        }

        // If the secondary weapon slot is open and the primary slot isn't, put the new weapon in the secondary slot
        if (PrimaryWeapon != null && SecondaryWeapon == null)
        {
            SecondaryWeapon = _newWeapon; 
            ActiveWeapon = SecondaryWeapon; 
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
                return;
            }

            if (ActiveWeapon == SecondaryWeapon)
            { 
                SecondaryWeapon = _newWeapon;
                ActiveWeapon = SecondaryWeapon; 
                return;
            }

        }
        
    }

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
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
         
    }

	public void SwitchWeapons()
	{
		ActiveWeapon = ((ActiveWeapon == PrimaryWeapon) ? SecondaryWeapon : PrimaryWeapon);

		myAudioSource.clip = AudioClips[1];
		myAudioSource.Play();
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

	public string Save()
	{
		StringWriter data = new StringWriter();
		
		data.WriteLine(transform.position);
		
		data.WriteLine(myUnit.CurrentHealth);

		// Weapons (Player.GetComponent<PlayerScript>().PrimaryWeapon, SecondaryWeapon, ActiveWeapon
		if(PrimaryWeapon != null)
		{
			if(PrimaryWeapon.name.IndexOf(' ') != -1)
			{
				data.WriteLine(PrimaryWeapon.name.Substring(0, PrimaryWeapon.name.IndexOf(' ')));
			}
			else
			{
				data.WriteLine(PrimaryWeapon.name);
			}
		}
		else
		{
			data.WriteLine("null");
		}

		if(SecondaryWeapon != null)
		{
			if(SecondaryWeapon.name.IndexOf(' ') != -1)
			{
				data.WriteLine(SecondaryWeapon.name.Substring(0, SecondaryWeapon.name.IndexOf(' ')));
			}
			else
			{
				data.WriteLine(SecondaryWeapon.name);
			}
		}
		else
		{
			data.WriteLine("null");
		}

		if(ActiveWeapon != null)
		{
			if(ActiveWeapon.name.IndexOf(' ') != -1)
			{
				data.WriteLine(ActiveWeapon.name.Substring(0, ActiveWeapon.name.IndexOf(' ')));
			}
			else
			{
				data.WriteLine(ActiveWeapon.name);
			}
		}
		else
		{
			data.WriteLine("null");
		}
		
		return data.ToString();
	}

	public void Load(string[] data)
	{
		// Parse transform.position!
		transform.position = GlobalConstants.StringToVector3(data[0]);

		myUnit.CurrentHealth = int.Parse(data[1].Trim());

		if(PrimaryWeapon != null)
		{
			Destroy(PrimaryWeapon.gameObject);
			PrimaryWeapon = null;
		}
		if(data[2] != "null")
		{
			PrimaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + data[2]), transform) as GameObject).GetComponent<WeaponBase>();
			PrimaryWeapon.gameObject.name = data[2];
		}

		if(SecondaryWeapon != null)
		{
			Destroy(SecondaryWeapon.gameObject);
			SecondaryWeapon = null;
		}
		if(data[3] != "null")
		{
			SecondaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + data[3]), transform) as GameObject).GetComponent<WeaponBase>();
			SecondaryWeapon.gameObject.name = data[3];
		}
		
		if(data[4] != "null")
		{
			if(data[4] == PrimaryWeapon.name)
			{
				ActiveWeapon = PrimaryWeapon;
			}
			else
			{
				ActiveWeapon = SecondaryWeapon;
			}
		}
	}

    public void MoveTo(Vector3 _destination)
    {
        moveAI.MoveTo(_destination);
    }

    public void Look(Vector3 _look)
    {
        LookingVector = _look;
        myAnimator.SetFloat("LookX", _look.x);
        myAnimator.SetFloat("LookY", _look.z); 
    }

	public void Stun(float time, bool unstun)
	{
		Stunned = true;
		AcceptInput = false;
		myForceField.canHeal = false;

		if (myEmojis != null)
		{
			myEmojis.Fire(EffectSystem.EffectType.Stunned, transform.position + (Vector3.up * 6) + (Vector3.back * 0.5f));
		}

		if(unstun)
		{
			StartCoroutine(Unstun(time));
		}
	}

	public IEnumerator Unstun(float time)
	{
		yield return new WaitForSeconds(time);

		Stunned = false;
		AcceptInput = true;
		myForceField.canHeal = true;
		myEmojis.Fired = false;
	}

    public void SetAggro(bool _b)
    {
        InCombat = _b;
        combatCD = 0;
        myAnimator.SetBool("InCombat", InCombat);
        myAudioSource.clip = (_b) ? AudioClips[0] : AudioClips[1];
        myAudioSource.Play();

    }

    public CController cc
    {
        get { return myCtrl; }
    }

    bool isFront(float v)
    {
        return (v < 0);
    }

}
