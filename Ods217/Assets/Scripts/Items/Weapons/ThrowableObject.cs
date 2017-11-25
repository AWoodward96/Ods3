using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour, IWeapon {

    public WeaponInfo myInfo;
    public AudioClip ShootClip;
    IArmed myOwner;
    List<GameObject> BulletList;
    bool tryReload;
    float currentshootCD;

    AudioSource myAudioSource;
    Animator myAnimator;
    AudioSource mySource;

    public bool SingleToss;

    // Use this for initialization
    void Awake()
    {
        BulletList = new List<GameObject>();
        myAudioSource = GetComponent<AudioSource>();
        myAnimator = GetComponent<Animator>();
        mySource = GetComponent<AudioSource>();
        mySource.clip = ShootClip;

        MakeBullets();
    }

    // Update is called once per frame
    void Update()
    {
        currentshootCD += Time.deltaTime;

        if (myWeaponInfo.currentAmmo <= 0 && !tryReload && !SingleToss)
        {
            tryReload = true; 
            ForceReload();
        }

        transform.rotation = Quaternion.identity;
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

    public IArmed Owner
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
                // Update crucial reference data 
                IBullet iB = BulletList[i].GetComponent<IBullet>();
                iB.Owner = (myOwner);
                iB.WeaponData = (myWeaponInfo);

                iB.Shoot(myOwner.gameObject.transform.position + _dir);
                currentshootCD = 0;
                myInfo.currentAmmo--;


                // Play the sound
                if(myAudioSource != null)
                { 
                    myAudioSource.clip = ShootClip;
                    myAudioSource.Play();
                }

                myOwner.myVisualizer.ShowMenu();

                if (myAnimator != null)
                    myAnimator.SetTrigger("Fire");

                if (SingleToss)
                {
                    SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer rnd in rends)
                        rnd.enabled = false;
                    StartCoroutine(frameToss());
                }

                return;
            }
        }

    }

    IEnumerator frameToss()
    {
        yield return new WaitForSeconds(.5f);
        myOwner.gameObject.GetComponent<IMultiArmed>().TossWeapon(Vector3.zero);
        gameObject.transform.parent.gameObject.SetActive(false); 
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
                BulletList.Add(newObj);
            }
        }
    }
}
