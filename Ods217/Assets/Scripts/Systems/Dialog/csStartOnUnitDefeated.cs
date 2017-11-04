using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
 
public class csStartOnUnitDefeated : MonoBehaviour, IPermanent {
     
    
    [Header("Text")]
    [TextArea(1,100)]
    public string Dialog;

    [Space(40)]
    [Header("OR File")]
    public TextAsset Text;

    bool triggered = false;
    public GameObject[] ArmedUnits;

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

    private void Update()
    {
        bool b = true;
        // loop through each unit in the GameObject. If they're dead, or defeated then, well trigger the cutscene
        for(int i = 0; i < ArmedUnits.Length; i++)
        {
            IArmed a = ArmedUnits[i].GetComponent<IArmed>();
            if(a.MyUnit.CurrentHealth > 0)
            {
                b = false;
            }
        }

        if(b && !triggered)
        {
            triggered = true;
            go();
        }
    }

    void go()
    { 
            if (Dialog == "")
                CutsceneManager.instance.StartCutscene(Text.text);
            else
                CutsceneManager.instance.StartCutscene(Dialog); 
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }
}
