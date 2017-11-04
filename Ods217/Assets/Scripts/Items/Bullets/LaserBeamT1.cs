using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamT1 : MonoBehaviour, IBullet {


    [Header("Bullet Data")]
    public Vector3 Direction; // Which direction this bullet will move in 
    float currentLife;

    public bool Fired; 
    IArmed Owner; // Which unit fired it 

    public LineRenderer mainLineRenderer;
    public Color mainLineColor;
    public int NumberOfDivisions;

    public ParticleSystem EndPointPartSystem;
    public ParticleSystem LinePartSystem;
    Material myMaterial;

    public Color[] FlickerColors;
    public Vector2 PingPongWidth;
    public float currentSize;

    WeaponInfo myWeapon;

    // Use this for initialization
    void Awake()
    {
        myMaterial = GetComponent<Renderer>().material; 

        Fired = false;
        EndPointPartSystem.Play();
        LinePartSystem.Play();
        mainLineRenderer.enabled = true; 
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet if it's fired
        if (Fired)
        {
            //OK so get a layer mask and a raycast hit
            LayerMask unitsAndGround = LayerMask.GetMask("Ground") | LayerMask.GetMask("Units");
            RaycastHit hit;

            // Set up the ray and get some other crucial positions
            Vector3 OwnerPos = transform.position;
            Vector3 direction = (GlobalConstants.ZeroYComponent(CamScript.CursorLocation) - GlobalConstants.ZeroYComponent(OwnerPos));
            Ray r = new Ray(OwnerPos, direction);

            // Find the correct point
            Vector3 endPoint;
            if(Physics.Raycast(r,out hit, Mathf.Min(direction.magnitude,10), unitsAndGround)) // First raycast out either to your cursor or 10 units in your cursors direction
            {
                endPoint = hit.point; 

                // If you hit something that can be damaged damage it
                IDamageable dmg = hit.transform.GetComponent<IDamageable>();
                if(dmg != null)
                {
                    // Don't trigger a hit if you hit yourself
                    if (dmg != Owner) 
                        dmg.OnHit(myWeapon.bulletDamage);
                }
            }
            else
            {
                // If you don't hit anything, then well there you go
                endPoint = OwnerPos + ((Direction * 10).magnitude < direction.magnitude ? (Direction * 10) : direction);
            }

            EndPointPartSystem.transform.position = endPoint;
            EndPointPartSystem.transform.rotation = Quaternion.LookRotation(-direction);

            Vector3 distanceVector = endPoint - OwnerPos;
            ParticleSystem.ShapeModule shape = LinePartSystem.shape;
            shape.length = distanceVector.magnitude;
            LinePartSystem.gameObject.transform.position = (distanceVector / 2) + OwnerPos;

            Vector3 V1 = OwnerPos + Vector3.right;
            Vector3 V2 = OwnerPos + endPoint;
            float angle = Vector3.Angle(V1, V2);

             
            LinePartSystem.gameObject.transform.rotation = Quaternion.Euler(0, -GlobalConstants.angleBetweenVec(endPoint - OwnerPos), 0);

            currentSize = Mathf.PingPong(Time.time * 5, .3f) + .1f;
            mainLineRenderer.startWidth = currentSize;
            mainLineRenderer.endWidth = currentSize;
            float rndAlpha = UnityEngine.Random.Range(.3f, .8f);
            Color rndColor = FlickerColors[UnityEngine.Random.Range(0, FlickerColors.Length)];
            mainLineRenderer.startColor = rndColor;
            Color rndColor2 = FlickerColors[UnityEngine.Random.Range(0, FlickerColors.Length)];
            mainLineRenderer.endColor = rndColor2;

            AnimateLightning(mainLineRenderer, OwnerPos, endPoint, NumberOfDivisions); 
        }
         
       

        if (currentLife > .1f)
            Fired = false;
        
        // If it's lived too long kill it
        currentLife += Time.deltaTime;
        ParticleSystem.EmissionModule mod = EndPointPartSystem.emission;
        mod.enabled = Fired;
        ParticleSystem.EmissionModule mod2 = LinePartSystem.emission;
        mod2.enabled = Fired;
        mainLineRenderer.enabled = Fired;
    }

    public bool CanShoot
    {
        get { return true; }
    }

    IArmed IBullet.Owner
    {
        get
        {
            return Owner;
        }

        set
        {
            Owner = value;
        }
    }

    public WeaponInfo WeaponData
    {
        get
        {
            return myWeapon;
        }

        set
        {
            myWeapon = value;
        }
    }

    // Align this object to that direction, enable it and set fired to true 
    public void Shoot(Vector3 _dir)
    {
        currentLife = 0;
        Direction = _dir.normalized; 
        Fired = true; 
    }

    // When this hits something
    public void OnTriggerEnter(Collider other)
    {
        // Don't trigger a hit if you hit yourself
        if (other.GetComponent<IArmed>() == Owner)
            return;


        // If it's not able to be damaged
        if (other.GetComponent<IDamageable>() != null)
        {
            // Hey maybe the non unit has something that it does when it's hit by a bullet
            IDamageable u = other.GetComponent<IDamageable>();

            u.OnHit(myWeapon.bulletDamage); 
        }

        
    }

    public void setOwner(IArmed _Owner)
    {
        Owner = _Owner;
    }


    void AnimateLightning(LineRenderer _Line, Vector3 _Start, Vector3 _End, int _numOfDivisions)
    {
        Vector3[] lightningPoints = new Vector3[_numOfDivisions];
        lightningPoints[0] = _Start;
        lightningPoints[_numOfDivisions -1] = _End;

        Vector3 distanceVector = _End - _Start;
        Vector3 perp = distanceVector.normalized;
        perp = new Vector3(-perp.x, perp.y, perp.z);

        mainLineRenderer.SetPosition(0, _Start);
        mainLineRenderer.SetPosition(1, _End);
         
    }

    public void setWeapon(WeaponInfo _weaponData)
    {
        myWeapon = _weaponData;
    }
}
