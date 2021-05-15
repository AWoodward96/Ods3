using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A class meant for utility only
/// Also define global CONSTANTS here
/// Global variables that change should be defined in the Game Manager script
/// </summary>
public class GlobalConstants {

    // How far back the camera should follow a target
    // Since the camera mode is orthographic the magnitude of this vector doesn't matter, only the ratios of the vector matter
    public static Vector3 DEFAULTFOLLOWBACK = new Vector3(0,50f,-30);


    // Since we are in this weird 2.5d world, we often find ourself using vectors that need to not take the y axis into account
    // We could use Vector2s but then the code gets confusing because the Z axis becomes the Y variable
    public static Vector3 ZeroYComponent(Vector3 _vec)
    {
        return new Vector3(_vec.x, 0, _vec.z);
    }

    // Used for rotating objects in a circle towards something like a cursor
    // An object pointing in a direction should have a sprite facing the right direction if using this method
    public static float angleBetweenVec(Vector3 vec)
    {
        float angle = Vector3.Angle(vec, Vector3.right);
        float sign = Mathf.Sign(vec.z);
        return (sign > 0) ? angle : 360 - angle;
    }

    // Gravity calculation
    // Inaccessable variables that are used to calculate the gravity of the world I want
    // float jumpHeight = 3; 
    // float timeToJumpApex = 1f;
    //public static float Gravity = (2 * jumpHeight) / Mathf.Pow(((timeToJumpApex > 0) ? timeToJumpApex : 1), 2); // This weird calculation gets me 6. So I'm just going to duplicate this line and have it be 6

    public static float Gravity = 6; // Global gravity since we're not using unitys rigidbody
    public static float Friction = .9f; // Slows down CControllers
    public static float AirFriction = 1f; // This variable is here just in case I want to change it. For now, there is no air friction


    // A helpful method to find an exact object based on a name
    // Really slow. Don't use in an update method
    public static GameObject FindGameObject(string Name)
    {
        Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();
        

        foreach (Transform obj in allObjects)
        {
            if (obj.gameObject.name == Name)
            {
 
                return obj.gameObject;
            }
        }

        return null;
    }

    // Converts a string printed value to a vector 3 provided it's in the proper unity format
    // Mostly used in the saving and loading of the game
    public static Vector3 StringToVector3(string _VectorString)
    {
        // Toss the parentheses
        if (_VectorString.StartsWith("(") && _VectorString.EndsWith(")")) 
            _VectorString = _VectorString.Substring(1, _VectorString.Length - 2); 

        // split the data
        string[] sArray = _VectorString.Split(',');

        // return it
        return new Vector3(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2])); 
    }

    /// <summary>
    /// Takes the value with based on minVal to maxVal and sets it as a value between newMinVal and newMaxVal
    /// </summary>
    /// <param name="value">The value we're warping</param>
    /// <param name="minVal">The min val associated with value</param>
    /// <param name="maxVal">The max val associated with value</param>
    /// <param name="newMinVal">The desired new minimum assocaited with value</param>
    /// <param name="newMaxVal">The desired new maximum associated witth value</param>
    /// <returns>New mapped value</returns>
    public static float Map(float value, float minVal, float maxVal, float newMinVal, float newMaxVal)
    {
        float trueVal = value - minVal;
        float ratio = trueVal / (maxVal - minVal);
        return (ratio * (newMaxVal - newMinVal)) + newMinVal;
    }

    /// <summary>
    /// Takes the value with based on minVal to maxVal and sets it as the inverse value between newMinVal and newMaxVal
    /// </summary>
    /// <param name="value">The value we're warping</param>
    /// <param name="minVal">The min val associated with value</param>
    /// <param name="maxVal">The max val associated with value</param>
    /// <param name="newMinVal">The desired new minimum assocaited with value</param>
    /// <param name="newMaxVal">The desired new maximum associated witth value</param>
    /// <returns>New mapped value</returns>
    public static float inverseMap(float value, float minVal, float maxVal, float newMinVal, float newMaxVal)
    {
        float trueVal = value - minVal;
        float ratio =  1 - (trueVal / (maxVal - minVal));
        return (ratio * (newMaxVal - newMinVal)) + newMinVal;
    }

    public static Vector3 getPhysicsArc(Vector3 myPos, Vector3 targetPos)
    {
        Vector3 p = targetPos;

        float gravity = Physics.gravity.magnitude;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(myPos.x, 0, myPos.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = myPos.y - p.y;

        // Selected angle in radians 
        float angle = GlobalConstants.inverseMap(distance, 1, 15, 45, 85) * Mathf.Deg2Rad;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > myPos.x ? 1 : -1);
        // Rotate our velocity to match the direction between the two objects 
        return Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
    }

    public static Vector3 getPhysicsArc(Vector3 myPos, Vector3 targetPos, float _angle)
    {
        Vector3 p = targetPos;

        float gravity = Physics.gravity.magnitude;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(myPos.x, 0, myPos.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = myPos.y - p.y;

        // Selected angle in radians 
        float angle = _angle * Mathf.Deg2Rad;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > myPos.x ? 1 : -1);
        // Rotate our velocity to match the direction between the two objects 
        return Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
    }

    public static bool AnimatorHasParameter(Animator _Anim, string _val)
    {
        for(int i = 0; i < _Anim.parameters.Length; i++)
        {
            if (_Anim.parameters[i].name.ToUpper() == _val.ToUpper())
                return true;
        }

        return false;
    }

    /// <summary>
    /// Author: peterho0218
    /// Found here: https://answers.unity.com/questions/1125768/how-do-i-predict-the-position-of-my-player-for-the.html 
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="shooterPosition"></param>
    /// <param name="targetVelocity"></param>
    /// <param name="projectileSpeed"></param>
    /// <returns></returns>
    public static Vector3 predictedPosition(Vector3 targetPosition, Vector3 shooterPosition, Vector3 targetVelocity, float projectileSpeed)
    {
        Vector3 localVel = targetVelocity;
        localVel.y = 0;
        Vector3 localtargetPos = targetPosition;
        localtargetPos.y = 0;
        Vector3 localShooterPos = shooterPosition;
        localShooterPos.y = 0;



        Vector3 displacement = localtargetPos - localShooterPos;
        float targetMoveAngle = Vector3.Angle(-displacement, localVel) * Mathf.Deg2Rad;
        //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
        if (localVel.magnitude == 0 || localVel.magnitude > projectileSpeed && Mathf.Sin(targetMoveAngle) / projectileSpeed > Mathf.Cos(targetMoveAngle) / localVel.magnitude)
        {
            return localtargetPos;
        } 

        //also Sine Formula
        float shootAngle = Mathf.Asin(Mathf.Sin(targetMoveAngle) * localVel.magnitude / projectileSpeed);
        return localtargetPos + localVel * displacement.magnitude / Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / localVel.magnitude;
    }

    /// <summary>
    /// Returns a built string of a specific gameobjects path
    /// from: https://answers.unity.com/questions/8500/how-can-i-get-the-full-path-to-a-gameobject.html
    /// autor: Dmitriy-Yukhanov -- https://answers.unity.com/users/35903/dmitriy-yukhanov.html
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
