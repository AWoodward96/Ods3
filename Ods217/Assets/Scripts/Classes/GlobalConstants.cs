using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class meant for utility only
/// Also define global CONSTANTS here
/// Global variables that change should be defined in the Game Manager script
/// </summary>
public class GlobalConstants {

    // How far back the camera should follow a target
    // Since the camera mode is orthographic the magnitude of this vector doesn't matter, only the ratios of the vector matter
    public static Vector3 DEFAULTFOLLOWBACK = new Vector3(0,12.5f,-10);


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

    // Inaccessable variables that are used to calculate the gravity of the world I want
    float jumpHeight = 3;
    float timeToJumpApex = 1f;
    public static float Gravity = (2 * 3) / Mathf.Pow(((1 > 0) ? 1 : 1), 2); // Global gravity since we're not using unitys rigidbody
    public static float Friction = .9f; // Slows down CControllers
    public static float AirFriction = 1f; // This variable is here just in case I want to change it. For now, there is no air friction


    // A helpful method to find an exact object based on a name
    // Really slow. Don't use in an update method
    public static GameObject FindGameObject(string Name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == Name)
                return obj;
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

}
