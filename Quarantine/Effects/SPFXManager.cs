using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effects Script
/// Instantiate an object that quickly destroys itself used for only effects
/// This does create game objects on the fly so don't use it too much
/// </summary>
public class SPFXManager : MonoBehaviour {

    public enum SPFX { Exclamation, Awkward };


    public static void showSPFX(SPFX _type, Vector3 _worldPosition, Vector3 _appliedForce)
    {
        showSPFX(_type, _worldPosition, _appliedForce, 2);
    }

    public static void showSPFX(SPFX _type, Vector3 _worldPosition, Vector3 _appliedForce, float _deathTimer)
    {
        switch (_type)
        {
            case SPFX.Exclamation:
                GameObject obj = Instantiate(Resources.Load("Prefabs/Particles/Exclamation"), _worldPosition, Quaternion.identity) as GameObject;
                obj.SetActive(false);
                obj.transform.localScale = new Vector3(1, 2, 1);
                SPFXObject script = obj.GetComponent<SPFXObject>();
                script.appliedForce = _appliedForce;
                script.killTimer = _deathTimer;
                obj.SetActive(true);
                break;
        }
    }
 
}
