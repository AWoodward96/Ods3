using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A blank melee script. Literally exists to do nothing
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BlankMelee : MonoBehaviour, IBullet {


    // Use this for initialization
    void Awake () {
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
 
	}

    public bool CanShoot()
    {
        return true;
    }

    public void Shoot(Vector3 _dir)
    {
 
    }

    public void OnHit(IUnit _unitObj)
    {

    }

    public void setOwner(IUnit _Owner)
    {
      
    }
}
