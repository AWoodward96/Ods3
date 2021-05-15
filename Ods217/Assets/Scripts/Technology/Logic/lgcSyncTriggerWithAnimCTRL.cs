using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class lgcSyncTriggerWithAnimCTRL : MonoBehaviour, IPermanent, ISavable {

    
    public bool State;
    Animator myAnim;
    ZoneScript Zone;

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
            if (myAnim == null)
                myAnim = GetComponent<Animator>();

            myAnim.SetBool("State", State);
        }
    }

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(Triggered);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		Triggered = bool.Parse(data[0].Trim());
	}
}
