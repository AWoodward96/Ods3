using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetPractice : MonoBehaviour, IPermanent {

    public GameObject[] Targets;
    public int targetIndex;
    public bool triggered;
    
    public bool circuitComplete = false;

    [Header("Text")]
    [TextArea(1, 100)]
    public string Dialog;

    [Space(20)]
    [TextArea(1,100)]
    public string ReloadDialog;
    bool triggedReload = false;
    IArmed player;

    // Use this for initialization
    void Start () {
        targetIndex = 0;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<IArmed>();
	}
	
	// Update is called once per frame
	void Update () {

        if(triggered)
        {
            lgcMoveToOnTrigger t = GlobalConstants.FindGameObject("obj_WeaponHolder").GetComponent<lgcMoveToOnTrigger>();
            if (!t.triggered)
            {
                t.triggered = true;
            }


            if (Targets.Length == 0)
                return;

            if (targetIndex < Targets.Length)
            {
                IArmed armedObject = Targets[targetIndex].GetComponent<IArmed>();
                if(!armedObject.Triggered)
                {
                    // Get the next armed object
                    IArmed nextArmed = (targetIndex + 1 == Targets.Length) ? Targets[0].GetComponent<IArmed>() : Targets[targetIndex + 1].GetComponent<IArmed>();
                    targetIndex++;

                    if (targetIndex >= Targets.Length)
                    {
                        targetIndex = 0;
                        circuitComplete = true;
                        triggered = false;
                        CutsceneManager.instance.StartCutscene(Dialog);
                        return;
                    }

                    nextArmed.Triggered = true;
                }

            }
            
            if(player == null)
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<IArmed>();

            if(player.myWeapon != null)
            {
				// I'm replacing this with another probably appropriate line!
				//			- Ed
                //if (player.myWeapon.weaponData.currentAmmo < 2 && !triggedReload)
				if(player.MyUnit.CurrentEnergy <= player.MyUnit.MaxEnergy / 4)
                {
                    CutsceneManager.instance.StartCutscene(ReloadDialog);
                    triggedReload = true;
                }
            }
           
               
        }

	}

    public void Activate()
    {
        
    }

    public bool Triggered
    {
        get
        {
            return triggered;
        }

        set
        {
            triggered = value; 
            for(int i = 0; i < Targets.Length; i++)
            {
                Targets[i].SetActive(triggered);
            }
            
        }
    }

    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
