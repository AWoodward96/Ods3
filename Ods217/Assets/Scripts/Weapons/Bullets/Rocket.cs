using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : BulletBase {

    [Space(20)]
    [Header("Rocket Data")]
    public float KickSpeed = 0;
    public float KickTime = 0;
    public float KickWeight = 1;
    float baseSpeed;

    public ParticleSystem SmokePart;
    public ParticleSystem KickPart;
    public AudioClip KickClip;
    AudioSource KickSource;
    Vector3 KickOrigin;
    Vector3 SmokeOrigin;

    ParticleSystem.EmissionModule SmokeEmit;
    ParticleSystem.EmissionModule KickEmit;

    private void Start()
    {
        baseSpeed = base.Speed;
        if(KickPart != null)
        {
            KickOrigin = KickPart.transform.localPosition;
            KickEmit = KickPart.emission;
        }

        if (SmokePart != null)
        {

            SmokeOrigin = SmokePart.transform.localPosition;
            SmokeEmit = SmokePart.emission;
        }

        gameObject.SetActive(false);
    }

    public override void Shoot(Vector3 _dir)
    {
         
        if (SmokePart != null)
        {
            SmokePart.gameObject.SetActive(false);
            SmokePart.transform.parent = transform;
            SmokePart.transform.localPosition = SmokeOrigin;
            SmokePart.gameObject.SetActive(true);
        }

        if (KickPart != null)
        {
            KickPart.transform.parent = transform;
            KickPart.transform.localPosition = KickOrigin;
            KickEmit.rateOverDistanceMultiplier = 1;
        }



        base.Shoot(_dir);


        transform.position += Direction.normalized; // kick it up a peg
        base.Speed = baseSpeed;

        StartCoroutine(Kick());
    }

    IEnumerator Kick()
    {
        yield return new WaitForSeconds(KickTime);
        base.Speed = baseSpeed + KickSpeed;


        transform.position += Direction.normalized * KickWeight; // kick it up a peg
        if (KickPart != null)
        {
            KickPart.gameObject.transform.SetParent(null);
            KickPart.Play(); 
        }

        if (KickClip != null)
        {
            if (KickSource == null)
                KickSource = gameObject.AddComponent<AudioSource>();

            KickSource.playOnAwake = false;
            KickSource.clip = KickClip;
            KickSource.Play();
        } 
    }

    public override void BulletDeath()
    {
        if(KickPart != null)
            KickPart.transform.parent = null;

        if(SmokePart != null)
            SmokePart.transform.parent = null;

        Instantiate(Resources.Load("Prefabs/Particles/RocketExplosion") as GameObject, transform.position, Quaternion.identity);

        base.BulletDeath();

    }
}
