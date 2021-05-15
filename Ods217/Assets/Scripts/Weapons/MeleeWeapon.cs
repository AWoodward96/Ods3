using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Space(20)]
    [Header("Melee Data")]
	public float range;
	public float arcAngle;	// Angle of the attack's total sweep, in degrees.
	float arcDotProduct;	// arcAngle converted into the proper dot product to check

	public bool launches;
	public float launchForce;

	public bool stuns;
	public float stunLength;  

    Vector3 direction;


    CController cc; 
    PlayerScript ps; 
    Animator myAnim;
     

	public float moveForce;

    public override void Awake()
    {
		myAudioSource = GetComponent<AudioSource>();
		ps = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		heldData.Initialize(this, ThrownObject, ps.gameObject);

		myOwner = GetComponentInParent<IArmed>();

		if(myOwner != null)
		{
			myShield = myOwner.gameObject.GetComponentInChildren<ForceFieldScript>();
			myEnergy = myOwner.gameObject.GetComponent<EnergyManager>();
		}

		// Check if we're being held
		IMultiArmed isMulti = GetComponentInParent<IMultiArmed>();
		IArmed isArmed = GetComponentInParent<IArmed>();

		if(isMulti != null)
		{
			heldData.PickUp(isMulti);
		}
		else if(isArmed != null)
		{
			heldData.DisableMulti();
		}
		else
		{
			heldData.PickedUp = false;
			heldData.Toss(Vector3.down, transform.position);
		}

		myAnim = RotateObject.GetComponent<Animator>();

		arcDotProduct = Mathf.Cos((arcAngle / 2.0f) * Mathf.Deg2Rad);
    }

    public override void FireWeapon(Vector3 _dir)
    {
		if(currentshootCD < weaponData.fireCD || isFiring)
		{
			return;
		}
			
		direction = _dir.normalized;
		RotateObject.transform.right = GlobalConstants.ZeroYComponent(direction);

		Vector3 myRot = RotateObject.transform.eulerAngles;
		myRot.x = 90.0f;
		RotateObject.transform.eulerAngles = myRot;

		cc = myOwner.gameObject.GetComponent<CController>(); 

        if(myAudioSource != null)
        {
            myAudioSource.clip = ShootClip;
            myAudioSource.Play();
        }

        StartCoroutine(MeleeAttack());
    }

 
    IEnumerator MeleeAttack()
	{
		isFiring = true;

		myAnim.Play("Attacking");
		cc.ApplyForce(direction * moveForce);

		// Run the check to see if we hit anything
		Collider[] c = Physics.OverlapSphere(transform.position, range);
		for(int i = 0; i < c.Length; i++)
		{ 
			GameObject obj = c[i].gameObject;
			if (obj == this.gameObject)
				continue;

			if(obj == myOwner.gameObject)
				continue;

			IDamageable dmg = obj.GetComponent<IDamageable>();
			if (dmg == null)
				continue;

			// Check to see if target is in cone of attack
			Vector3 toTarget = (obj.transform.position - transform.position).normalized;
			if(Vector3.Dot(toTarget, direction) >= arcDotProduct)
			{
				// Check if our target is a tank
				mobTank myTank = obj.GetComponent<mobTank>();
				if(myTank != null)
				{
					if(myTank.CheckAttack(transform.position, Mathf.FloorToInt(weaponData.bulletDamage)))
					{
						myTank.Stun();
					}
				}

				// Fun time!
				else
				{
					dmg.OnMelee(Mathf.FloorToInt(weaponData.bulletDamage));
				}

				// Launch the target
				if(launches)
				{
					CController otherCC = obj.GetComponent<CController>();
					if(otherCC != null)
					{
						otherCC.ApplyForce(toTarget * launchForce);
                         
						// Stun the target!
						if(stuns)
						{
							if(otherCC.gameObject == ps.gameObject)
							{
								ps.Stun(stunLength, true);
							}
						}
					}
				}
			}
		}

		yield return new WaitUntil(finishSwing); 
		isFiring = false; 
	}

	bool finishSwing()
	{
		AnimatorStateInfo myInfo = myAnim.GetCurrentAnimatorStateInfo(0);
		return myInfo.IsName("Attacking");
	}
}
