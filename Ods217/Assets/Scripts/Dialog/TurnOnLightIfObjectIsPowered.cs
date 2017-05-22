using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is Logic Script
/// When a INon-Unit object is powered, turn the light on.
/// Used for special effects
/// </summary>
[RequireComponent(typeof(Light))]
public class TurnOnLightIfObjectIsPowered : MonoBehaviour {

    Light myLight;
    public GameObject INonUnitObject;
    INonUnit effector;
    bool poweredState;
    bool poweredStatePrev;

    [Range(0,1)]
    public float flickerChance;
    bool willFlicker;
    RandomBlink blinkScript;

    ZoneScript prevZone;

	// Use this for initialization
	void Start () {
        myLight = GetComponent<Light>();
        effector = INonUnitObject.GetComponent<INonUnit>();

        flickerChance = Random.Range(0f, 1f);
        if (flickerChance < .8)
        {
            willFlicker = true;
            blinkScript = gameObject.AddComponent<RandomBlink>();
            blinkScript.TimerMin = .1f;
            blinkScript.TimerMax = .3f;
            blinkScript.enabled = false;
        } 
        myLight.enabled = effector.Powered;

	}
	
	// Update is called once per frame
	void Update () {
        poweredState = effector.Powered;
        if (poweredState != poweredStatePrev)
        {
            myLight.enabled = poweredState;

            if (willFlicker)
            {
                blinkScript.enabled = true;
                StartCoroutine(blinkCRT());
            }
        }

        poweredStatePrev = poweredState;


        // Handle changing zones
        if (prevZone != ZoneScript.ActiveZone)
        {
            myLight.enabled = poweredState;
        }

        prevZone = ZoneScript.ActiveZone;
    }

    IEnumerator blinkCRT()
    {
        yield return new WaitForSeconds(Random.Range(0f, 3f));
        blinkScript.enabled = false;
        myLight.enabled = poweredState;
    }
}
