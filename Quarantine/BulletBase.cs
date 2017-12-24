using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteToParticlesAsset;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class BulletBase : MonoBehaviour {

    public Vector3 Direction;
    public float Speed;
    public float Lifetime; 
    public bool Fired;

    [Space(20)]
    [Header("Paired WeaponData")]
    public WeaponInfo Data;

    float currentLife;
    Renderer myRenderer;
    Collider myCollider;

    public virtual void Initialize()
    {
        myRenderer = GetComponent<Renderer>();
        myCollider = GetComponent<Collider>();
    }

    public virtual void BulUpdate()
    {
        transform.position += Direction.normalized * Speed;
        transform.rotation = Quaternion.Euler(90, 0, GlobalConstants.angleBetweenVec(Direction));


        // If it's lived too long kill it
        currentLife += Time.deltaTime;

        if (currentLife > Lifetime)
            Fired = false;

        myRenderer.enabled = Fired;
        myCollider.enabled = Fired;
    }

    

}

 