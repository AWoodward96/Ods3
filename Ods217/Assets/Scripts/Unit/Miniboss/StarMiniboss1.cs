using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarMiniboss1 : AIStandardUnit{

    [Header("Part 1")]
    public Canvas EntranceCanvas;
    public Transform[] jumpPointMarkers;
    public Transform impactPointMarker;
    public Rigidbody explosiveMine;
    public delayedExplosion giantExplosion;
    public ParticleSystem SmokeBomb; 
    int jumpPointIndex;
     
    public bool triggered;
    public bool hopTrigger;
    public bool hopping;
    Rigidbody myRGB;
    ForceFieldScript myForceField;
    
	EnergyManager myEnergy;
   
    bool aistate = false; // false = rafters; true = floor;
    bool exitState;
    bool disarmed = false;
    public enum shootstate { Wait, Jump, Shoot }
    public shootstate currentShootState;
    float waitTimer;
    int aiCounter = 0;

    Vector3 overrideLookingVector;
    LineRenderer line; 

    public UsableIndicator ind_Usable; // Has to be initialized publicly because we're holding shit like weapons

    // Use this for initialization
    public override void Start () {
        AIState = EnemyAIState.Aggro;
        myRGB = GetComponent<Rigidbody>(); 

        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = (14 + 1);
        line.useWorldSpace = false;

        myForceField = GetComponentInChildren<ForceFieldScript>();

		myEnergy = GetComponent<EnergyManager>();

        ind_Usable.Preset = UsableIndicator.usableIndcPreset.Disarm;
        ind_Usable.Output = DisarmDelegate;

        explosiveMine.gameObject.SetActive(false);
        Physics.IgnoreCollision(this.GetComponent<Collider>(), explosiveMine.GetComponent<Collider>());
        Physics.IgnoreCollision(this.GetComponent<Collider>(), playerRef.GetComponent<Collider>());

        SmokeBomb.transform.SetParent(null);


       
        base.Start();

        if (currentWeapon.weaponData.name == "Shotgun")
            base.SwapWeapons();
	}

	
	// Update is called once per frame
	public override void Update () {
 
        if(CutsceneManager.InCutscene)
        {
            moveAI.Update();
        }

        if (triggered)
        {
            // since our AI state is automatically set to Aggro, base.update should call the overriden aggro method every time
            // or the vulnerable method, if we've disarmed him
            base.Update();
        }
        else
            UpdateAnimationController();

        myAnimator.SetBool("AIState", aistate);
        ind_Usable.Disabled = !(base.AIState == EnemyAIState.Vulnerable); // Hide the usable indicator if we're not vulnerable
    }

    public override bool Triggered
    {
        get
        {
            return triggered;
        }

        set
        { 
            if(!triggered && value == true)
                StartCoroutine(OnTriggered());
        }
    }


    IEnumerator OnTriggered()
    {
        EntranceCanvas.gameObject.SetActive(true);
        
        myAnimator.SetInteger("Special", 1);

        while(CutsceneManager.InCutscene)
        {
            yield return null;
        }

        myAnimator.SetInteger("Special", 0);
        EntranceCanvas.gameObject.SetActive(false);
        triggered = true;
        Camera.main.GetComponent<CamScript>().AddWatch(gameObject);

    }
    public override void AggroState()
    {
        // Call the right method based on the AI state bool
        if(!aistate)
        {
            // If false, then we should be on the rafters hearling grenades
            RafterShoot();
            animationHandler.HeldPosition = new Vector3(0, -.2f, 0);
        }
        else
        {
            // If true, then we should be on the ground with a machine gun
            FloorShoot();
            animationHandler.HeldPosition = new Vector3(0,.33f,0);
        }
 
    }

    // Jump around in the rafters, hurling grenades
    void RafterShoot()
    {
        switch (currentShootState)
        {
            case shootstate.Wait:
                // Turn on the cc if we're close enough to the point
                if (Vector3.Distance(transform.position, jumpPointMarkers[jumpPointIndex].position) < 1f)
                {
                    transform.position = jumpPointMarkers[jumpPointIndex].position;

                    // Increment index
                    if (!myRGB.isKinematic)
                    {
                        jumpPointIndex++;
                        if (jumpPointIndex >= jumpPointMarkers.Length)
                            jumpPointIndex = 0;
                    }

                    myRGB.isKinematic = true;
                    myCC.enabled = true; 
                    myAnimator.ResetTrigger("Leaping");
                    myAnimator.SetTrigger("Landed");

                }
                myAnimator.SetFloat("LeapingY", myRGB.velocity.y);

                // Make sure that the gun is set behind you at all times 
                animationHandler.holdGun = false;

                // Start a timer
                waitTimer += Time.deltaTime;
                if (waitTimer > 1.5f) // After 3 seconds shoot
                {
                    aiCounter++;

                    // If we've done this 4 times
                    if (aiCounter > 4)
                    {
                        if(waitTimer > 4 && !exitState)
                        { 
                            // Reset the timer
                            exitState = true;
                            waitTimer = 0;

                          
                            myAnimator.SetTrigger("HurlSmoke");
                            StartCoroutine(FallEntry());
                        }
                    }
                    else
                    {
                        waitTimer = 0;
                        currentShootState = shootstate.Shoot; 
                    }


                }
                break;
            case shootstate.Jump:
                // Set jump velocity
                StartCoroutine(Jump());
                myAnimator.SetTrigger("Leaping");
                myAnimator.ResetTrigger("Landed");
                myAnimator.SetFloat("LeapingY", myRGB.velocity.y);

                myRGB.isKinematic = false;
                myCC.enabled = false;

                animationHandler.faceFront = true;
                animationHandler.holdGun = false;

                // Wait until you land to shoot
                currentShootState = shootstate.Wait;
                animationHandler.holdGun = false; 
                animationHandler.faceFront = false;
                break;
            case shootstate.Shoot:

                // Fire all cannons
                myWeapon.FireWeapon(playerRef.transform.position - transform.position);
                animationHandler.holdGun = true;  

                if(transform.position.y < 0)
                    transform.position = jumpPointMarkers[jumpPointIndex].position;

                // If we've run out, start a quick timer
				if (MyUnit.CurrentEnergy < myWeapon.weaponData.shotCost)
                {
                    waitTimer += Time.deltaTime;
                }

                // After 1 second, jump!
                if (waitTimer > 1)
                {
                    waitTimer = 0;
                    currentShootState = shootstate.Jump;
                }
                break;

        }
         
    }
    
    IEnumerator Jump()
    {
        yield return new WaitForSeconds(.16f); // A wait because we have a lead-up jumping animation
        myRGB.velocity = GlobalConstants.getPhysicsArc(transform.position, jumpPointMarkers[jumpPointIndex].position);
    }
 
    IEnumerator FallEntry()
    {

        yield return new WaitForSeconds(.7f);
        // Reset the animation
        SmokeBomb.transform.position = transform.position + Vector3.forward;
        SmokeBomb.Play();
        SmokeBomb.GetComponent<AudioSource>().Play();
        

        yield return new WaitForSeconds(.5f);

        // Throw the grenade
        // Set up the giant explosion
        giantExplosion.transform.position = impactPointMarker.transform.position + Vector3.up;
        giantExplosion.Fire(5);

        // Set up the explosive mine
        explosiveMine.transform.SetParent(null);
        explosiveMine.gameObject.SetActive(true);
        explosiveMine.transform.position = transform.position;
        explosiveMine.velocity = GlobalConstants.getPhysicsArc(transform.position, impactPointMarker.position + ((transform.position - impactPointMarker.position) * .1f));


        // Turn off the cc and rgb
        myRGB.isKinematic = true;
        myCC.enabled = false;

        // Get the fuck out
        transform.position = Vector3.zero + (Vector3.down * 10);
        yield return new WaitForSeconds(5.1f);

        SmokeBomb.Stop();

        myAnimator.ResetTrigger("HurlSmoke");

        // Re-enable the renderer
        GetComponent<SpriteRenderer>().enabled = true;
        // Set the position up
        transform.position = impactPointMarker.transform.position + (Vector3.up * 10);

        // Reset and re-enable the rgb and cc
        myRGB.velocity = Vector3.zero;
        myCC.Velocity = Vector3.zero;
        myRGB.isKinematic = true;
        myCC.enabled = true;

        // Turn off the explosive mine
        explosiveMine.transform.SetParent(this.transform);
        explosiveMine.gameObject.SetActive(false);

        yield return new WaitForSeconds(1);
        // Reset the exit states
        aistate = true;
        exitState = false;
        // Swap to the machine gun
        SwapWeapons();
        overrideLookingVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position); // Also set up the looking vector
        aiCounter = 0;

    }

    IEnumerator BackToRafters()
    {
        giantExplosion.transform.position = transform.position;
        myRGB.isKinematic = false;
        myCC.enabled = false;
        myRGB.velocity = Vector3.up * 100;
        giantExplosion.Fire(3);

        // Set up the exposive mine
        explosiveMine.gameObject.SetActive(true);
        explosiveMine.transform.SetParent(null); 
        explosiveMine.transform.position = transform.position;
        explosiveMine.velocity = Vector3.up * 5;

        myAnimator.SetTrigger("Leaping");
        myAnimator.ResetTrigger("Landed");

        yield return new WaitForSeconds(1f);
        myRGB.isKinematic = true; // Freeze in mid air
        myCC.Velocity = Vector3.zero;
        yield return new WaitForSeconds(2f);
         
        // Set the position back up
        jumpPointIndex = UnityEngine.Random.Range(0, jumpPointMarkers.Length);
        transform.position = jumpPointMarkers[jumpPointIndex].position + (Vector3.up);

        // Reset the rgb and cc
        myCC.enabled = true;
        myRGB.velocity = Vector3.zero;

        // Reset the ai states
        aiCounter = 0;
        aistate = false;
        exitState = false;

        // Switch back to the grenade launcher
        SwapWeapons();

        myAnimator.SetTrigger("Landed");
        myAnimator.ResetTrigger("Leaping");

        jumpPointIndex++;
        if (jumpPointIndex >= jumpPointMarkers.Length)
            jumpPointIndex = 0;

        // Reset the explosive mine again
        explosiveMine.gameObject.SetActive(false);
        explosiveMine.transform.SetParent(this.transform);
        explosiveMine.transform.position = transform.position;
    }

    // Jump down to the ground, whip out a machine gun and go to town
    void FloorShoot()
    {
        if(!exitState)
        {
            animationHandler.LookingVector = overrideLookingVector;
            myCC.ApplyForce(overrideLookingVector.normalized * .2f);

            overrideLookingVector = Vector3.RotateTowards(overrideLookingVector, GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position), .1f, .1f);
            myWeapon.FireWeapon(overrideLookingVector);
			if (MyUnit.CurrentEnergy < myWeapon.weaponData.shotCost)
            {
                waitTimer = 0;
                base.AIState = EnemyAIState.Vulnerable; 
            }

            // Exit state
            if(aiCounter > 4)
            {
                exitState = true;
                StartCoroutine(BackToRafters());
            }
        }

    }

    public override void VulnState()
    {

        // Keep updating the looking vector while we're vulnerable
        animationHandler.holdGun = true;

        waitTimer += Time.deltaTime;

        if (!disarmed)
        {

            myCC.ApplyForce(overrideLookingVector.normalized * .2f);

            // The circles size is based on how long it'll take to 
            CreatePoints();


            line.enabled = true;

			if (MyUnit.CurrentEnergy >= MyUnit.MaxEnergy - 10)
            {
                aiCounter++;
                waitTimer = 0;
                base.AIState = EnemyAIState.Aggro; 
                line.enabled = false;
            }
        }
        else
        {
            myAnimator.SetBool("Vulnerable", true); 
            line.enabled = false;

            animationHandler.holdGun = false;

            // Wait and take a bunch of hits from the player
            if(waitTimer > 5)
            {
                myAnimator.SetBool("Vulnerable", false);
                aiCounter++; 
                disarmed = false;
                waitTimer = 0;
                base.AIState = EnemyAIState.Aggro; 
            }
        }
        
    }

    void CreatePoints()
    {
        float x = 0f;
        float ccheight = GetComponent<CharacterController>().height;
        float y = -(ccheight / 2) + .1f;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (14 + 1); i++)
        {
			x = Mathf.Sin(Mathf.Deg2Rad * angle) * (myEnergy.RegenTime + myEnergy.ChargeDelay - waitTimer + .5f);
			z = Mathf.Cos(Mathf.Deg2Rad * angle) * (myEnergy.RegenTime + myEnergy.ChargeDelay - waitTimer + .5f);

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / 14);
        }
    }

 

    void DisarmDelegate()
    {
        if (myForceField != null) // As long as the forcefield exists
        {
            // Break it!
			myForceField.RegisterHit(MyUnit.CurrentEnergy);
            disarmed = true;
            waitTimer = 0;

            base.myCC.ApplyForce((transform.position - playerRef.transform.position).normalized * 20);
        }
    }

    public override void OnHit(int _damage)
    {

        if (myZone != ZoneScript.ActiveZone) // Don't let them take damage if you're not in their scene
            return;

        // Take no damage if on rafters
        if (!aistate)
            return;

        // Firstly show the health bar (Remove this when we have the on-screen healthbar)
        myVisualizer.ShowMenu();
        if (myForceField != null)
        {
			if (MyUnit.CurrentEnergy > 0)
                myForceField.RegisterHit(_damage);
            else
            { 
                UnitData.CurrentHealth -= _damage;
            }
        } 

        if (UnitData.CurrentHealth <= 0)
        {
            Camera.main.GetComponent<CamScript>().RemoveWatch(gameObject);
            myWeapon.ReleaseWeapon();
            this.gameObject.SetActive(false);
        }
    }

    public override void UpdateAnimationController()
    {
        if (aistate) // If we're not in idle state ...
        {
            overrideLookingVector = Vector3.RotateTowards(overrideLookingVector, GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position), .1f, .1f);
            animationHandler.LookingVector = overrideLookingVector;

        }
        else
        {
            if(currentShootState == shootstate.Jump || currentShootState == shootstate.Wait)
            {
                animationHandler.LookingVector = myRGB.velocity;
                animationHandler.faceFront = false;
 
            }

            if(currentShootState == shootstate.Shoot)
            {
                animationHandler.LookingVector = GlobalConstants.ZeroYComponent(playerRef.transform.position - transform.position);
            }
        }

        animationHandler.velocity = myCC.Velocity;
         
        //animationHandler.holdGun = (AIState == EnemyAIState.Aggro || (AIState == EnemyAIState.Vulnerable && !disarmed));

        animationHandler.gunObject1 = WeaponSlot1;
        animationHandler.gunObject2 = WeaponSlot2;
        animationHandler.activeGunObject = currentWeapon;
        animationHandler.Update();
    } 
}
