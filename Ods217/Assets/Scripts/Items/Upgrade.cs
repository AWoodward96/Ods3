using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used only in prefabs, to be loaded into the shop script
/// Will also be applied to the player
/// </summary>
public class Upgrade : MonoBehaviour, IBuyable
{
	public Sprite icon;

	public int ID;
	public string name;
	[TextArea(1, 3)]
	public string description;
	public int cost;

	protected PlayerScript player;

	public virtual void Apply()
	{
		Debug.Log("This upgrade was not given an override for Apply(); it won't have any effect.");	
	}

	public virtual void Remove()
	{
		Debug.Log("This upgrade was not given an override for Remove(); it cannot be undone.");
	}

	public Sprite ShopIcon
	{
		get
		{
			return icon;
		}

		set
		{
			icon = value;
		}
	}

	public string ShopName
	{
		get
		{
			return name;
		}

		set
		{
			name = value;
		}
	}

	public string ShopDescription
	{
		get
		{
			return description;
		}

		set
		{
			description = value;
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
		Debug.Log("This upgrade wasn't given a Buy function, so it is unpurchasable.");

		return -1;
	}
}
