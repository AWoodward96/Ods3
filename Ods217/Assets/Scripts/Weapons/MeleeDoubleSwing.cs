using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDoubleSwing : MeleeWeapon {

    [Space(20)]
    [Header("Double Swing Data")]
    public float baseRotation = 120; 
    public float curRotation;
    public float SwingOverlap = 1f;
    public bool Swing;
    public bool Held;
     

    bool flip;

    Vector3 Look;
     

    public override void Awake()
    {
        base.Awake();
        heldData.holdType = HeldWeapon.HoldType.Hold;
    }

    public override void UpdateBullets()
    {
        if (myOwner == null)
            return;

        if (WeaponReleased)
            return;

        // Update the animations for this object
        Vector3 rot = new Vector3((Held) ? 0 : 90, 0, curRotation);
        RotateObject.transform.rotation = Quaternion.Euler(rot);


        HandleLooking();

        if (currentshootCD > (weaponData.fireCD + SwingOverlap))
        {
            Held = true;
            Swing = false;
            curRotation = baseRotation * ((flip) ? -1 : 1);
        }


    }


    void HandleLooking()
    {
        PlayerScript p = myOwner.gameObject.GetComponent<PlayerScript>();
        if(p != null)
        {
            if (p.cc.Sprinting)
            { 
                Held = true;
                flip = false;
                curRotation = baseRotation * ((flip) ? -1 : 1);
            }
            else
            { 
                flip = p.LookingVector.x > 0;
            }

        }else
        {
            AIStandardUnit standard = myOwner.gameObject.GetComponent<AIStandardUnit>();
            if (standard != null)
            {
                if (standard.cc.Sprinting)
                { 
                    Held = true;
                    flip = false;
                    curRotation = baseRotation * ((flip) ? -1 : 1);
                }
                else
                    flip = standard.animationHandler.LookingVector.x > 0;
            }
        } 

    }

    public override void FireWeapon(Vector3 _dir)
    {
        base.FireWeapon(_dir);


        // now swing! 
       if(currentshootCD > weaponData.fireCD)
        {
            // Reset basic variables
            Held = false;
            currentshootCD = 0; 
            Swing = !Swing; 
             
            // Rotate the object
            curRotation = GlobalConstants.angleBetweenVec(_dir) + ((Swing) ? -180 : 0); 
        }

    }

}
