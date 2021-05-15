using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcMachineToggleSwitch : lgcSwitch
{
	public SpriteRenderer consoleRenderer;
	public Sprite moving;
	public Sprite stopped;

	public lgcMachineToggleSwitch master;			// The master switch for this switch; can turn all of its switches on at once.
	bool prevMasterState = true;					// The last known state of the master switch

	public override void Start()
	{
		base.Start();

		consoleRenderer.sprite = (Triggered) ? moving : stopped;

		if(master == null)
		{
			Debug.Log("No master switch assigned to " + gameObject.name + ". If this machine can cause a softlock, you may want to reconsider.");
		}
	}

	void Update()
	{
		// Has the master switch's state been changed?
		if(master != null && this != master && prevMasterState != master.State)
		{
			// Was it turned on?
			if(master.State == true)
			{
				// If so, turn ourself on too!
				Triggered = true;
			}

			// Update the last known state of the master switch
			prevMasterState = master.State;
		}
	}

	public override void Delegate()
	{
		// If this switch is the master switch, manually toggling it shouldn't do anything unless it's off.
		if(this != master || (this == master && !State))
		{
			base.Delegate();
		}
	}

	public override bool Triggered {
		get
		{
			return State;
		}
		set
		{
			State = value;
			consoleRenderer.sprite = (State) ? moving : stopped;

			// If we're not the master, turn the master off, since our value has been changed!
			if(State == false && master != null && this != master)
			{
				master.Triggered = false;
			}
		}
	}
}
