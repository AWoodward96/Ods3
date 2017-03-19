using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BulPulse : MonoBehaviour, IBullet {


    [Header("Bullet Data")]
    public Vector3 Direction;
    public float BulletSpeed;
    public GlobalConstants.Alligience myAlligience;
    public float Lifetime;
    float currentLife;

    bool Fired;
    IUnit Owner;
    SpriteRenderer myRenderer;
    BoxCollider myCollider;


    // Use this for initialization
    void Awake () {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;

        Fired = false;
        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;
    }
	
	// Update is called once per frame
	void Update () {
		if(Fired)
        {
            transform.position += (Direction * BulletSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(Direction));
        }

        currentLife += Time.deltaTime;

        if (currentLife > Lifetime)
            Fired = false;

        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;
	}

    public bool CanShoot()
    {
        return !Fired;
    }

    public void Shoot(Vector3 _dir)
    {
        Direction = _dir.normalized;
        Fired = true;
        currentLife = 0;
    }

    public void OnHit(IUnit _unitObj)
    {

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IUnit>() == Owner)
            return;

        if(other.GetComponent<IUnit>() != null)
        {
            IUnit u = other.GetComponent<IUnit>();
            OnHit(u);
            u.OnHit(Owner.MyWeapon());
            Fired = false;
        }

        if(other.GetComponent<INonUnit>() != null)
        {
            INonUnit u = other.GetComponent<INonUnit>();
     
            u.OnHit();
            Fired = false;
        }

       
        Fired = false;
    }

    public void setOwner(IUnit _Owner)
    {
        Owner = _Owner;
    }
}
