using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBoom : WeaponBase {

    [Header("Big Boom Info")]
    [Tooltip("Load up the three prefabs for the Big Boom in order: LittleBoy, TubbyTeen, and FatMan")]
    public GameObject[] Prefabs;
    GameObject[] RocketObjects = new GameObject[3];
 
    public bool locked;
    public AudioClip FireClip;
    public AudioClip PowerUpClip; 

    PlayerScript Player;

    IEnumerator shootCRT;
     

    public override void Awake()
    {
        base.Awake(); 
    }

    public override void MakeBullets()
    {
        // If they aren't, clear the list
        foreach (BulletBase o in myBullets)
        {
            Destroy(o.gameObject);
        }
         
        myBullets.Add(Instantiate(Prefabs[0], transform.position, Quaternion.identity).GetComponent<BulletBase>());
        myBullets.Add(Instantiate(Prefabs[1], transform.position, Quaternion.identity).GetComponent<BulletBase>());
        myBullets.Add(Instantiate(Prefabs[2], transform.position, Quaternion.identity).GetComponent<BulletBase>());
    }

    public override void FireWeapon(Vector3 _dir)
    {
        if (locked)
            return;

        if (currentshootCD < weaponData.fireCD)
            return;

        if (myOwner.MyUnit.CurrentEnergy < weaponData.shotCost)
            return;

        if (!myEnergy)
            myEnergy = myOwner.gameObject.GetComponent<EnergyManager>();

        if (myEnergy.BrokenEnergy)
            return;

        // Don't allow shooting anymore regardless of the cd
        locked = true;

        if(myAudioSource != null)
        {
            myAudioSource.clip = PowerUpClip;
            myAudioSource.Play();
        }

        // Expend the energy and show the UI
        myEnergy.ExpendEnergy(weaponData.shotCost);
        if (myOwner != null)
            myOwner.myVisualizer.ShowMenu();

        // Fire at will!
        currentshootCD = 0;
        isFiring = true;
        shootCRT = FireBigBoom();
        StartCoroutine(shootCRT);

    }

    public override void UpdateBullets()
    {
        base.UpdateBullets();

        if(playerRef != null)
        {
            if (playerRef.cc.Sprinting && isFiring)
            {
                locked = false;
                StopCoroutine(shootCRT);
            }
        }
    }


    IEnumerator FireBigBoom()
    {
        int fire = 0;
         
        while(locked)
        { 
            //yield return new WaitForSeconds(.3f + (.1f * fire));
            yield return new WaitForSeconds(.3f);

            Vector3 looking = Vector3.zero;

            if (playerRef != null)
            {
                playerRef.combatCD = 0;
                looking = playerRef.LookingVector;
            }else if(myOwner.gameObject.GetComponent<AIStandardUnit>() != null)
            {
                looking = myOwner.gameObject.GetComponent<AIStandardUnit>().animationHandler.LookingVector;
            }

             
            BulletBase b = myBullets[fire].GetComponent<BulletBase>();
            b.gameObject.SetActive(true);
            b.myOwner = (myOwner);
            b.myInfo = (weaponData);
            b.myInfo.bulletDamage = 15 * (fire + 1);
            b.transform.position = transform.position + (looking.normalized / 2);
            b.Shoot(looking);

            if(myAudioSource != null)
            {
                myAudioSource.clip = FireClip;
                myAudioSource.Play();
            }

            myOwner.gameObject.GetComponent<EnergyManager>().timeSinceHit = 0;


            fire++;
            if (fire >= 3)
                locked = false;
             
             
        }

        isFiring = false;
         
    }

    PlayerScript playerRef
    {
        get
        {
            if (myOwner == null)
                return null;
             
            if(Player == null)
                Player = myOwner.gameObject.GetComponent<PlayerScript>();

            return Player;
        }
    }
 

}
