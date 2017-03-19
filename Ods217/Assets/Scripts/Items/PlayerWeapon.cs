using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour {

    public PlayerWeapon instance;
    enum FireType { Pulse, Shotgun, Melee };
    FireType GunType = FireType.Pulse;

    [Header("Primary Weapon Data")]
    public int BulletDamage;
    public GameObject BulType; 
    public int MaxClip;
    public int CurrentClip;
    public float ReloadSpeed;
    public float FireCoolDown;

    float currentCd;
    bool tryReload;
    List<GameObject> myBullets = new List<GameObject>();
    public IUnit Owner;

    public AudioClip ShootClip;
    AudioSource myAudioSource;

    [Header("Secondary Weapon Data")]
    public GameObject EMPBullet;
    public enum SecondaryType { EMP, None };
    public SecondaryType CurrentSecondary = SecondaryType.None;
    public int CurrentSecondaryClip;

    List<GameObject> EMPBullets = new List<GameObject>();

    // First check
    void Start()
    {
        instance = this;
        myAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        ValidateValues();

        currentCd += Time.deltaTime;
    }

    void ValidateValues()
    {
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
        if(myBullets.Count != MaxClip)
        {
            // If they aren't, clear the list
            foreach(GameObject o in myBullets)
            {
                Destroy(o);
            }

            myBullets = new List<GameObject>();
            
            // Then refill it
            for(int i = 0; i < MaxClip; i++)
            {
                GameObject newObj = (GameObject)Instantiate(BulType);
                newObj.GetComponent<IBullet>().setOwner(Owner);
                myBullets.Add(newObj); 
            }
        }
    }

 

    public void FireWeapon(Vector3 dir)
    {
        // Break out if we can't even shoot
        if (currentCd < FireCoolDown)
            return;

        if (CurrentClip <= 0)
            return;

        for(int i = 0; i < myBullets.Count; i++)
        {
            if(myBullets[i].GetComponent<IBullet>().CanShoot())
            {
                // Do something based on the type
                // For now we'll just shoot one bullet
                myBullets[i].transform.position = transform.position;
                myBullets[i].GetComponent<IBullet>().Shoot(dir);
                currentCd = 0;
                CurrentClip--;

                // Play the sound
                myAudioSource.clip = ShootClip;
                myAudioSource.Play();
                return;
            }
        }
    }

    public void FireSecondary()
    {

    }

    public void LoadSecondaryAmmo(SecondaryType _type, int _clipSize)
    {
        switch (_type)
        {
            case SecondaryType.EMP:
                CurrentSecondary = SecondaryType.EMP;
                CurrentSecondaryClip = _clipSize;
                break;
        }

        
    }

    public void ForceReload()
    {
        CurrentClip = 0;
        tryReload = true;
        StopAllCoroutines(); 
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(ReloadSpeed);
        CurrentClip = MaxClip;
        tryReload = false;
    }
}
 
