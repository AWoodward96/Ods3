using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Primary Weapon Script
/// Handles all the information needed for weapons
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour
{

    [Header("Weapon Info")]
    public string Name;
    public string Description;
    public enum FireType { Pulse, Shotgun, Melee };
    public FireType GunType;

    [Header("Weapon Data")]
    public int BulletDamage; // How much damage the bullet will do
    public GameObject BulType; // The actual bullet prefab
    public int MaxClip; // The maximum clip size
    public int CurrentClip; // How many bullets you have left in the chamber
    public float ReloadSpeed; // How long it takes to reload
    public float FireCoolDown; // How fast you can shoot

    float currentCd;
    bool tryReload;
    List<GameObject> primaryBullets = new List<GameObject>();
    public IUnit Owner;

    public AudioClip ShootClip;
    AudioSource myAudioSource;


    public enum SecondaryTypes { EMP, None };
    [Header("Secondary Weapon Data")]
    public SecondaryTypes SecondaryType = SecondaryTypes.None;
    public int CurrentSecondaryClip; // How many secondary shots you have left in chamber
    public int CurrentMaxSecondaryClip; // How many secondary shots you can have in a chamber

    List<GameObject> SecondaryBullets = new List<GameObject>();

    // First check
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Ensure we actually have valid weapon data
        ValidateValues();

        currentCd += Time.deltaTime;

        // If we currently have no secondary clip don't show any clips in the chamber (See the health visualizer script)
        if (CurrentSecondaryClip < 1)
        {
            SecondaryType = SecondaryTypes.None;
        }
    }


    void ValidateValues()
    {
        if (BulType == null)
            return;

        // Validate the fields
        if (MaxClip <= 0)
            MaxClip = 1;

        if (CurrentClip > MaxClip)
            CurrentClip = MaxClip;

        if (CurrentClip <= 0 && !tryReload)
        {
            CurrentClip = 0;
            tryReload = true;
            StartCoroutine(Reload());
        }

        // Ensure the bullets are properly stored
        if (primaryBullets.Count != MaxClip)
        {
            // If they aren't, clear the list
            foreach (GameObject o in primaryBullets)
            {
                Destroy(o);
            }

            primaryBullets = new List<GameObject>();

            // Then refill it
            for (int i = 0; i < MaxClip; i++)
            {
                GameObject newObj = (GameObject)Instantiate(BulType);
                newObj.GetComponent<IBullet>().setOwner(Owner);
                primaryBullets.Add(newObj);
            }
        }
    }



    public void FireWeapon(Vector3 _dir)
    {
        // Break out if we can't even shoot
        if (currentCd < FireCoolDown)
            return;

        if (CurrentClip <= 0)
            return;

        for (int i = 0; i < primaryBullets.Count; i++)
        {
            if (primaryBullets[i].GetComponent<IBullet>().CanShoot)
            {
                // Do something based on the type
                // For now we'll just shoot one bullet
                primaryBullets[i].transform.position = transform.position;
                primaryBullets[i].GetComponent<IBullet>().Shoot(_dir);
                currentCd = 0;
                CurrentClip--;



                // Play the sound
                myAudioSource.clip = ShootClip;
                myAudioSource.Play();

                Owner.myVisualizer.ShowMenu();
                return;
            }
        }
    }

    // This is almost identical to the secondary, but secondary
    public void FireSecondary(Vector3 _dir)
    {
        if (SecondaryType != SecondaryTypes.None)
        {
            if (currentCd < FireCoolDown)
                return;

            if (CurrentSecondaryClip <= 0)
                return;

            for (int i = 0; i < SecondaryBullets.Count; i++)
            {
                if (SecondaryBullets[i].GetComponent<IBullet>().CanShoot)
                {
                    // Do something based on the type
                    // For now we'll just shoot one bullet
                    SecondaryBullets[i].transform.position = transform.position;
                    SecondaryBullets[i].GetComponent<IBullet>().Shoot(_dir);
                    currentCd = 0;
                    CurrentSecondaryClip--;

                    // Play the sound
                    myAudioSource.clip = ShootClip;
                    myAudioSource.Play();

                    Owner.myVisualizer.ShowMenu();
                    return;
                }
            }
        }
    }

    public void LoadSecondaryAmmo(SecondaryTypes _type, GameObject SecondaryBullet, int _clipSize)
    {
        // We don't want to spawn a million bullets so get rid of any of them
        foreach (GameObject o in SecondaryBullets)
        {
            Destroy(o);
        }

        SecondaryBullets = new List<GameObject>();
        SecondaryType = SecondaryTypes.EMP;
        CurrentSecondaryClip = _clipSize;
        CurrentMaxSecondaryClip = CurrentSecondaryClip;

        if (CurrentSecondaryClip > SecondaryBullets.Count)
        {
            // Get however many emps we'll need to generate
            int howMany = CurrentSecondaryClip - SecondaryBullets.Count;

            // Make them
            for (int i = 0; i < howMany; i++)
            {
                GameObject o = (GameObject)Instantiate(SecondaryBullet);
                o.GetComponent<IBullet>().setOwner(Owner);
                SecondaryBullets.Add(o);
            }

            // Alright at this point you should have all the bullets you need
        }

        Owner.myVisualizer.ShowMenu();
    }

    // Called when the player presses r
    public void ForceReload()
    {
        CurrentClip = 0;
        tryReload = true;
        Owner.myVisualizer.ShowMenu();
        StopAllCoroutines();
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(ReloadSpeed);
        CurrentClip = MaxClip;
        Owner.myVisualizer.ShowMenu();
        tryReload = false;
    }

    public bool isReloading
    {
        get { return tryReload; }
    }
}

