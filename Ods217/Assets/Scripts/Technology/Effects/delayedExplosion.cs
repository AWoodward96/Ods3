using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class delayedExplosion : MonoBehaviour {

    ParticleSystem explosionSystem;
    AudioSource src;
    SpriteRenderer myRend;

    public float Size;
    public int Damage;

	// Use this for initialization
	void Start () {
        explosionSystem = GetComponent<ParticleSystem>();
        myRend = GetComponent<SpriteRenderer>();
        src = GetComponent<AudioSource>();
        myRend.enabled = false;

        if (explosionSystem == null)
            explosionSystem = GetComponentInChildren<ParticleSystem>();
	} 

    public void Fire(float _time)
    {
        // Raycast down so we are on the ground
        Ray r = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if(Physics.Raycast(r,out hit,10,LayerMask.GetMask("Ground")))
        {
            transform.position = hit.point + (Vector3.up *.1f);
        }

        myRend.enabled = true;   
        StartCoroutine(explosion(_time));
    }

    IEnumerator explosion(float _time)
    {
        yield return new WaitForSeconds(_time);
        myRend.enabled = false;
        explosionSystem.Play();

        if (src != null)
            src.Play();


        // See if we hit anyone
        Collider[] cols = Physics.OverlapSphere(transform.position, Size);
        IDamageable foo;
        CController cc;
        for (int i = 0; i < cols.Length; i++)
        {
            foo = cols[i].GetComponent<IDamageable>();
            if (foo != null)
            {
                foo.OnHit(Damage);
            }

            cc = cols[i].GetComponent<CController>();
            if (cc != null)
            {
                if (!cc.Immovable)
                {
                    Vector3 dist = cc.transform.position - transform.position;
                    float strength = (1.5f - dist.magnitude) * -10;
                    cc.ApplyForce(dist * strength);
                }
            }
        }
    }
}
