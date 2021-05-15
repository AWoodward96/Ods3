using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burger : Consumable
{
	static int timesConsumed = 0;
	public int dmgDelta = 3;	// By what value does eating this affect the player's punch damage?
	public float cdDelta = -0.11f;	// By what value does eating this affect the player' punch cooldown length?

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

		player.punchDamage += dmgDelta;
		player.punchCD += cdDelta;
	}

	public override void WearOff()
	{
		base.WearOff();

		player.punchDamage -= dmgDelta;
		player.punchCD -= cdDelta;
	}
}
