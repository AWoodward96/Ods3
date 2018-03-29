using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetPractice : MonoBehaviour, IPermanent {

    public GameObject[] Targets;
    public int targetIndex;
    public bool triggered;
    
    public bool circuitComplete = false;
    public bool circuitComplete2 = false;



    public GameObject[] turretObjects;
    TrainingTurret1[] turretScripts;
    int turretCounter;

    [Header("Text")]
    [TextArea(1, 100)]
    public string Dialog;


    [Header("Text 2")]
    [TextArea(1, 100)]
    public string Dialog2;


    [Space(20)]
    [TextArea(1,100)]
    public string ReloadDialog;
    bool triggedReload = false;
    IArmed player;

     

    // Use this for initialization
    void Start () {
        targetIndex = 0;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<IArmed>();


        turretScripts = new TrainingTurret1[turretObjects.Length];
        for (int i = 0; i < turretObjects.Length; i++)
        {
            TrainingTurret1 t = turretObjects[i].GetComponentInChildren<TrainingTurret1>();
            turretScripts[i] = t;
        }

    }

    // Update is called once per frame
    void Update () {

        if(triggered)
        {
            if (!circuitComplete)
                falseState();
            else
                trueState();
            
            
    //        if(player == null)
    //            player = GameObject.FindGameObjectWithTag("Player").GetComponent<IArmed>();

    //        if(player.myWeapon != null)
    //        {
				//// I'm replacing this with another probably appropriate line!
				////			- Ed
    //            //if (player.myWeapon.weaponData.currentAmmo < 2 && !triggedReload)
				//if(player.MyUnit.CurrentEnergy <= player.MyUnit.MaxEnergy / 4)
    //            {
    //                CutsceneManager.instance.StartCutscene(ReloadDialog);
    //                triggedReload = true;
    //            }
    //        }
           
               
        }

	}

    void falseState()
    {
        lgcMoveToOnTrigger t = GlobalConstants.FindGameObject("WeaponHolder").GetComponent<lgcMoveToOnTrigger>();
        if (!t.triggered)
        {
            t.triggered = true;
        }


        if (Targets.Length == 0)
            return;

        if (targetIndex < Targets.Length)
        {
            IArmed armedObject = Targets[targetIndex].GetComponent<IArmed>();
            if (!armedObject.Triggered)
            {
                // Get the next armed object
                IArmed nextArmed = (targetIndex + 1 == Targets.Length) ? Targets[0].GetComponent<IArmed>() : Targets[targetIndex + 1].GetComponent<IArmed>();
                targetIndex++;

                if (targetIndex >= Targets.Length)
                {
                    targetIndex = 0;
                    circuitComplete = true;
                    circuitComplete2 = false;
                    
                    CutsceneManager.instance.StartCutscene(Dialog); 
                    return;
                }

                nextArmed.Triggered = true;
            }

        }
    }

    void trueState()
    {
        if(!CutsceneManager.InCutscene)
        {
            if (!circuitComplete2)
            {
                StartCoroutine(handleSpawn(turretObjects[0]));
                StartCoroutine(handleSpawn(turretObjects[1]));
                StartCoroutine(handleSpawn(turretObjects[2]));
                StartCoroutine(handleSpawn(turretObjects[3]));
                circuitComplete2 = true;
            }

            bool checker = false;

            for (int i = 0; i < turretScripts.Length; i++)
            {
                if (turretScripts[i].gameObject.activeSelf)
                    checker = true;
            }

            if(!checker)
            {
                triggered = false;
                CutsceneManager.instance.StartCutscene(Dialog2);
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


    IEnumerator handleSpawn(GameObject _moverObject)
    {
        _moverObject.GetComponent<lgcMoveToOnTrigger>().Triggered = true;
        sinFloat floater = _moverObject.GetComponentInChildren<sinFloat>();
        floater.enabled = false;
        yield return new WaitForSeconds(3);
        turretCounter++;
        floater.enabled = true;
        TrainingTurret1 tt1 = _moverObject.GetComponentInChildren<TrainingTurret1>();
        if(tt1 != null)
            tt1.Triggered = true;
    }

    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
