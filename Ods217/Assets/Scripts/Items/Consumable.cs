using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Consumable : Item, IBuyable
{
	public int cost;

	protected PlayerScript player;

	[TextArea(1, 100)]
	public string[] consumeCutscenes;

	// Consumable Implementation

	// Eat the item.
	public virtual void Consume()
	{
		// Since we've eaten, adjust the variables in the game manager
		if(GameManager.currentTimeOfDay == GameManager.TimeOfDay.Morning)
		{
			GameManager.instance.ateMorning = true;
		}
		else
		{
			GameManager.instance.ateEvening = true;
		}
		GameManager.instance.starveTimer = 0;

		// Remove the object from the player's inventory.
		for(int i = 0; i < GameManager.Inventory.Count; i++)
		{
			if(GameManager.Inventory[i].GetComponent<Consumable>() == this)
			{
				GameManager.Inventory.RemoveAt(i);
				break;
			}
		}

		// Run a check to see if the effect should be applied!
		if((GameManager.currentTimeOfDay == GameManager.TimeOfDay.Morning && GameManager.instance.ateEvening)
			|| (GameManager.currentTimeOfDay == GameManager.TimeOfDay.Evening && GameManager.instance.ateMorning))
		{
			ApplyEffect();
		}

		// NOTE: For inheritors, you would play this consumption's cutscene, then increment timesConsumed here
	}

	// Have the effect wear off. Should be called when overwriting the effect in ApplyEffect, or when exiting a period without eating.
	public virtual void WearOff()
	{
		GameManager.instance.affectingFood = null;
	}

	public virtual void ApplyEffect()
	{
		if(player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		}

		if(GameManager.instance.affectingFood != null)
		{
			GameManager.instance.affectingFood.WearOff();
		}

		// Set the current food in the Game Manager to this, for effect purposes
		GameManager.instance.affectingFood = this;
	}

	// IBuyable Implementation

	public Sprite ShopIcon
	{
		get
		{
			return InventoryIcon;
		}

		set
		{
			InventoryIcon = value;
		}
	}

	public string ShopName
	{
		get
		{
			return Name;
		}

		set
		{
			Name = value;
		}
	}

	public string ShopDescription
	{
		get
		{
			return Description;
		}

		set
		{
			Description = value;
		}
	}

	public int Cost
	{
		get
		{
			return cost;
		}

		set
		{
			cost = value;
		}
	}

	public virtual int Buy()
	{
		// Player can't afford the upgrade
		if(GameManager.ScrapCount - Cost < 0)
		{
			return 1;
		}

		// No room in the inventory to take this item
		else if(GameManager.Inventory.Count == GameManager.maxInventorySize)
		{
			return 3;
		}

		// Successful purchase
		else
		{
			GameManager.Inventory.Add(GetComponent<Item>());

			GameManager.ScrapCount -= Cost;

			return 0;
		}
	}

	// ISavable Implementation
	/*public virtual string Save()
	{
		
	}
		
	public virtual void Load(string[] data)
	{
		
	}*/
}
