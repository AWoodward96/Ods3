using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class lgcShootSwitch : MonoBehaviour, IDamageable {

	public bool State = false;
	UnitStruct myUnit;
	ZoneScript zone;

	public GameObject[] ObjectsToTrigger;
	IPermanent[] objectHandles;

	public AudioClip SoundToPlayWhenTriggered; 
	AudioSource mySource;

    public SpriteRenderer OrbRenderer;
    Light OrbLight;

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

        if(OrbRenderer != null)
        {
            OrbLight = OrbRenderer.GetComponentInChildren<Light>();

            Color c = (State) ? Color.red : Color.blue;
            c.a = .5f;
            OrbRenderer.color = c;
            if (OrbLight != null)
                OrbLight.color = c;
        }
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(objectHandles.Length == 0)
		{
			return;
		}
 
	}

    public void OnMelee(int _damage)
    {
        OnHit(_damage);
    }

    public void OnHit(int _damage)
	{
        Triggered = !Triggered;
	}

	public ZoneScript myZone
	{
		get{return zone;}
		set{zone = value;}
	}

	// A logic based boolean flag
	public bool Triggered
	{
		get{return State; }
		set
        { 
            State = value;
            for (int i = 0; i < objectHandles.Length; i++)
            {
                objectHandles[i].Triggered = State;
            }

            if (SoundToPlayWhenTriggered != null)
            {
                mySource.clip = SoundToPlayWhenTriggered;
                mySource.Play();
            }

            if(OrbRenderer != null)
            {
                Color c = (State) ? Color.red : Color.blue;
                c.a = .5f;
                OrbRenderer.color = c;
                if (OrbLight != null)
                    OrbLight.color = c;
            }

        }
	}

	public UnitStruct MyUnit
	{
		get{return myUnit;}
		set{myUnit = value;}
	}
}
