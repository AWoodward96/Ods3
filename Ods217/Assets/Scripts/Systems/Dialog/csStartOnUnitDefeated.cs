using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
 
public class csStartOnUnitDefeated : MonoBehaviour, IPermanent, ISavable {
     
    
    [Header("Text")]
    [TextArea(1,100)]
    public string Dialog;

    [Space(40)]
    [Header("OR File")]
    public TextAsset Text;

    bool triggered = false;
    public GameObject[] ArmedUnits;

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
 
	}

    private void Update()
    {
        bool b = true;
        // loop through each unit in the GameObject. If they're dead, or defeated then, well trigger the cutscene
        for(int i = 0; i < ArmedUnits.Length; i++)
        {
            IArmed a = ArmedUnits[i].GetComponent<IArmed>();
            if(a.MyUnit.CurrentHealth > 0)
            {
                b = false;
            }
        }

        if(b && !triggered)
        {
            triggered = true;
            go();
        }
    }

    void go()
    { 
            if (Dialog == "")
                CutsceneManager.instance.StartCutscene(Text.text);
            else
                CutsceneManager.instance.StartCutscene(Dialog); 
    }

    public ZoneScript myZone
    {
        get { return Zone; }
        set { Zone = value; }
    }

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(triggered);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		triggered = bool.Parse(data[0].Trim());
	}
}
