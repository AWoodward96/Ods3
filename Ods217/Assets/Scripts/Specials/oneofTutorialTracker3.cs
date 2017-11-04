using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class oneofTutorialTracker3 : MonoBehaviour, IPermanent {

    public bool triggered;
 
    [TextArea(3, 50)]
    public string Dialog1;

    [TextArea(3, 50)]
    public string Dialog2;


    [TextArea(3, 50)]
    public string Dialog3;

    public GameObject[] turretObjects;
    TrainingTurret1[] turretScripts;
    int turretCounter = 0;

    BoxCollider myCollider;

    float time;

    bool catchBool1;
    bool catchBool2;
    bool catchBool3;
    bool catchBool4;

    public sinFloat weaponpedistalFloatys;

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
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
        weaponpedistalFloatys.enabled = false;

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
            time += Time.deltaTime;

            if(!catchBool3)
            {
                for (int i = 0; i < turretCounter; i++)
                {
                    TrainingTurret1 t = turretObjects[i].GetComponentInChildren<TrainingTurret1>();
                    if(t != null)
                        t.triggered = true;
                }
            }
           

            if(time > 15 && !catchBool1)
            {
                catchBool1 = true;
                StartCoroutine(handleSpawn(turretObjects[1]));
            }

            if (time > 30 && !catchBool2)
            {
                catchBool2 = true;
                StartCoroutine(handleSpawn(turretObjects[2]));
            }


            if(time > 35 && !catchBool3)
            {
                catchBool3 = true;
                CutsceneManager.instance.StartCutscene(Dialog2);
                StartCoroutine(handleFloaties());
            }

            checkTurretsIfAlive();
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
    }

    IEnumerator handleFloaties()
    {
        yield return new WaitForSeconds(3);
        weaponpedistalFloatys.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !triggered)
        { 
            CutsceneManager.instance.StartCutscene(Dialog1);
            StartCoroutine(handleSpawn(turretObjects[turretCounter]));
        }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    void checkTurretsIfAlive()
    {
        int checker = 0;
    
        for(int i = 0; i < turretScripts.Length; i++)
        {
            if (!turretScripts[i].gameObject.activeSelf)
                checker++;
        }

        if(checker >= 3 && !catchBool4)
        {
            catchBool4 = true;
            catchBool3 = true;
            CutsceneManager.instance.StartCutscene(Dialog3);
        }
    }
}
