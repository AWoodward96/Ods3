using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// A Logic Script
/// A Simple Boolean switch system for other objects to build off of
/// </summary>
public class lgcSwitch : MonoBehaviour, IPermanent, ISavable {

    ZoneScript z;
    public bool State;
    [HideInInspector]
    public UsableIndicator ind;

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
            return z;
        }

        set
        {
            z = value;
        }
    }

    public virtual bool Triggered
    {
        get
        {
            return State;
        }

        set
        {
            State = value;
        }
    }

    public virtual void Start()
    {
        ind = GetComponentInChildren<UsableIndicator>();
        ind.Output = Delegate;
    }

    public virtual void Delegate()
    {
        Triggered = !Triggered;
    }
  
	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(State);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		State = bool.Parse(data[0].Trim());
	}
}
