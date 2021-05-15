using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuyable
{
	Sprite ShopIcon
	{
		get;
		set;
	}

	string ShopName
	{
		get;
		set;
	}

	string ShopDescription
	{
		get;
		set;
	}

	int Cost
	{
		get;
		set;
	}

	int Buy();
}
