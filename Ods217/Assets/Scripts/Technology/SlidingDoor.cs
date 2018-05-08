using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour, IPermanent {

    public GameObject Door1;
    public GameObject Door2;
    public Vector3 DoorPos1True;
    public Vector3 DoorPos2True;

    public Vector3 DoorPos1False;
    public Vector3 DoorPos2False;

    public float Speed = 5;
    public bool State;
    public bool Locked;

    ZoneScript Zone;
     
      
    public ZoneScript myZone
    {
        get
        {
            return Zone;
        }

        set
        {
            Zone = value;
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
            State = value;
            if (GetComponent<AudioSource>() != null)
                GetComponent<AudioSource>().Play();
        }
    }
 

    // Update is called once per frame
    void Update()
    { 
        Door1.transform.localPosition = Vector3.Lerp(Door1.transform.localPosition, (State && !Locked) ? DoorPos1True : DoorPos1False, Speed * Time.deltaTime);
        Door2.transform.localPosition = Vector3.Lerp(Door2.transform.localPosition, (State && !Locked) ? DoorPos2True : DoorPos2False, Speed * Time.deltaTime);
    }
 

    public void Activate()
    {
       
    }
}
