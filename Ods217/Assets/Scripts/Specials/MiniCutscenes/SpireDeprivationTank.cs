using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpireDeprivationTank : MonoBehaviour, IPermanent {

    public GameObject CutsceneTank;
    public GameObject StaticTank;
    public Transform DropPoint;
    GameObject PlayerObj;
    public bool State;
    ZoneScript z;

    public ZoneScript myZone
    {
        get
        {
            return z;
        }

        set
        {
            z = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return State;
        }

        set
        {
            if(!State)
            {  
                StaticTank.SetActive(false);
                CutsceneTank.SetActive(true); 
                StartCoroutine(WaitUntilAnimationIsComplete());
                State = true;
            }
        }
    }

 
    IEnumerator WaitUntilAnimationIsComplete()
    {
        yield return new WaitForSeconds(3);
        GetComponent<Animator>().SetTrigger("Run");
        yield return new WaitForSeconds(17.534f); 
        StaticTank.SetActive(true);
        CutsceneTank.SetActive(false); 
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && CutsceneManager.InCutscene && CutsceneTank.activeInHierarchy)
        {
            StopAllCoroutines();
            StaticTank.SetActive(true);
            CutsceneTank.SetActive(false);
        }        
    }
}
