using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTargetScript : MonoBehaviour, IArmed {

    public UnitStruct UnitData;
    public bool Activated;
    public Vector3 ActivatedPos;

    public GameObject deathPart;

    ZoneScript Zone;

    // Use this for initialization
    void Start()
    {
  
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, (Activated) ? ActivatedPos : ActivatedPos + (Vector3.down * 50), Time.deltaTime * 10);
    }


    public UnitStruct MyUnit
    {
        get
        {
            return UnitData;
        }

        set
        {
            UnitData = value;
        }
    }

    public HealthBar myVisualizer
    {
        get
        {
           return  GetComponentInChildren<HealthBar>();
        }
    }

    public IWeapon myWeapon
    {
        get
        {
            return null;
        }
    }

    public bool Triggered
    {
        get
        {
            return Activated; 
        }

        set
        {
            Activated = value;
        }
    }


    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public void OnHit(int _damage)
    {

        Instantiate(deathPart, transform.position, Quaternion.identity);
         
        Triggered = false;
    }

 
}
