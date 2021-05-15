using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyUpgrade : Upgrade
{
	public int amount;

	public override void Apply()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

		player.myUnit.MaxEnergy += amount;
		player.myUnit.CurrentEnergy = player.myUnit.MaxEnergy;
	}

	public override void Remove()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

		player.myUnit.MaxEnergy -= amount;
		player.myUnit.CurrentEnergy = player.myUnit.MaxEnergy;
	}

	public override int Buy()
	{
		// Can't buy a second upgrade because that'd be a waste of money
		for(int i = 0; i < GameManager.instance.ownedUpgrades.Count; i++)
		{
			if(GameManager.instance.ownedUpgrades[i] == this)
			{
				return 2;
			}	
		}

		// Player can't afford the upgrade
		if(GameManager.ScrapCount - Cost < 0)
		{
			return 1;
		}

		// Successful purchase
		else
		{
			GameManager.instance.ownedUpgrades.Add(this);

			GameManager.ScrapCount -= Cost;

			// TODO: Remove this once the upgrade station is up and running!
			GameManager.instance.EquipUpgrade(this);

			return 0;
		}
	}
}
