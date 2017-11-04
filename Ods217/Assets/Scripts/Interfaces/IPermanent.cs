using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permanents are any object in the game that might want to interact with other Permanents in the game
/// </summary>
public interface IPermanent {

    void Activate();

    GameObject gameObject
    {
        get;
    }

    // A logic based boolean flag
    bool Triggered
    {
        get;
        set;
    } 

}
