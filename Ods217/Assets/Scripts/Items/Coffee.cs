using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coffee : Consumable
{
	static int timesConsumed = 0;
	public float speedDelta = 0.2f;	// By what value does drinking this affect the player's speed?

	public override void Consume()
	{
		base.Consume();

		CutsceneManager.instance.StartCutscene(consumeCutscenes[timesConsumed]);

		// TODO: How should we handle times consumed? For now, I'm just going to put a ceiling on it.
		timesConsumed = Mathf.Min(timesConsumed + 1, consumeCutscenes.Length - 1);
	}

	public override void ApplyEffect()
	{
		base.ApplyEffect();

		// Sprint speed increased by a ludicrous amount for testing purposes
		player.cc.Speed += speedDelta;
	}

	public override void WearOff()
	{
		base.WearOff();

		player.cc.Speed -= speedDelta;
	}
}
