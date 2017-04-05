using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConstants {

    
    public static Vector3 DEFAULTFOLLOWBACK = new Vector3(0,10,-8);


    public static Vector3 ZeroYComponent(Vector3 _vec)
    {
        return new Vector3(_vec.x, 0, _vec.z);
    }

    public static float angleBetweenVec(Vector3 vec)
    {
        float angle = Vector3.Angle(vec, Vector3.right);
        float sign = Mathf.Sign(vec.z);
        return (sign > 0) ? angle : 360 - angle;
    }

    public enum Alligience { Ally, Enemy };

    float jumpHeight = 3;
    float timeToJumpApex = 1f;
    public static float Gravity = (2 * 3) / Mathf.Pow(((1 > 0) ? 1 : 1), 2);
    public static float Friction = .9f;
    public static float AirFriction = .95f;


}
