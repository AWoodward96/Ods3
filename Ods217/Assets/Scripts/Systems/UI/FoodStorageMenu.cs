using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodStorageMenu : MonoBehaviour
{
	public bool MenuOpen;

	public GameObject myFridge;
	UsableIndicator ind_Interactable;

	PlayerScript myPlayer;

	public GameObject myItemsContainer;
	public GameObject storedItemsContainer;

	List<Image> myItems;
	List<Image> storedItems;

	Sprite emptySlot;

	public Button btnDeposit;
	public Button btnWithdraw;

	// Use this for initialization
	void Start ()
	{
		// Set up other references
		ind_Interactable = myFridge.GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
		ind_Interactable.Output = openMenuDelegate; 
		myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		MenuOpen = false;

		myItems = new List<Image>();
		myItemsContainer.GetComponentsInChildren<Image>(myItems);
		myItems.RemoveAt(0);	// Don't count the background as an image

		storedItems = new List<Image>();
		storedItemsContainer.GetComponentsInChildren<Image>(storedItems);
		storedItems.RemoveAt(0);

		emptySlot = myItems[0].sprite;
	}

	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Escape) && MenuOpen)
		{
			MenuOpen = false;
			myPlayer.AcceptInput = true;
		}
			
		gameObject.SetActive(MenuOpen);
	}

	public void UpdateItems()
	{
		// Go through and update all the inventory slots
		int offset = 0;

		for(int i = 0; i - offset < myItems.Count; i++)
		{
			// Firstly, are we within the bounds of the inventory?
			if(i >= GameManager.Inventory.Count)
			{
				// If we're out of the bounds of the inventory, is that outside the max possible boundary of the inventory?
				if(i >= GameManager.maxInventorySize)
				{
					// If so, we have no more slots available; disable any further slots
					myItems[i - offset].gameObject.SetActive(false);
				}
				else
				{
					// If not, we should still show further slots, though they shouldn't be interactable
					myItems[i - offset].gameObject.SetActive(true);
					myItems[i - offset].GetComponent<Button>().interactable = false;
					myItems[i - offset].sprite = emptySlot;
				}
					
			}

			// If we're in the bounds of the inventory, then is the current item edible?
			else if(GameManager.Inventory[i].GetComponent<Consumable>() == null)
			{
				// If not, we want to ignore it while not potentially putting an empty slot in the middle of the list
				offset++;
			}

			// If we've made it this far, the item is within the occupied bounds of the inventory and is edible
			else
			{
				myItems[i - offset].gameObject.SetActive(true);
				myItems[i - offset].sprite = GameManager.Inventory[i].InventoryIcon;

				// If there's room in storage, it should be set to interactible too
				if(GameManager.FoodStorage.Count < GameManager.foodStorageSize)
				{
					myItems[i - offset].GetComponent<Button>().interactable = true;
				}
				else
				{
					myItems[i - offset].GetComponent<Button>().interactable = false;
				}
			}
		}

		// Do the same for storage
		for(int i = 0; i < storedItems.Count; i++)
		{
			// With food storage, since they're guaranteed to be food, we just have to worry about whether or not they're in occupied bounds
			if(i < GameManager.FoodStorage.Count)
			{
				storedItems[i].sprite = GameManager.FoodStorage[i].InventoryIcon;

				// If there's room in the player's inventory, it should be set to interactible
				if(GameManager.Inventory.Count < GameManager.maxInventorySize)
				{
					storedItems[i].GetComponent<Button>().interactable = true;
				}
				else
				{
					storedItems[i].GetComponent<Button>().interactable = false;
				}
			}

			else
			{
				storedItems[i].sprite = emptySlot;
				storedItems[i].GetComponent<Button>().interactable = false;
			}
		}

		// Check if the quick move buttons should be enabled
		if(GameManager.Inventory.Count < GameManager.maxInventorySize)
		{
			btnWithdraw.interactable = true;
		}
		else
		{
			btnWithdraw.interactable = false;
		}

		if(GameManager.FoodStorage.Count < GameManager.foodStorageSize)
		{
			btnDeposit.interactable = true;
		}
		else
		{
			btnDeposit.interactable = false;
		}
	}

	void openMenuDelegate()
	{
		if(!MenuOpen)
		{
			MenuOpen = true;
			MenuManager.OtherMenuOpen = MenuOpen;
			UpdateItems();

			if (myPlayer.AcceptInput)
			{
				myPlayer.AcceptInput = false;
				myPlayer.GetComponent<CController>().HaltMomentum();
			}

			gameObject.SetActive(MenuOpen);
		}
	}

	public void DepositItem(int index)
	{
		// No sense in bothering if the index is either out of bounds or food storage has no room
		if(index >= myItems.Count || GameManager.FoodStorage.Count >= GameManager.foodStorageSize)
		{
			return;
		}

		// Decode the index, since due to non-edibles existing, it can be lower than its real index in the inventory list
		int realIndex = index;
		for(int i = 0; i < realIndex + 1; i++)
		{
			if(GameManager.Inventory[i].GetComponent<Consumable>() == null)
			{
				realIndex++;
			}
		}

		// Make the move
		GameManager.FoodStorage.Add(GameManager.Inventory[realIndex].GetComponent<Consumable>());
		GameManager.Inventory.RemoveAt(realIndex);

		UpdateItems();
	}

	public void WithdrawItem(int index)
	{
		// TODO: Withdrawing code here
		// No sense in bothering if the index is either out of bounds or the inventory has no room
		if(index >= storedItems.Count || GameManager.Inventory.Count >= GameManager.maxInventorySize)
		{
			return;
		}

		// No need to decode the index since food storage is guaranteed to be 1 to 1
		// Make the move
		GameManager.Inventory.Add(GameManager.FoodStorage[index]);
		GameManager.FoodStorage.RemoveAt(index);

		UpdateItems();
	}

	public void DepositAll()
	{
		// Loop through the whole inventory and deposit every consumable we can
		int i = 0;
		while(i < GameManager.Inventory.Count)
		{
			// If we're out of room, that's all she wrote
			if(GameManager.FoodStorage.Count >= GameManager.foodStorageSize)
			{
				break;
			}

			// Otherwise, check if it's actually food and add it if so
			else if(GameManager.Inventory[i].GetComponent<Consumable>() != null)
			{
				GameManager.FoodStorage.Add(GameManager.Inventory[i].GetComponent<Consumable>());
				GameManager.Inventory.RemoveAt(i);
			}

			// If it's not, we actually have to manually increment our iterator
			else
			{
				i++;
			}
		}

		UpdateItems();
	}

	public void WithdrawAll()
	{
		// TODO: Quick withdraw code here
		// Loop through all of food storage and withdraw every item we can
		while(GameManager.FoodStorage.Count > 0)
		{
			// If our inventory's full, that's it.
			if(GameManager.Inventory.Count >= GameManager.maxInventorySize)
			{
				break;
			}

			// Otherwise, blindly add it to the inventory since we know it's food
			GameManager.Inventory.Add(GameManager.FoodStorage[0]);
			GameManager.FoodStorage.RemoveAt(0);
		}

		UpdateItems();
	}
}
