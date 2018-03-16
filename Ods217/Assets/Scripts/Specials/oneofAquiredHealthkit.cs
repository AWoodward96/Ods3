using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oneofAquiredHealthkit : MonoBehaviour {
    [TextArea(3, 50)]
    public string Dialog1;
    int healthkit;
    bool done;

    private void Start()
    {
        healthkit = GameManager.HealthKits;
    }

    // Update is called once per frame
    void Update () {
		if(healthkit != GameManager.HealthKits && !done)
        {
            done = true;
            CutsceneManager.instance.StartCutscene(Dialog1); 
        }
	}
}
