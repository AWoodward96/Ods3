using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface for any object that can be interacted with that isn't a unit
/// </summary>
public interface INonUnit
{

    /// <summary>
    /// OnHit is called when a normal bullet hits this object
    /// </summary>
    void OnHit();
    /// <summary>
    /// OnEMP is called when an EMP bullet hits this object
    /// </summary>
    void OnEMP();


    bool Powered
    {
        get;
        set;
    }


    bool Triggered
    {
        get;
        set;
    }
}
