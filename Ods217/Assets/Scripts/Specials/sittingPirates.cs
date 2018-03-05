using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sittingPirates : MonoBehaviour {

    public GameObject[] Units;
    public lgcMoveToOnTrigger TriggerMe;
    public csStartOnCollision CutsceneTrigger;
    bool broken = false;
 
	// Update is called once per frame
	void Update () {
        if(!broken)
        {
            for (int i = 0; i < Units.Length; i++)
            {
                mobCowardUnit c = Units[i].GetComponent<mobCowardUnit>();
                if (c.AIState == AIStandardUnit.EnemyAIState.Vulnerable || c.AIState == AIStandardUnit.EnemyAIState.Aggro)
                {
                    DisableCutscene();
                    return;
                }
            } 
        }
		
	}

    void DisableCutscene()
    { 
        broken = true;
        TriggerMe.triggered = true;
        CutsceneTrigger.gameObject.SetActive(false);
        for (int i = 0; i < Units.Length; i++)
        {
            mobCowardUnit c = Units[i].GetComponent<mobCowardUnit>();
            c.AIState = AIStandardUnit.EnemyAIState.Vulnerable;
        }
    }
}
