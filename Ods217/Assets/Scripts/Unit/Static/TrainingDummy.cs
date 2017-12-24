using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IArmed {

    public UnitStruct UnitData;
    public ForceFieldScript myForceField;
    public GameObject MyWeapon;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
            return GetComponentInChildren<HealthBar>();
        }
    }

    public WeaponBase myWeapon
    {
        get
        {
            return MyWeapon.GetComponent<WeaponBase>();
        }
    }

    public bool Triggered
    {
        get
        {
            return false;
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public void OnHit(int _damage)
    {
        // Badoop badoop you were hit by a bullet :)
        // Take damage why did I add a smiley you know what it doesn't matter
        myVisualizer.ShowMenu();
         

        if (myForceField)
        {
            if (myForceField.Health > 0)
                myForceField.RegisterHit(_damage);
            else
                UnitData.CurrentHealth -= _damage;
        }
        else
            UnitData.CurrentHealth -= _damage;
    }


    public virtual void TossWeapon()
    {

    }

    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
