using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class oneofTutorialTracker4 : MonoBehaviour, IPermanent { 
    [TextArea(3, 50)]
    public string DialogDisarmed;

    [TextArea(3, 50)]
    public string DialogArmed;

    [TextArea(3, 50)]
    public string TriggeredWhenDisarmed;


    public GameObject TutorialEnemy;

    bool triggered = false;
    bool disarmTrigger = false; 

    public bool Triggered
    {
        get
        {
            return triggered;
        }

        set
        {
            triggered = value;
        }
    }

    // Use this for initialization
    void Start () {
 

    }

    // Update is called once per frame
    void Update () {
        if(triggered)
        {
            IArmed a = TutorialEnemy.GetComponent<IMultiArmed>();

            if(a.myWeapon == null && !disarmTrigger)
            {
                CutsceneManager.instance.StartCutscene(TriggeredWhenDisarmed);
                disarmTrigger = true;
            }

            if(a.MyUnit.CurrentHealth <= 0)
            {
                if (a.myWeapon == null)
                {
                    CutsceneManager.instance.StartCutscene(DialogDisarmed);
                    triggered = false;
                    return;
                }
                else
                {
                    CutsceneManager.instance.StartCutscene(DialogArmed);
                    triggered = false;
                    return;
                }
            }
        } 
	}
 
    public void Activate()
    {
        throw new NotImplementedException();
    }

 
}
