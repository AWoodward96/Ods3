using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface for every unit in the game
/// Units have health bars and weapons as well as a health visualizer to see this information
/// </summary>
public interface IUnit
{

    void OnDeath();
    void OnHit(IWeapon _FromWhatWeapon);

    UnitStruct MyUnit
    {
        get;
    }

    IWeapon MyWeapon
    {
        get;
    }

    HealthBar myVisualizer
    {
        get;
    }

    GameObject gameObject
    {
        get;
    }
}
