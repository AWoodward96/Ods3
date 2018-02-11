using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for keeping data on a Unit
/// </summary>
[System.Serializable]
public class UnitStruct
{

    [Header("Meta Data")]
    public string Name;

    [Header("Stats")]
    public int MaxHealth;
    public int CurrentHealth;

	// Energy for shields and weapons
	public int MaxEnergy;
	public int CurrentEnergy;

}
