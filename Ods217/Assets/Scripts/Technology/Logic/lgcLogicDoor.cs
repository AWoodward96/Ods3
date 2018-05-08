using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  A Logic Script for Doors
///  Loops through all the Objects, it they're IPermenants then check if they're triggered, if they're IDamageable then check if they're alive
///  Set an objects trigger state to true only if all IDamageable's are dead and/or all IPermenants are triggered
/// </summary>
public class lgcLogicDoor: SlidingDoor {
     
	public List<GameObject> TriggerObjects;

    List<IDamageable> harmTriggers;
    List<IPermanent> triggers;
    AudioSource mySource;

	bool wasTriggered;

	void Awake()
	{
        harmTriggers = new List<IDamageable>();
        triggers = new List<IPermanent>();
        for (int i = 0; i < TriggerObjects.Count; i++)
		{
            IDamageable arm = TriggerObjects[i].GetComponent<IDamageable>();
            if(arm != null)
            {
                harmTriggers.Add(arm);
                continue;
            }


            IPermanent perm = TriggerObjects[i].GetComponent<IPermanent>();
            if(perm != null)
            {
                triggers.Add(perm);
            }
		}

		wasTriggered = false;
        mySource = GetComponent<AudioSource>();
	}

    // Update is called once per frame
    void Update()
    {
        //bool Moving = Vector3.Magnitude((Door1.transform.localPosition - ((State && !Locked) ? DoorPos1True : DoorPos1False))) > .1f;
        //if (Moving && mySource != null)
        //    mySource.Play();


        Door1.transform.localPosition = Vector3.Lerp(Door1.transform.localPosition, (State && !Locked) ? DoorPos1True : DoorPos1False, Speed * Time.deltaTime);
        Door2.transform.localPosition = Vector3.Lerp(Door2.transform.localPosition, (State && !Locked) ? DoorPos2True : DoorPos2False, Speed * Time.deltaTime);
 
        bool done = true;
        // First, check and see if any harmables are dead. If so, we're done
        for (int i = 0; i < harmTriggers.Count; i++)
        {
            if (harmTriggers[i].MyUnit.CurrentHealth == 0)
            {
                done = false;
                break;
            }
        }
  
        // Then check to see if the triggered list is full of triggered objects
        for (int i = 0; i < triggers.Count; i++)
        {
            // If any of the triggers aren't set off, then the door won't open.
            if (!triggers[i].Triggered)
            {
                done = false;
                break;
            }
        }

		// If we're done, we need to make sure the timed switches don't close the door again!
		// We only want to spend time doing this once, though
		if(done && !wasTriggered)
		{
			for(int i = 0; i < TriggerObjects.Count; i++)
			{
				lgcTimedSwitch mySwitch = TriggerObjects[i].GetComponent<lgcTimedSwitch>();
                if (mySwitch != null)
                {
                    mySwitch.IsCompleted = true;
                }
            }

            CutsceneManager.instance.StartCutscene(
            "LoadPerm(" + gameObject.name + ",me)\n" + "HaltPlayer()\n" +
            "CameraTarget(Me)\n" +
            "Wait(3)\n" +  
            "CameraTarget(Player)"
            );

            wasTriggered = done;
            StartCoroutine(crtDone(done));
        }
         
    }

    IEnumerator crtDone(bool done)
    {
        yield return new WaitForSeconds(1.5f);
        if (mySource != null) ;
        mySource.Play();
        State = done; 
    }
}
