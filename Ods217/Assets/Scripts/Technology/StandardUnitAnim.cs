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

    public Vector3 HolsteredRotation;
    public Vector3 HolsteredPosition;
    public Vector3 HeldPosition;

    public Vector3 velocity;
    public GameObject gunObject1;
    public GameObject gunObject2;
    public GameObject myObject;
    public GameObject activeGunObject;

    SpriteRenderer myRend;

    public StandardUnitAnim(GameObject _myObj, GameObject _gunObj1, GameObject _gunObj2, Animator _anim)
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
  
        // Handle negative speeds
        if (((velocity.x > 0 && flipSprites) || (velocity.x < 0 && !flipSprites)) && Mathf.Abs(velocity.x) > Mathf.Abs(velocity.z))
            spd *= -1;

        if (((velocity.z > 0 && (LookingVector.z <  myObject.transform.position.z)) || (velocity.z < 0 && (LookingVector.z > myObject.transform.position.z))) && Mathf.Abs(velocity.x) < Mathf.Abs(velocity.z))
            spd *= -1;

        myRend.flipX = flipSprites;

        Anim.SetFloat("Speed", spd);
        Anim.SetBool("FaceFront", faceFront); 
        Anim.SetBool("Moving", GlobalConstants.ZeroYComponent(velocity).magnitude > .1f);
        GunAnims(); 
    }

    void GunAnims()
    {
        if (activeGunObject == null)
            return;

        SpriteRenderer gunRend = activeGunObject.GetComponentInChildren<SpriteRenderer>();
        gunRend.flipY = flipSprites;

        Vector3 pos = (holdGun) ? HeldPosition : HolsteredPosition;
        if(holdGun)
            pos.z = (faceFront) ? -.1f : .1f;
        else
            pos.z = (faceFront) ? .1f : -.1f;

        activeGunObject.transform.localPosition = pos;

        if (holdGun)
            activeGunObject.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(LookingVector.normalized));
        else
            activeGunObject.transform.rotation = Quaternion.Euler(HolsteredRotation);

    }

    


}
