using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CController))] 
public class PawnScript : MonoBehaviour, IUnit{

    public UnitStruct myUnit;
    public Weapon myWeapon;
    GameObject GunObj;
    public bool StaticWeapon;

    Animator myAnimator;
    CController myCC;

    public List<PawnCommand> Commands;

    public bool UsingItem;
    public Vector3 RootPosition;
    public Vector3 HolsteredPosition;
    public Vector3 HolsteredRotation;

    Vector3 lookingVector;
    // Use this for initialization
    void Awake () {

        lookingVector =  Vector3.right;


        // Get the weapon for this object
        if (myWeapon == null)
        {
            myWeapon = GetComponentInChildren<Weapon>();
            myWeapon.Owner = this;
        }
        else
        {
            myWeapon.Owner = this;
        }

        Transform[] objs = GetComponentsInChildren<Transform>();
        foreach (Transform o in objs)
        {
            if (o.name == "GunChild")
                GunObj = o.gameObject;
        }

        myAnimator = GetComponent<Animator>();
        myCC = GetComponent<CController>();

        Commands = new List<PawnCommand>();
        //Commands.Add(new PawnCommand(PawnCommand.commandType.Move,0,0,false  ,"",new Vector3(0,0,0)));
        //Commands.Add(new PawnCommand(PawnCommand.commandType.Aim, 0, 0, false  , "", new Vector3(-1, 0, 1)));
        //Commands.Add(new PawnCommand(PawnCommand.commandType.Aim, 0, 0, false   , "", new Vector3(-1, 0, -1)));
        //Commands.Add(new PawnCommand(PawnCommand.commandType.Aim, 0, 0, false, "", new Vector3(1, 0, -1)));
        //Commands.Add(new PawnCommand(PawnCommand.commandType.Shoot, 0, 3, false, "", new Vector3(0, 0, -1)));
        //Commands.Add(new PawnCommand(PawnCommand.commandType.SetFloat, 1f, 0, false, "Special", new Vector3(0, 0, -1)));
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if(Commands.Count < 1)
        {
            return;
        }

        PawnCommand currentCommand = Commands[0];

        switch(currentCommand.cType)
        {
            case PawnCommand.commandType.Move:
                Vector3 movementVector = GlobalConstants.ZeroYComponent(currentCommand.VectorVal) - GlobalConstants.ZeroYComponent(transform.position); 
                myCC.ApplyForce(movementVector.normalized * (myCC.Speed + (myCC.Sprinting ? myCC.SprintSpeed : 0)));
                myCC.Sprinting = currentCommand.boolVal;
                if(movementVector.magnitude < .2f)
                {
                    myCC.Velocity = Vector3.zero;
                    transform.position = new Vector3(currentCommand.VectorVal.x, transform.position.y, currentCommand.VectorVal.z);
                    Commands.RemoveAt(0);
                }
                break;
            case PawnCommand.commandType.Aim:
                if (currentCommand.boolVal)
                    lookingVector = Vector3.Lerp(lookingVector, currentCommand.VectorVal, 3f * Time.deltaTime);
                else
                    lookingVector = currentCommand.VectorVal;

                if((lookingVector - currentCommand.VectorVal).magnitude < .1)
                {
                    lookingVector = currentCommand.VectorVal;
                    Commands.RemoveAt(0);
                }
                break;
            case PawnCommand.commandType.Shoot:
                lookingVector = currentCommand.VectorVal;
                myWeapon.FireWeapon(currentCommand.VectorVal);
                Commands.RemoveAt(0);
                break;
            case PawnCommand.commandType.SetBool:
                myAnimator.SetBool(currentCommand.ParameterName, currentCommand.boolVal); 
                Commands.RemoveAt(0);
                break; 
            case PawnCommand.commandType.SetFloat:
                myAnimator.SetFloat(currentCommand.ParameterName, currentCommand.floatVal);
                Commands.RemoveAt(0); 
                break;
            case PawnCommand.commandType.SetInt:
                myAnimator.SetInteger(currentCommand.ParameterName, currentCommand.intVal);
                Commands.RemoveAt(0);
                break;
            default:
                Debug.LogError("You have told the pawn script: " + this + " to do a command that isn't programmed yet. Contact Alex Woodward.");
                break;
        }

        if(!StaticWeapon)
        {
            GunObject();
        }


        // Update the animator
        // Handle walking and running bools
        myAnimator.SetFloat("SpeedX", myCC.Velocity.x);
        myAnimator.SetFloat("SpeedY", myCC.Velocity.z);
        myAnimator.SetFloat("Speed", 1);
        myAnimator.SetBool("Moving", (myCC.Velocity.magnitude > 1f));
        myAnimator.SetBool("Running", myCC.Sprinting);

    }

