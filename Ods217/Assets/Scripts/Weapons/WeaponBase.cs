using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour {

    public GameObject ThrownObject;
    public GameObject RotateObject;

    [Space(10)]
    public HeldWeapon heldData;
    public WeaponInfo weaponData;

    [Space(10)] 
    public bool Reloading;
    public AudioClip ShootClip;

    public List<BulletBase> myBullets;
    public IArmed myOwner;


    float currentshootCD;
    bool tryReload;

    GameObject player;
    AudioSource myAudioSource;

    bool WeaponReleased;
     
 
    // Use this for initialization
    void Awake () {
        myAudioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        heldData.Initialize(this, ThrownObject, player);
        myBullets = new List<BulletBase>();

        // Check if we're being held
        IMultiArmed isMulti = GetComponentInParent<IMultiArmed>();
        IArmed isArmed = GetComponentInParent<IArmed>();

        if(isMulti != null)
        {
            heldData.PickUp(isMulti);
        }else if(isArmed != null)
        {
            heldData.DisableMulti();
        }else
        {
            heldData.PickedUp = false;
            heldData.Toss(Vector3.down, transform.position);
        }
 
        MakeBullets();
 
	}
	
	// Update is called once per frame
	void Update () {

        if(!WeaponReleased)
        {
            ThrownObject.SetActive(!heldData.PickedUp);
            RotateObject.SetActive(heldData.PickedUp);
        }

        if(!heldData.PickedUp)
        {
            heldData.HandlePlayerPickupRange();
        }

        UpdateBullets();

        if (weaponData.currentAmmo <= 0 && !tryReload)
        {
            tryReload = true;
            ForceReload();
        }

        currentshootCD += Time.deltaTime;
	}

    public virtual void UpdateBullets()
    { 
        for(int i = 0; i < myBullets.Count; i ++)
        {
            if (myBullets[i].Fired)
            {
                myBullets[i].UpdateBullet(); 
            }
            else
            {
                if (myBullets[i].gameObject.activeInHierarchy)
                {
                    myBullets[i].gameObject.SetActive(false); 
                }  
            }
        }
    }
   

    void MakeBullets()
    { 

        // Ensure the bullets are properly stored
        if (myBullets.Count != weaponData.maxAmmo)
        {
            // If they aren't, clear the list
            foreach (BulletBase o in myBullets)
            {
                Destroy(o.gameObject);
            }

            myBullets = new List<BulletBase>();

            // Then refill it
            for (int i = 0; i < weaponData.maxAmmo; i++)
            {
                GameObject newObj = (GameObject)Instantiate(weaponData.BulletObject);
                myBullets.Add(newObj.GetComponent<BulletBase>());
                newObj.SetActive(false);
            }
        }
    }

    public void FireWeapon(Vector3 _dir)
    {
        // Break out if we can't even shoot
        if (currentshootCD < weaponData.fireCD)
            return;

        if (weaponData.currentAmmo <= 0)
            return;
 
        int i = weaponData.currentAmmo - 1;
        // Do something based on the type
        // For now we'll just shoot one bullet    
        myBullets[i].gameObject.SetActive(true); 
        myBullets[i].myOwner = (myOwner);
        myBullets[i].myInfo = (weaponData);
        myBullets[i].transform.position = transform.position;
        myBullets[i].Shoot(_dir);

        currentshootCD = 0;
        weaponData.currentAmmo--;

        //if (myAnimator != null)
        //{
        //    myAnimator.SetTrigger("Fire");
        //}

        //Play the sound
        myAudioSource.clip = ShootClip;
        myAudioSource.Play();

        myOwner.myVisualizer.ShowMenu();
        return; 

    }

    public void ForceReload()
    {
        weaponData.currentAmmo = 0;
        tryReload = true;
        Reloading = true;
        myOwner.myVisualizer.ShowMenu();
        //myAudioSource.clip = weaponData.reloadClip;
        //myAudioSource.Play();
        StopAllCoroutines();
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(weaponData.reloadSpeed);
        weaponData.currentAmmo = weaponData.maxAmmo;
        myOwner.myVisualizer.ShowMenu();
        Reloading = false;
        tryReload = false;
        myAudioSource.clip = weaponData.reloadClip;
        myAudioSource.Play();
    }

    private void OnDisable()
    {
        if(ThrownObject != null)
            ThrownObject.SetActive(false);
 
    }

    private void OnDestroy()
    {
        // destroy all bullets
        for(int i = 0; i < myBullets.Count;i ++)
        {
            Destroy(myBullets[i]);
        }
    }

    private void OnEnable()
    {
        if(!heldData.PickedUp && ThrownObject != null)
        {
            ThrownObject.SetActive(true);
        }
    }


    /// <summary>
    /// Method called when we want to get rid of a weapon but not toss it when it's owner dies. 
    /// This will keep the weapon active in scene but it won't be usable, allowing it to update it's bullets at will
    /// </summary>
    public virtual void ReleaseWeapon()
    {
        transform.SetParent(null);
        ThrownObject.SetActive(false);
        RotateObject.SetActive(false);
        heldData.PickedUp = false;
        WeaponReleased = true;
    }
}


 
