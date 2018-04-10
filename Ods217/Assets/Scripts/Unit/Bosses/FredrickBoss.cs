using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FredrickBoss : AIStandardUnit {

    [Header("Fredrick Specs")]
    public bool Aggro;

    float dTime = 0;
    public bool localAIState; // False = Standing still shooting at the player when they can see you; True = Jumping to a new position
    float curJumpAngle = 60;
    float[] JumpAngle = { 60, 45, 25 };
    public Transform[] JumpPoints;
    Transform selectedPoint;

    public Canvas EntranceCanvas;

    ForceFieldScript myForceField;
    Rigidbody myRGB;

    bool jumpTriggered = false;

    public override void Start()
    {
        myRGB = GetComponent<Rigidbody>();
        myForceField = GetComponentInChildren<ForceFieldScript>();
        myRGB.isKinematic = true;
        base.Start();
    }

    public override void Update()
    { 
        if (CutsceneManager.InCutscene)
        {
            moveAI.Update();
            animationHandler.Update();
        }


        myAnimator.SetBool("Disarmed", AIState == EnemyAIState.Vulnerable);

        if (Aggro)
         base.Update();
    } 

    public override bool Triggered
    {
        get  { return Aggro; }
        set
        { 
            if (!Aggro)
                StartCoroutine(OnTriggered());
        }
    }


    IEnumerator OnTriggered()
    {
        EntranceCanvas.gameObject.SetActive(true);

        myAnimator.SetInteger("Special", 1);

        while (CutsceneManager.InCutscene)
        {
            yield return null;
        }

        Camera.main.GetComponent<CamScript>().AddWatch(gameObject);
        myAnimator.SetInteger("Special", 0);
        EntranceCanvas.gameObject.SetActive(false);
        Aggro = true; 
    }


    public override void AggroState()
    {
        dTime += Time.deltaTime;
        Vector3 toPlayer = GlobalConstants.ZeroYComponent( playerRef.transform.position - transform.position);

        if (localAIState)
        {
            Vector3 toPoint = GlobalConstants.ZeroYComponent(selectedPoint.position - transform.position);  
            if(toPoint.magnitude < .33f || transform.position.y < 3)
            { 

                myRGB.isKinematic = true;
                myCC.enabled = true;
                localAIState = false;

                myAnimator.SetBool("Jumping", false);
                transform.position = selectedPoint.position;
                myCC.Velocity = Vector3.zero;
                
            }
        }else
        { 
            // Raycast to the player to see if you can shoot at them
            Ray r = new Ray(transform.position, toPlayer);
            RaycastHit hit;
            if(!Physics.SphereCast(r.origin,1.5f,r.direction,out hit,toPlayer.magnitude,LayerMask.GetMask("Platform")))
            {
                myWeapon.FireWeapon(toPlayer);
			}else if(MyUnit.CurrentEnergy <= (MyUnit.MaxEnergy / 2)) // if we're low on ammo then bail
            {
                //myWeapon.ForceReload(); 
                if (!jumpTriggered) 
                    StartCoroutine(WaitJump());
                return;
            }

            // Check if you are out of ammo
			if (MyUnit.CurrentEnergy <= (MyUnit.MaxEnergy / 2) || dTime > 5)
            {
                /*if(!myWeapon.Reloading)
                    myWeapon.ForceReload();*/
                if (!jumpTriggered)
                    StartCoroutine(WaitJump());
                return;
            }

        }
    }

    IEnumerator WaitJump()
    {
        jumpTriggered = true;
        yield return new WaitForSeconds(.1f);
        JumpToNewPoint();
        localAIState = true;
        jumpTriggered = false;
    }

    void JumpToNewPoint()
    {
        float largestDistance = 0;
        Transform t = null;
        for(int i = 0; i < JumpPoints.Length; i ++)
        {
            // Ignore the point if it's the current one
            if (JumpPoints[i] == selectedPoint)
                continue;

            Vector3 toP = GlobalConstants.ZeroYComponent(JumpPoints[i].position - playerRef.transform.position);
            if (toP.magnitude > largestDistance)
            {
                largestDistance = toP.magnitude;
                t = JumpPoints[i];
            }
        }

        selectedPoint = t;
 

        // Turn off the cc and turn on the rgb
        myRGB.isKinematic = false;
        myCC.enabled = false;
        dTime = 0;

        myAnimator.SetBool("Jumping", true);
        StartCoroutine(Jump());
    }

    IEnumerator Jump()
    {
        yield return new WaitForSeconds(.167f);// wait for the crouch frames 

        Vector3 toSelected = GlobalConstants.ZeroYComponent(selectedPoint.position - transform.position);

        myRGB.velocity = GlobalConstants.getPhysicsArc(transform.position, selectedPoint.position + toSelected.normalized, curJumpAngle);
    }

    public override void OnHit(int _damage)
    { 

        if (UnitData.CurrentHealth / UnitData.MaxHealth < .66f)
            curJumpAngle = JumpAngle[1];

        if (UnitData.CurrentHealth / UnitData.MaxHealth < .33f)
            curJumpAngle = JumpAngle[2];

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
            GameObject obj = Resources.Load("Prefabs/Particles/deathPartParent") as GameObject;
            Instantiate(obj, transform.position, obj.transform.rotation);


            Camera.main.GetComponent<CamScript>().RemoveWatch(gameObject);
            myWeapon.ReleaseWeapon();
            this.gameObject.SetActive(false);
        }
    }

    public void StunBoss()
    {
        if (localAIState)
            return;

        AIState = EnemyAIState.Vulnerable;
        dTime = 0;
        GetComponent<EnergyManager>().timeSinceHit = -3;
    }

    public void SaveBoss()
    {
        if(localAIState)
        {
            myRGB.isKinematic = true;
            myCC.enabled = true;
            localAIState = false;

            myAnimator.SetBool("Jumping", false);
            transform.position = selectedPoint.position;
        }
    }

    public override void VulnState()
    {
        dTime += Time.deltaTime;

        if(dTime > 3)
        {
            AIState = EnemyAIState.Aggro;
            dTime = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.GetMask("Platform"))
        {
            Debug.Log("YEEEP");
            ExplosiveBox b = collision.gameObject.GetComponent<ExplosiveBox>();
            if(b != null)
            {
                b.Triggered = true;
            }
        }
    }
}
