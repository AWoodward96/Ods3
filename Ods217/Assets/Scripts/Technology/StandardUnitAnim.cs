using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StandardUnitAnim {

    Animator Anim;
    public Vector3 LookingVector;

    public bool faceFront;
    public bool flipSprites;
    public bool holdGun;

    public enum DirectionalType { Two, Four };
    public DirectionalType LookType = DirectionalType.Two;

    public Vector3 HolsteredRotation; 
    public Vector3 HolsteredPosition;
    public Vector3 HeldPosition;

    public Vector3 velocity;
    public WeaponBase gunObject1;
    public WeaponBase gunObject2; 
    public GameObject myObject;
    public WeaponBase activeGunObject;

    SpriteRenderer myRend;

    public StandardUnitAnim(GameObject _myObj, WeaponBase _gunObj1, WeaponBase _gunObj2, Animator _anim)
    {
        myObject = _myObj;
        gunObject1 = _gunObj1; 
        gunObject2 = _gunObj2;
        Anim = _anim;
        myRend = myObject.GetComponent<SpriteRenderer>();
    }

    public void Initialize(GameObject _myObj, WeaponBase _gunObj1, WeaponBase _gunObj2, Animator _anim)
    {
        myObject = _myObj;
        gunObject1 = _gunObj1;
        gunObject2 = _gunObj2;
        Anim = _anim;
        myRend = myObject.GetComponent<SpriteRenderer>();
    }

    public void Update()
    {  
        faceFront = LookingVector.z <= 0;
        flipSprites = LookingVector.x <= 0;

        float spd = 1;  // This float controls that speed. To be clear, this is the animation speed, not the speed of the player. A - speed indicates an animation playing in reverse.
        if (LookType == DirectionalType.Two)
        {
            // Handle negative speeds
            if (((velocity.x > 0 && flipSprites) || (velocity.x < 0 && !flipSprites)) && Mathf.Abs(velocity.x) > Mathf.Abs(velocity.z))
                spd *= -1;

            if (((velocity.z > 0 && (LookingVector.z < myObject.transform.position.z)) || (velocity.z < 0 && (LookingVector.z > myObject.transform.position.z))) && Mathf.Abs(velocity.x) < Mathf.Abs(velocity.z))
                spd *= -1;

            myRend.flipX = flipSprites;
        }
  


        bool isMoving = GlobalConstants.ZeroYComponent(velocity).magnitude > .1f;
        Anim.SetFloat("Speed", spd);
        Anim.SetBool("FaceFront", faceFront); 
        Anim.SetBool("Moving",isMoving);

        if (GlobalConstants.AnimatorHasParameter(Anim, "moveX") && isMoving)
            Anim.SetFloat("moveX", velocity.x);
        if (GlobalConstants.AnimatorHasParameter(Anim, "moveY") && isMoving)
            Anim.SetFloat("moveY", velocity.z);
        if (GlobalConstants.AnimatorHasParameter(Anim, "lookX"))
            Anim.SetFloat("lookX", LookingVector.x);
        if (GlobalConstants.AnimatorHasParameter(Anim, "lookY"))
            Anim.SetFloat("lookY", LookingVector.z);

        GunAnims(); 
    }

    void GunAnims()
    {
        if (activeGunObject == null)
            return;

		MeleeWeapon myMelee = activeGunObject.GetComponent<MeleeWeapon>();
 

        GameObject rotateMe = activeGunObject.RotateObject;

        Vector3 pos = (holdGun) ? HeldPosition : HolsteredPosition;
		if (holdGun || myMelee != null)
            pos.z = (faceFront) ? -.1f : .1f;
        else
            pos.z = (faceFront) ? .1f : -.1f;


        activeGunObject.transform.localPosition = pos;

        if (activeGunObject.heldData.holdType == HeldWeapon.HoldType.Hold)
        { 
            return;
        }
         

		// Don't flip melee weapons
		if(myMelee == null)
		{
        	SpriteRenderer gunRend = activeGunObject.RotateObject.GetComponentInChildren<SpriteRenderer>();
        	gunRend.flipY = flipSprites;
		}


        if (holdGun)
		{
			if(myMelee == null)
			{
				rotateMe.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(LookingVector.normalized)); // NOT local because if it's local then it'll be relative to the rest of this objects rotation. Set it globally
			}
			else
			{
				rotateMe.transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(LookingVector.normalized));
			}
		}
        else
            rotateMe.transform.localRotation = Quaternion.Euler(HolsteredRotation);

        // If we have two objects
        if(gunObject1 != null && gunObject2 != null)
        {
            WeaponBase notHeldObject = (activeGunObject == gunObject1) ? gunObject2 : gunObject1;

            Vector3 newPos = HolsteredPosition;
            newPos.z = (faceFront) ? .1f : -.1f;
            notHeldObject.transform.localPosition = newPos;
            notHeldObject.RotateObject.transform.localRotation = Quaternion.Euler(HolsteredRotation);
        }

    }

    


}
