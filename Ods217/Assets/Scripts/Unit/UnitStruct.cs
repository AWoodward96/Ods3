using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A struct for keeping data on a Unit
/// </summary>
[System.Serializable]
public struct UnitStruct
{

    [Header("Meta Data")]
    public string Name;

    [Header("Stats")]
    public int MaxHealth;
    public int CurrentHealth;

}
