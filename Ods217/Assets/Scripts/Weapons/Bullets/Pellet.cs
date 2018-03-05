using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : BulletBase {

    public GameObject Poofy;
    public float LifeTime;
    float dTime;


    public override void Shoot(Vector3 _dir)
    {
        base.Shoot(_dir);

        Poofy.transform.SetParent(transform);
        Poofy.transform.localPosition = Vector3.zero;
        Poofy.GetComponent<ParticleSystem>().Play();
    }

    public override void UpdateBullet()
    {
        dTime += Time.deltaTime;

        if (dTime < LifeTime)
            base.UpdateBullet();
        else
            BulletDeath(); 
    }

    public override void BulletDeath()
    {
        Poofy.transform.SetParent(null);
        Poofy.GetComponent<ParticleSystem>().Stop();
        dTime = 0;
        base.BulletDeath();
       
    }
}
