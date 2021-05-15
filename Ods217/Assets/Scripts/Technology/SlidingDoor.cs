using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SlidingDoor : MonoBehaviour, IPermanent, ISavable {

    public GameObject Door1;
    public GameObject Door2;
    public Vector3 DoorPos1True;
    public Vector3 DoorPos2True;

    public Vector3 DoorPos1False;
    public Vector3 DoorPos2False;

    public float Speed = 5;
    public bool State;
    public bool Locked;

	[Header("ISavable Variables")]
	public int saveID = -1;

	[HideInInspector]
	public bool saveIDSet = false;

	public int SaveID
	{
		get
		{
			return saveID;
		}
		set
		{
			saveID = value;
		}
	}

	public bool SaveIDSet
	{
		get
		{
			return saveIDSet;
		}
		set
		{
			saveIDSet = value;
		}
	}

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

    // Save whether or not the door is open, or locked
    public string Save()
    {
		StringWriter data = new StringWriter();
	
		data.WriteLine(gameObject.activeInHierarchy);
		data.WriteLine(State);
		data.WriteLine(Locked);
	
		return data.ToString();
    }
	
    // Given a string, load it's data into this door
    public void Load(string[] data)
    {
		gameObject.SetActive(bool.Parse(data[0].Trim()));
		State = bool.Parse(data[1].Trim());
		Locked = bool.Parse(data[2].Trim());
    }
}
