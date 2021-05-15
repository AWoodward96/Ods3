using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldSwitchListener : MonoBehaviour {

    public HoldSwitch Switch;
    public HoldSwitchListenerEntry[] Entries;


	// Update is called once per frame
	void Update () {
		foreach(HoldSwitchListenerEntry e in Entries)
        {
            if (Switch.myValue > e.Threshold)
                e.Obj.SetActive(true); 
        }
	}
}

[System.Serializable]
public struct HoldSwitchListenerEntry
{
    public GameObject Obj;
    public float Threshold;
}