using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpirePlayerArmedWatcher : MonoBehaviour {

    public weaponDispenser disp; 

 
	// Update is called once per frame
	void Update () {
        if (!disp.hasWeapon)
            GameManager.Events["ARMED FOR LEVEL 1"] = true;
	}
}
