using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponBase {

    [Range(1,10)]
    public float ArcWidth; // Just how strong the rnd arc will be
    [Range(1, 10)]
    public float ArcDistance; // How far away from the player the arc will be effected
    public int BulsPerShot;
    public int BulsGenerated = 5;


    ParticleSystem ptl;

    public override void Awake()
    {
        base.Awake();
        maxBullets = BulsGenerated;
        MakeBullets();
    }

    public override void FireWeapon(Vector3 _dir)
    {
        // Break out if we can't even shoot
        if (currentshootCD < weaponData.fireCD)
            return;

        if (myOwner.MyUnit.CurrentEnergy < weaponData.shotCost)
            return;

        if (myEnergy.BrokenEnergy)
            return;

        if (myBullets.Count == 0)
        {
            maxBullets = (int)Mathf.Ceil(myOwner.MyUnit.MaxEnergy / (float)weaponData.shotCost);
            MakeBullets(); 
            if (myBullets.Count == 0)
                return;
        }

        _dir = _dir.normalized * ArcDistance;

        List<BulletBase> canFire = new List<BulletBase>();
        int bulsfound = 0;
        for(int i = 0; i < myBullets.Count; i++)
        {
            if (bulsfound == BulsPerShot)
                break;

            if(myBullets[i].CanShoot && !canFire.Contains(myBullets[i]))
            {
                canFire.Add(myBullets[i]);
                bulsfound++;
            }
        }

        for (int i = 0; i < canFire.Count; i++)
        {
            canFire[i].gameObject.SetActive(true);
            canFire[i].myOwner = (myOwner);
            canFire[i].myInfo = (weaponData);
            canFire[i].transform.position = transform.position + (_dir.normalized / 2);
            canFire[i].Shoot(_dir + GlobalConstants.ZeroYComponent(Random.insideUnitSphere * ArcWidth)); 
        } 

        currentshootCD = 0;
 

        myEnergy.ExpendEnergy(weaponData.shotCost);

        // Check for an animator
        Animator anim = RotateObject.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Fire");

        if (myOwner != null)
            myOwner.myVisualizer.ShowMenu();

        // Check the gun object for a particle system. If it exists play it
        if(GunPtlSys != null)
        {
            GunPtlSys.Play();
        }

        //Play the sound
        if (myAudioSource != null)
        {
            myAudioSource.clip = ShootClip;
            myAudioSource.Play();
        }
         
    }


    ParticleSystem GunPtlSys
    {
        get {
            if(ptl == null)
                ptl = RotateObject.GetComponentInChildren<ParticleSystem>();

            return ptl;
        }
    }
}
