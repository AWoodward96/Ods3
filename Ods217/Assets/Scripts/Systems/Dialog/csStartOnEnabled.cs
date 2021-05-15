using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class csStartOnEnabled : MonoBehaviour, IPermanent, ISavable{


    [Header("Text")]
    [TextArea(3, 50)]
    public string Dialog;
    ZoneScript z;
    bool t = false; 

    [Space(40)]
    [Header("OR File")]
    public TextAsset Text;

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

    bool loaded = false;

    // Use this for initialization
    void OnEnable () {
        t = true;
        StartCoroutine(Run());
	}

    IEnumerator Run()
    {
        yield return new WaitForEndOfFrame();
         
        if (Text != null)
            CutsceneManager.instance.StartCutscene(Text.text);
        else
            CutsceneManager.instance.StartCutscene(Dialog);


        StartCoroutine(Deactivate());
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

    public bool Triggered
    {
        get
        {
            return t;
        }

        set
        {
            gameObject.SetActive(value);
        }
    }

    IEnumerator Deactivate()
    {
        // Ensure that the cutscene actually started before deactivating this object because the CS Manager.instance is slow
        while (CutsceneManager.InCutscene)
        {
            yield return null;
        }

        gameObject.SetActive(false);
    }


    public string Save()
    {
        StringWriter data = new StringWriter();

        data.WriteLine(gameObject.activeInHierarchy);

        return data.ToString();
    }

    public void Load(string[] data)
    { 
        gameObject.SetActive(bool.Parse(data[0].Trim()));
        loaded = true;
    }
}
