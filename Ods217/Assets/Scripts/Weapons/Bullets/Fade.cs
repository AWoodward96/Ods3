using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : BulletBase {

   // public GameObject Poofy;
    public float LifeTime;
    float dTime;
    SpriteRenderer rend;


    public override void Shoot(Vector3 _dir)
    {
        base.Shoot(_dir);

        //Poofy.transform.SetParent(transform);
        //Poofy.transform.localPosition = Vector3.zero;
        //Poofy.GetComponent<ParticleSystem>().Play();

        if (rend == null)
            rend = GetComponent<SpriteRenderer>();
    }

    public override void UpdateBullet()
    {
        dTime += Time.deltaTime;

        if(rend != null)
        {
            Color c = Color.white;
            c.a = 1.5f - (dTime / LifeTime);
            rend.color = c; 
        }

        if (dTime < LifeTime)
            base.UpdateBullet();
        else
            BulletDeath(); 
    }

    public override void BulletDeath()
    {
        //Poofy.transform.SetParent(null);
        //Poofy.GetComponent<ParticleSystem>().Stop();
        dTime = 0;
        base.BulletDeath();
       
    }
}
