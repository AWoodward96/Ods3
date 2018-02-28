using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class oneofTutorialTracker1 : MonoBehaviour, IPermanent {

    public bool triggered;

    bool withinCollider;

    [TextArea(3, 50)]
    public string Dialog1;



    [TextArea(3, 50)]
    public string Dialog2;


    [TextArea(3, 50)]
    public string Dialog3;

    public float timeInCollider;

    BoxCollider myCollider;

    bool fired1 = false;
    bool fired2 = false;

    // Use this for initialization
    void Start () {
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (withinCollider)
        {
            timeInCollider += Time.deltaTime;
            if(timeInCollider > 14 && !fired1)
            { 
                CutsceneManager.instance.StartCutscene(Dialog1);
                fired1 = true;
            }

            if(timeInCollider > 24 && !fired2)
            {
 
                CutsceneManager.instance.StartCutscene(Dialog2);
                fired2 = true; 
            }

            if (timeInCollider > 37)
            {
                CutsceneManager.instance.StartCutscene(Dialog3); 
                withinCollider = false;
                myCollider.enabled = false;
            }
        }
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
        }
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && triggered)
        {
            withinCollider = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player" && triggered)
        {
            timeInCollider = 0;
            withinCollider = false;
        }
    }

    ZoneScript Zone;
    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }
}
