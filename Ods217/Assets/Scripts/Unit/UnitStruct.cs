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
	public float MaxEnergy;
	public float CurrentEnergy;
     
    public UnitStruct(string _name, int _maxHealth, int _currentHealth, float _maxEnergy, float _currentEnergy)
    {
        Name = _name;
        MaxHealth = _maxHealth;
        CurrentHealth = _currentHealth;
        MaxEnergy = _maxEnergy;
        CurrentEnergy = _currentEnergy; 
    }

}
