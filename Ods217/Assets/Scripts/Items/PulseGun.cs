using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PulseGun : MonoBehaviour, IWeapon {

    public WeaponInfo myInfo;
    public AudioClip ShootClip;
    IUnit myOwner;
    List<GameObject> BulletList;
    bool tryReload;
    float currentshootCD;

    AudioSource myAudioSource;

    // Use this for initialization
    void Start () {
        BulletList = new List<GameObject>();
        myAudioSource = GetComponent<AudioSource>();

        MakeBullets();
    }
	
	// Update is called once per frame
	void Update () {
        currentshootCD += Time.deltaTime;

        if (myWeaponInfo.currentAmmo <= 0 && !tryReload)
        {
            tryReload = true;
            ForceReload();
        }
    }


    public List<GameObject> bulList
    {
        get
        {
            return BulletList;
        }
    }

    public bool isReloading
    {
        get
        {
            return tryReload;
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

    public IUnit Owner
    {
        get
        {
            return myOwner;
        }

        set
        {
            myOwner = value;
        }
    }

    public void FireWeapon(Vector3 _dir)
    {
        // Break out if we can't even shoot
        if (currentshootCD < myInfo.fireCD)
            return;

        if (myInfo.currentAmmo <= 0)
            return;

        for (int i = 0; i < BulletList.Count; i++)
        {
            if (BulletList[i].GetComponent<IBullet>().CanShoot)
            {
                // Do something based on the type
                // For now we'll just shoot one bullet
                BulletList[i].transform.position = transform.position;
                BulletList[i].GetComponent<IBullet>().Shoot(_dir);
                currentshootCD = 0;
                myInfo.currentAmmo--;
 

                // Play the sound
                myAudioSource.clip = ShootClip;
                myAudioSource.Play();
                return;
            }
        }
    }

    public void ForceReload()
    {
        myInfo.currentAmmo = 0;
        tryReload = true;
        Owner.myVisualizer.ShowMenu();
        StopAllCoroutines();
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(myInfo.reloadSpeed);
        myInfo.currentAmmo = myInfo.maxAmmo;
        Owner.myVisualizer.ShowMenu();
        tryReload = false;
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
                newObj.GetComponent<IBullet>().setOwner(myOwner);
                BulletList.Add(newObj);
            }
        }
    }
}
