using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UnitStruct  {

    [Header("Meta Data")]
    public string Name;

    [Header("Stats")]
    public int MaxHealth;
    public int CurrentHealth;

}
