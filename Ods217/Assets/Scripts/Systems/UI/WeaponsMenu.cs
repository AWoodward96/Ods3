using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A Menu Class
/// Used on the [ESC] item menu
/// Shows your current loadout and stats
/// </summary>
public class WeaponsMenu : MonoBehaviour
{
	public static WeaponsMenu instance;

	WeaponBase primaryWeapon;
	WeaponBase secondaryWeapon;
	WeaponBase displayedWeapon;

	Image theOutput;
	Button btnSwitch;

	List<Text> theStatOutputs;

	PlayerScript player;

	// Use this for initialization
	void Awake()
	{
		// Ensure there's only one instance of this script
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);

		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

		// Get the player's current weapons
		primaryWeapon = PlayerRef.PrimaryWeapon;
		secondaryWeapon = PlayerRef.SecondaryWeapon;
		displayedWeapon = PlayerRef.ActiveWeapon;

		// Get the weapon display image
		theOutput = transform.Find("CurrentWeapon").GetComponent<Image>();

		// Get the switch weapon button
		btnSwitch = transform.Find("WeaponSwitch").GetComponent<Button>();

		// Initialize stat outputs
		theStatOutputs = new List<Text>();

		Transform statsSection = transform.Find("Stats");
		Text[] temp = statsSection.GetComponentsInChildren<Text>();
		for(int i = 0; i < temp.Length - 1; i++)
		{
			if(temp[i] == statsSection.GetComponent<Text>())
			{
				continue;
			}

			theStatOutputs.Add(temp[i]);
		}
	}


	void Update()
	{

	}

	public void UpdateWeaponsMenu()
	{
		// Update the PlayerRef's weapons!
		primaryWeapon = PlayerRef.PrimaryWeapon;
		secondaryWeapon = PlayerRef.SecondaryWeapon;
		displayedWeapon = PlayerRef.ActiveWeapon;

		if(primaryWeapon == null || secondaryWeapon == null)
		{
			btnSwitch.interactable = false;
			btnSwitch.GetComponentInChildren<Text>().color = new Color(0.44140625f, 0.45703125f, 0.5078125f);
		}
		else
		{
			btnSwitch.interactable = true;
			btnSwitch.GetComponentInChildren<Text>().color = new Color(1.0f, 1.0f, 1.0f);
		}

		if(displayedWeapon != null)
		{
			theOutput.enabled = true;
			theOutput.sprite = displayedWeapon.ThrownObject.GetComponent<SpriteRenderer>().sprite;

			theStatOutputs[0].text = displayedWeapon.weaponData.name;
			theStatOutputs[1].text = "Shot Cost: " + displayedWeapon.weaponData.shotCost;
			theStatOutputs[2].text = "Cooldown: " + displayedWeapon.weaponData.fireCD;
			theStatOutputs[3].text = "Damage: " + displayedWeapon.weaponData.bulletDamage;
		}
		else
		{
			theOutput.enabled = false;

			theStatOutputs[0].text = "No Weapon";
			theStatOutputs[1].text = "Shot Cost: ";
			theStatOutputs[2].text = "Cooldown: ";
			theStatOutputs[3].text = "Damage: ";
		}
	}

	public void SwitchWeapons()
	{
		if(displayedWeapon == primaryWeapon)
		{
			displayedWeapon = secondaryWeapon;
		}
		else
		{
			displayedWeapon = primaryWeapon;
		}

		if(displayedWeapon != null)
		{
			theOutput.enabled = true;
			theOutput.sprite = displayedWeapon.ThrownObject.GetComponent<SpriteRenderer>().sprite;

			theStatOutputs[0].text = displayedWeapon.weaponData.name;
			theStatOutputs[1].text = "Shot Cost: " + displayedWeapon.weaponData.shotCost;
			theStatOutputs[2].text = "Cooldown: " + displayedWeapon.weaponData.fireCD;
			theStatOutputs[3].text = "Damage: " + displayedWeapon.weaponData.bulletDamage;
		}
		else
		{
			theOutput.enabled = false;

			theStatOutputs[0].text = "No Weapon";
			theStatOutputs[1].text = "Shot Cost: ";
			theStatOutputs[2].text = "Cooldown: ";
			theStatOutputs[3].text = "Damage: ";
		}
	}

	PlayerScript PlayerRef
	{
		get
		{
			if(player == null)
			{
				player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
			}

			return player;
		}
	}
}
