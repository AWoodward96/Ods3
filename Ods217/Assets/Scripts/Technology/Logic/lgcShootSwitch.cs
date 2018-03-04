using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class lgcShootSwitch : MonoBehaviour, IDamageable {

	bool isActive = false;
	UnitStruct myUnit;
	ZoneScript zone;

	public GameObject[] ObjectsToTrigger;
	IPermanent[] objectHandles;

	public AudioClip SoundToPlayWhenTriggered; 
	AudioSource mySource;

	public SpriteRenderer ConsoleRenderer;
	public Sprite Active;
	public Sprite Inactive;
	public Sprite Disabled;

	// Use this for initialization
	void Start ()
	{
		objectHandles = new IPermanent[ObjectsToTrigger.Length];

		for(int i = 0; i < ObjectsToTrigger.Length; i++)
		{
			objectHandles[i] = ObjectsToTrigger[i].GetComponent<IPermanent>();
			if(objectHandles[i] == null)
			{
				Debug.Log(ObjectsToTrigger[i].name + " has no IPermanent component!");
			}
		}

		mySource = GetComponent<AudioSource>();
		mySource.playOnAwake = false;
		mySource.spatialBlend = 1;

		mySource.clip = SoundToPlayWhenTriggered;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(objectHandles.Length == 0)
		{
			return;
		}

		// Update the sprite renderer
		if(ConsoleRenderer != null)
		{
			ConsoleRenderer.sprite = (objectHandles[0].Triggered) ? Active : Inactive;
		}
	}

	public void OnHit(int _damage)
	{
		for(int i = 0; i < objectHandles.Length; i++)
		{
			objectHandles[i].Triggered = !objectHandles[i].Triggered;
		}

		if (SoundToPlayWhenTriggered != null)
		{
			mySource.clip = SoundToPlayWhenTriggered;
			mySource.Play();
		}
	}


	public void Activate()
	{
		
	}

	public ZoneScript myZone
	{
		get{return zone;}
		set{zone = value;}
	}

	// A logic based boolean flag
	public bool Triggered
	{
		get{return false;}
		set{}
	}

	public UnitStruct MyUnit
	{
		get{return myUnit;}
		set{myUnit = value;}
	}
}