    void GunObject()
    {
        // Do everything related to the gun object here (idealy)
        // Make the gun look at where your cursor is
        // At a rotation of 0 the gun points right 

        // Handle Gun 'animations' 
        // Where the cursor is in world space
        Vector3 CursorLoc = transform.position + lookingVector;        
        // Handle looking up and down based on velocity
        myAnimator.SetBool("FaceFront", (CursorLoc.z < transform.position.z)); // Flips a bool switch based on if the cursor is above or below the character

        if (!myCC.Sprinting) // If we're not sprinting then the gun should rotate around the player relative to where the mouse is
        {
            // Now set up the rotating gun
            Vector3 toCursor = CursorLoc - transform.position; // This value will already have a 0'd y value :)
            toCursor = toCursor.normalized;
            // Alright now we need the angle between those two vectors and then rotate the object 
            GunObj.transform.rotation = Quaternion.Euler(0, 0, GlobalConstants.angleBetweenVec(toCursor));
            // Flip the gun if it's on the left side of the player
            GunObj.GetComponentInChildren<SpriteRenderer>().flipY = (CursorLoc.x < transform.position.x);


            Vector3 pos = RootPosition;
            if (CursorLoc.z < transform.position.z)
                pos.z = -.01f;
            else
                pos.z = .01f;
            GunObj.transform.localPosition = pos;


            Light gunShootFlare = GunObj.GetComponentInChildren<Light>();
            if (gunShootFlare != null)
                gunShootFlare.transform.localPosition = new Vector3(gunShootFlare.transform.localPosition.x, (CursorLoc.x < transform.position.x) ? -.2f : .2f, gunShootFlare.transform.localPosition.z);

        }
        else // If you're sprinting then loc the guns rotation at 20 degrees depending on which direction you're facing
        {

            //// Flip the gun if you're moving left
            GunObj.GetComponentInChildren<SpriteRenderer>().flipY = (myCC.Velocity.x < 0);

            GunObj.transform.rotation = Quaternion.Euler(HolsteredRotation);
            GunObj.transform.localPosition = HolsteredPosition;

            Vector3 pos = GunObj.transform.localPosition;

            if ((myCC.Velocity.magnitude > 1f)) // If we're moving then always put it behind the player
            {
                pos.z = .01f;
                // If we're running primarily up then put it in front of the player sprite (behind the player, but towards the camera because we're running up)
                float zval = myCC.Velocity.z;
                float xval = myCC.Velocity.x;
                if (zval > 0 && Math.Abs(xval) < zval)
                    pos.z = -.01f;

            }
            else
            {
                if (UsingItem) // If we're using an item then put it behind the player
                    pos.z = .01f;
                else
                {
                    if (CursorLoc.z < transform.position.z) // Otherwise put it behind wherever the player is facing
                        pos.z = .01f;
                    else
                        pos.z = -.01f;
                }

            }

            GunObj.transform.localPosition = pos;
        }


    }

    public UnitStruct MyUnit
    {
        get
        {
            return myUnit;
        }
    }

    public HealthVisualizer myVisualizer
    {
        get
        {
            return GetComponentInChildren<HealthVisualizer>();
        }
    }

    public Weapon MyWeapon
    {
        get
        {
            return myWeapon;
        }
    }

    public void OnDeath()
    {
         
    }

    public void OnHit(Weapon _FromWhatWeapon)
    {
        myVisualizer.ShowMenu();
        myUnit.CurrentHealth -= _FromWhatWeapon.BulletDamage;
    }

    public void syncUnits(IUnit _OtherUnit)
    {
        myUnit = _OtherUnit.MyUnit;
    }

    public void setParameter(string _boolName, bool _boolValue)
    { 
         myAnimator.SetBool(_boolName, _boolValue); 
    }

 
}

public class PawnCommand
{
    public enum commandType { Move, Aim, SetBool, SetFloat, SetInt, Shoot };
    public commandType cType;
     
    public Vector3 VectorVal;

    public string ParameterName;
    public float floatVal;  
    public int intVal;
    public bool boolVal;
    public bool sprinting;

    public PawnCommand(commandType _type, float _fVal, int _iVal, bool _bVal, string _ParamVal, Vector3 _vectorVal)
    {
        cType = _type;
        VectorVal = _vectorVal;
        ParameterName = _ParamVal;
        boolVal = _bVal;
        intVal = _iVal;
        floatVal = _fVal; 
    }

}
