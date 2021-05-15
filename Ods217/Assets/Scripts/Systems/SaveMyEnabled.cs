using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Attach this to an object that has no other ISavable scripts, but still needs its activeInHierarchy value saved for whatever reason
public class SaveMyEnabled : MonoBehaviour, ISavable
{
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

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(gameObject.activeSelf);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		gameObject.SetActive(bool.Parse(data[0].Trim()));
	}
}
