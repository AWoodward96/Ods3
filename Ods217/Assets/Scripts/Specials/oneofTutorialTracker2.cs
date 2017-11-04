using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class oneofTutorialTracker2 : MonoBehaviour {

    public bool triggered;
 
    [TextArea(3, 50)]
    public string Dialog1; 

    [TextArea(3, 50)]
    public string Dialog2; 

    [TextArea(3, 50)]
    public string Dialog3; 

    BoxCollider myCollider;

    int triggerValue;

    // Use this for initialization
    void Start () {
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {

        if(triggerValue == 1)
        {
            triggerValue++;
            CutsceneManager.instance.StartCutscene(Dialog1);
        }

        if (triggerValue == 3)
        {
            triggerValue++;
            CutsceneManager.instance.StartCutscene(Dialog2);
        }

        if (triggerValue == 5)
        {
            triggerValue++;
            CutsceneManager.instance.StartCutscene(Dialog3);
        }
         
	}


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            triggerValue++;
        }
    }
}
