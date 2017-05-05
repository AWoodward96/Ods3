using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The primary physics script for any unit object
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CController : MonoBehaviour
{
    // This script utilizes the Character Controller
    CharacterController myCtrl;

    [Header("Movement Info")]
    [Range(0, 100)]
    public float Speed;
    [Range(0, 1000)]
    public float MaxSpeed;

    public bool canMove; // Can this object move?
    public bool Airborne;

    [Header("Buffs")]
    [Range(0, 10)]
    public float SprintSpeed;
    public bool Sprinting;
    [HideInInspector]
    public bool SprintingPrev; // So we can tell when we've switched from sprinting to not sprinting


    Vector3 Acceleration;
    public Vector3 Velocity;
    public Vector3 ProjectedPosition; // Where this object should be next frame



    // Use this for initialization
    void Start()
    {
        myCtrl = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Update the projected position
        ProjectedPosition = Velocity * Time.deltaTime + transform.position;
        CalculateMove(); // This actually makes you move

        // Make sure that you're on the ground and not floating in mid air
        RaycastHit hit;
        Ray r = new Ray(transform.position + myCtrl.center, Vector3.down);
        if (!Physics.Raycast(r, out hit, .2f + myCtrl.height - myCtrl.center.y, LayerMask.GetMask("Ground")))
        {
            ApplyForce(Vector3.down * GlobalConstants.Gravity);
            Airborne = true;
        }
        else
            Airborne = false;

        // I don't like how I have to do this but hey
        SprintingPrev = Sprinting;
    }

    // Called whenever we want to move this object
    public void ApplyForce(Vector3 _spd)
    {
        if (canMove)
            Acceleration += (_spd);
    }

    // Move the actual object
    void CalculateMove()
    {
        Velocity += Acceleration;
        Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);
        myCtrl.Move(Velocity * Time.deltaTime);

        Velocity *= (Airborne) ? GlobalConstants.AirFriction : GlobalConstants.Friction;

        Acceleration = Vector3.zero;
    }
}

