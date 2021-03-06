﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DetonationScript : MonoBehaviour, ISavable {

    public Canvas DetonationCanvas;
    public Text TimerText;
    public float DetonationTime = 100;
    bool detonating;

	[Header("ISavable Variables")]
	public int saveID = -1;

	[HideInInspector]
	public bool saveIDSet = false;

    PlayerScript player;

	// Use this for initialization
	void Start () {
        DetonationCanvas.enabled = false;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
	}
	
	// Update is called once per frame
	void Update () {
        if(detonating)
        { 
            DetonationTime -= Time.deltaTime;
            TimerText.text = formatTimeString();

            // Check to see if the timers run out
            if (DetonationTime <= 0)
            {
                DetonationTime = 0; 
                if(player != null)
                {
                    player.OnHit(10000);
                    player.OnHit(10000);
                } 
            }
        }
         
	} 

    public void StartDetonation()
    {
        if (detonating)
            return;

        DetonationTime = 180;
        detonating = true;
        DetonationCanvas.enabled = true;
        TimerText.text = formatTimeString();
    }

    public void StopDetonation()
    {
        detonating = false;
    }

    string formatTimeString()
    { 
        return string.Format("{0}:{1:00}", (int)DetonationTime / 60, (int)DetonationTime % 60);

    }

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(detonating);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		detonating = bool.Parse(data[0].Trim());

		if(detonating)
		{
			StartDetonation();
		}
	}

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
}
