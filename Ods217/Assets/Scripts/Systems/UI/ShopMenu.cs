using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenu : MonoBehaviour
{
	public bool MenuOpen;
	public bool DescriptionOpen;

	public GameObject myShop;
	UsableIndicator ind_Interactable;

	PlayerScript myPlayer;

	public GameObject DescriptionSection;
	public Text itemName;
	public Image itemImage;
	public Text itemDesc;
	public Text itemCost;

	public GameObject ItemsSection;

	public Text dialogText;

	[TextArea(1, 100)]
	public string[] greetings;
	int currentGreeting;

	[TextArea(1, 100)]
	public string responseNoCash = "Sorry, I'm not running a charity here!";

	[TextArea(1, 100)]
	public string responseBuy = "Thanks for your purchase!";

	[TextArea(1, 100)]
	public string responseDuplicate = "(I already own one. I don't need an extra laying around...)";

	[TextArea(1, 100)]
	public string responseNoRoom = "(I have absolutely no room for this item.)";

	Text[] stockButtons;

	Animator descriptionAnimator;

	public List<GameObject> stockObjects;
	List<IBuyable> stock;
	int currentIndex;

	// Use this for initialization
	void Start ()
	{
		// Set up other references
		ind_Interactable = myShop.GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
		ind_Interactable.Output = openMenuDelegate; 
		myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		MenuOpen = false;
		DescriptionOpen = false;

		descriptionAnimator = DescriptionSection.GetComponent<Animator>();

		stockButtons = ItemsSection.GetComponentsInChildren<Text>();

		currentGreeting = 0;

		stock = new List<IBuyable>();
		for(int i = 0; i < stockObjects.Count; i++)
		{
			stock.Add(stockObjects[i].GetComponent<IBuyable>());
		}

		for(int i = 0; i < stockButtons.Length; i++)
		{
			if(i < stock.Count)
			{
				stockButtons[i].text = stock[i].ShopName;
			}
			else
			{
				stockButtons[i].transform.parent.gameObject.SetActive(false);
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Escape) && MenuOpen)
		{
			MenuOpen = false;
			myPlayer.AcceptInput = true;
		}

		if (!MenuOpen)
		{
			DescriptionOpen = false;
		}

		descriptionAnimator.SetBool("Open", DescriptionOpen);
			
		gameObject.SetActive(MenuOpen);
	}

	void openMenuDelegate()
	{
		if(!MenuOpen)
		{
			MenuOpen = true;
			MenuManager.OtherMenuOpen = MenuOpen;
			if (myPlayer.AcceptInput)
			{
				myPlayer.AcceptInput = false;
				myPlayer.GetComponent<CController>().HaltMomentum();
			}

			dialogText.text = greetings[currentGreeting];
			currentGreeting++;

			if(currentGreeting >= greetings.Length)
			{
				currentGreeting = greetings.Length - 1;
			}

			gameObject.SetActive(MenuOpen);
		}
	}

	public void ClickItem(int index)
	{
		if(DescriptionOpen && index != currentIndex)
		{
			DescriptionOpen = false;

			StartCoroutine(changeDescriptions(index));
		}

		else
		{
			DescriptionOpen = true;

			itemImage.sprite = stock[index].ShopIcon;

			itemName.text = stock[index].ShopName;
			itemDesc.text = stock[index].ShopDescription;

			itemCost.text = "Cost: $";
			itemCost.text += stock[index].Cost.ToString() + "\n";
			itemCost.text += "Scrap: $" + GameManager.ScrapCount + "\n\n";
			itemCost.text += "Total: $" + (GameManager.ScrapCount - stock[index].Cost);
		}

		currentIndex = index;
	}

	public void CancelItem()
	{
		DescriptionOpen = false;
		currentIndex = -1;
	}

	public void BuyItem()
	{
		int result = stock[currentIndex].Buy();

		// Success
		if(result == 0)
		{
			dialogText.text = responseBuy;
		}

		// Insufficient Funds
		else if(result == 1)
		{
			dialogText.text = responseNoCash;
		}

		// Player would be buying a second item when it would be pointless to
		else if(result == 2)
		{
			dialogText.text = responseDuplicate;
		}

		// No inventory space
		else if(result == 3)
		{
			dialogText.text = responseNoRoom;
		}

		CancelItem();
	}

	public void ExitMenu()
	{
		MenuManager.OtherMenuOpen = false;
		MenuOpen = false;
		myPlayer.AcceptInput = true;
	}

	IEnumerator changeDescriptions(int index)
	{
		yield return new WaitForSeconds(0.15f);

		itemImage.sprite = stock[index].ShopIcon;

		itemName.text = stock[index].ShopName;
		itemDesc.text = stock[index].ShopDescription;

		itemCost.text = "Cost: $";
		itemCost.text += stock[index].Cost.ToString() + "\n";
		itemCost.text += "Scrap: $" + GameManager.ScrapCount + "\n\n";
		itemCost.text += "Total: $" + (GameManager.ScrapCount - stock[index].Cost);

		DescriptionOpen = true;
	}
}
