using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour, IWeapon {


    private IArmed myOwner;
    public WeaponInfo myInfo;
    public List<GameObject> BulletList;

    float currentshootCD;

    [Header("Effects")]
    public Animator myAnimator;
    public Light ShootFlare;
    public AudioClip ShootClip; 
    AudioSource myAudioSource;
    bool tryReload;

    WeaponInfo baseWeaponInfo; 

    // Use this for initialization
    void Start () {
        baseWeaponInfo = myInfo;
        myAudioSource = GetComponent<AudioSource>();

        MakeBullets();
        
    }
	
	// Update is called once per frame
	void Update () {
        currentshootCD += Time.deltaTime;
        if (currentshootCD > .1 && ShootFlare != null)
        {
            ShootFlare.enabled = false;
        }

        if(myWeaponInfo.currentAmmo <= 0 && !tryReload)
        {
            tryReload = true;
            ForceReload();
        }
    }


    public WeaponInfo myWeaponInfo
    {
        get
        {
            return myInfo;
        }

        set
        {
            myInfo = value;
        }
    }


    public void FireWeapon(Vector3 _dir)
    {
        // Break out if we can't even shoot
        if (currentshootCD < myInfo.fireCD)
            return;

        if (myInfo.currentAmmo <= 0)
            return;

        
        // for (int i = 0; i < BulletList.Count; i++)
        {
                int i = myInfo.currentAmmo - 1;
                // Do something based on the type
                // For now we'll just shoot one bullet                
                IBullet iB = BulletList[i].GetComponent<IBullet>();
                iB.Owner = (myOwner);
                iB.WeaponData = (myWeaponInfo);
                BulletList[i].transform.position = transform.position;
                BulletList[i].GetComponent<IBullet>().Shoot(_dir);
                currentshootCD = 0;
                myInfo.currentAmmo--;

                if (ShootFlare != null)
                {
                    ShootFlare.enabled = true;
                }

                if (myAnimator != null)
                {
                    myAnimator.SetTrigger("Fire");
                }

                // Play the sound
                myAudioSource.clip = ShootClip;
                myAudioSource.Play();

                myOwner.myVisualizer.ShowMenu();
                return; 
        }


    }

    public List<GameObject> bulList
    {
        get { return BulletList; }
    }

    public IArmed Owner
    {
        get { return myOwner; }
        set { myOwner = value; 
        }
    }


    // Called when the player presses r
    public void ForceReload()
    {
        myInfo.currentAmmo = 0;
        tryReload = true;
        Owner.myVisualizer.ShowMenu();
        myAudioSource.clip = myInfo.reloadClip;
        myAudioSource.Play();
        StopAllCoroutines();
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(myInfo.reloadSpeed);
        myInfo.currentAmmo = myInfo.maxAmmo;
        Owner.myVisualizer.ShowMenu();
        tryReload = false;
        myAudioSource.clip = myInfo.reloadClip;
        myAudioSource.Play();
    }

    public bool isReloading
    {
        get { return tryReload; }
    }

    public void UpdateUpgrades()
    {
        myWeaponInfo = baseWeaponInfo; 
        Upgrades myUpgrades = GameManager.instance.UpgradeData;

        for(int i =0; i < 3; i++)
        {
            if(myUpgrades.ClipSize[i])
            {
                myWeaponInfo.maxAmmo += 1;
            }

            if(myUpgrades.FireRate[i])
            {
                myWeaponInfo.fireCD -= .05f;
            }

            if(myUpgrades.ReloadSpeed[i])
            {
                myWeaponInfo.reloadSpeed -= .03f;
            }
        }

        MakeBullets();

        for(int i = 0; i <BulletList.Count;i ++)
        {
            BulletList[i].GetComponent<PlayerBullet>().myUpgrades = myUpgrades.UpgradeType;
        }

        
    }

    void MakeBullets()
    {
        // Ensure the bullets are properly stored
        if (BulletList.Count != myInfo.maxAmmo)
        {
            // If they aren't, clear the list
            foreach (GameObject o in BulletList)
            {
                Destroy(o);
            }

            BulletList = new List<GameObject>();

            // Then refill it
            for (int i = 0; i < myInfo.maxAmmo; i++)
            {
                GameObject newObj = (GameObject)Instantiate(myInfo.BulletObject); 
                BulletList.Add(newObj);
            }
        }
    }
}
