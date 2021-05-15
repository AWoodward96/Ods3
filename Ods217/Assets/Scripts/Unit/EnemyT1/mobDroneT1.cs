using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class mobDroneT1 : MonoBehaviour, IArmed {

    public UnitStruct UnitData;
    public WeaponBase Weapon;
    public bool Activated;
    int flipDirection = 1;
    ZoneScript Zone;
 
    public IArmed Target;

    Rigidbody myRGB;

    const float DESIREDRADIUS = 1.5f;
    const float RADVECWEIGHT = 1.5f;
    const float ROTVECWEIGHT = 3.5f;
    const float NOISEWEIGHT = .5f;

    Vector3 targetPos;

    //public GameObject Vis1;
    //public GameObject Vis2;


    // Use this for initialization
    void Start()
    { 
        Weapon.myOwner = this;
         

        myRGB = GetComponent<Rigidbody>();
        Target = GameObject.FindGameObjectWithTag("Player").GetComponent<IArmed>();
    }

    
    public virtual void UpdateDrone()
    {

        // The goal of the mobDroneT1 is to circle around the player always trying to dodge it's aim
        // Keep a distance from the player but try to not stay within its sights
        if(Target != null)
            targetPos = Target.gameObject.transform.position;

        if (myRGB == null)
            myRGB = GetComponent<Rigidbody>();

        Vector3 radiusVector = getRadiusVector(); // Stay at a distance
        Vector3 rotationVector = getRotationVector(); // Rotate around the player
        Vector3 noiseVector = getNoiseVector();

        Vector3 moveVector = (radiusVector * RADVECWEIGHT) + (rotationVector * ROTVECWEIGHT) + (noiseVector * NOISEWEIGHT);
        myRGB.AddForce(moveVector);

        if (Mathf.Abs(transform.position.y - targetPos.y) > .3f)
            myRGB.AddForce(0, (targetPos.y - transform.position.y), 0);

        myRGB.velocity *= .975f;

        // Try to shoot at the player
        Weapon.FireWeapon(GlobalConstants.ZeroYComponent(targetPos - transform.position));

        // Rotate the weapon to shoot at the player
        Weapon.RotateObject.transform.localRotation =  Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(GlobalConstants.ZeroYComponent((targetPos - transform.position).normalized))); // NOT local because if it's local then it'll be relative to the rest of this objects rotation. Set it globally


    }

    Vector3 getRadiusVector()
    {
        Vector3 desiredPos = Vector3.zero;

        // Get the distance vector from the player to this object (zeroing Y) 
        Vector3 distVec = transform.position - targetPos;
        distVec = GlobalConstants.ZeroYComponent(distVec);

        // Get the position from the player at the desired distance
        distVec = distVec.normalized * DESIREDRADIUS;
        desiredPos = distVec + targetPos;

        //Vis1.transform.position = desiredPos;

        return GlobalConstants.ZeroYComponent(desiredPos - transform.position);
    }

    Vector3 getNoiseVector()
    {
        Vector3 desiredPos = UnityEngine.Random.insideUnitCircle;
        desiredPos = new Vector3(desiredPos.x, 0, desiredPos.y).normalized; 
        return desiredPos;
    }
  
    Vector3 getRotationVector()
    { 
        Vector3 desiredVector = targetPos - transform.position; 

        //Vis2.transform.position = transform.position + new Vector3(-desiredVector.z, 0, desiredVector.x);

        return new Vector3(flipDirection * desiredVector.z, 0, desiredVector.x).normalized;
    }

    public virtual IArmed myTarget
    {
        get { return Target; } 
        set {  Target = value; }
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
            return Weapon;
        }
    }

    public ZoneScript myZone
    {
        get
        {
            return Zone;
        }

        set
        {
            Zone = value;
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
            if (Activated)
                Activate();
        }
    }

    public void Activate()
    {
        Activated = true;
    }

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    } 

    public void OnHit(int _damage)
    {

        UnitData.CurrentHealth -= _damage;
        myVisualizer.ShowMenu();

        if (UnitData.CurrentHealth <= 0)
        {
            GameObject obj = Resources.Load("Prefabs/Particles/deathPartParent") as GameObject;
            Instantiate(obj, transform.position, obj.transform.rotation);

            if (myWeapon != null)
                myWeapon.ReleaseWeapon(); 

            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        flipDirection *= -1; 
    }
     
}
