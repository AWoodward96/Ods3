using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A Menu Class
/// Used on the [ESC] item menu
/// Shows all the items you currently have in your inventory
/// </summary>
public class InventoryMenu : MonoBehaviour
{

    public static InventoryMenu instance;

    public Image DescriptionSection;
    Text descriptionText;
    Text nameText;
	Button eatBtn;
	Button tossBtn;

	public Text ScrapText;

	Sprite emptySlot;

    List<Item> theItems;
    List<Image> theImages;

    Animator descriptionAnimator;

	int selectedItem;

	bool descriptionOpen;

    // Use this for initialization
    void Awake()
    {
		// Ensure there's only one instance of this script
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);

        // Get the items from the game manager
        theItems = GameManager.Inventory;


        // Get all of the images in the description
        Image[] imgArr = GetComponentsInChildren<Image>();
        theImages = new List<Image>();
        foreach (Image img in imgArr)
        {
            if (img == GetComponent<Image>())
                continue;

			if(img.name == "ScrapHeader")
			{
				continue;
			}

            theImages.Add(img);
        }

        // Sort them
        theImages = theImages.OrderBy(t => t.name).ToList();

        // Set up the description drop down variables
        descriptionAnimator = DescriptionSection.GetComponent<Animator>();
        Text[] textArr = DescriptionSection.GetComponentsInChildren<Text>();
        foreach (Text t in textArr)
        {
            if (t.name == "NameText")
                nameText = t;

            if (t.name == "DescriptionText")
                descriptionText = t;
        }

		Button[] buttonArr = DescriptionSection.GetComponentsInChildren<Button>();
		for(int i = 0; i < buttonArr.Length; i++)
		{
			if(buttonArr[i].name == "EatBtn")
			{
				eatBtn = buttonArr[i];
			}

			else if(buttonArr[i].name == "TossBtn")
			{
				tossBtn = buttonArr[i];
			}
		}

		emptySlot = theImages[0].sprite;

		selectedItem = -1;

		descriptionOpen = false;
    }


    void Update()
    {
		ScrapText.text = "x" + GameManager.ScrapCount.ToString();

		descriptionAnimator.SetBool("Open", descriptionOpen);
    }

    public void UpdateInventoryMenu()
    {
		descriptionOpen = false;

        // go through and update all the inventory slots
        theItems = GameManager.Inventory;
		for (int i = 0; i < theImages.Count; i++)
        {
			if(i < theItems.Count)
			{
	            Item current = theItems[i];
	            theImages[i].color = Color.white;
	            theImages[i].sprite = current.InventoryIcon;
				theImages[i].GetComponent<Button>().interactable = true;
			}
			else
			{
				theImages[i].sprite = emptySlot;
				theImages[i].GetComponent<Button>().interactable = false;
			}
        }
    }


    public void ClickedItemSlot(int _slotnumber)
    {
		if(descriptionOpen)
		{
			if(selectedItem == _slotnumber)
			{
				CloseItemDesc();
				return;
			}

			StartCoroutine(changeDescriptions(_slotnumber));
		}

        // If you hover over an item show the description field
        else if (_slotnumber < theItems.Count)
        {
			selectedItem = _slotnumber;

			descriptionOpen = true;
            nameText.text = theItems[_slotnumber].Name; // Set the text of the title field to the items title text
            descriptionText.text = theItems[_slotnumber].Description; // Set the text of the description field to the items description text
			if(theItems[_slotnumber].GetComponent<Consumable>() != null)
			{
				eatBtn.gameObject.SetActive(true);
			}
			else
			{
				eatBtn.gameObject.SetActive(false);
			}

			if(theItems[_slotnumber].Tossable)
			{
				tossBtn.gameObject.SetActive(true);
			}
			else
			{
				tossBtn.gameObject.SetActive(false);
			}
        }
    }

	IEnumerator changeDescriptions(int newSlotNumber)
	{
		descriptionOpen = false;
		yield return new WaitForSeconds(0.15f);

		selectedItem = newSlotNumber;

		descriptionOpen = true;
		nameText.text = theItems[newSlotNumber].Name; // Set the text of the title field to the items title text
		descriptionText.text = theItems[newSlotNumber].Description; // Set the text of the description field to the items description text
		if(theItems[newSlotNumber].GetComponent<Consumable>() != null)
		{
			eatBtn.gameObject.SetActive(true);
		}
		else
		{
			eatBtn.gameObject.SetActive(false);
		}

		if(theItems[newSlotNumber].Tossable)
		{
			tossBtn.gameObject.SetActive(true);
		}
		else
		{
			tossBtn.gameObject.SetActive(false);
		}
	}

	public void CloseItemDesc()
	{
		descriptionOpen = false;
		selectedItem = -1;
	}

	public void EatItem()
	{
		if(selectedItem == -1)
		{
			Debug.Log("selectedItem is < 0. How did you even do that?");
			return;
		}
			
		StartCoroutine(crtEat());
	}

	IEnumerator crtEat()
	{
		MenuManager.MenuOpen = false;
		yield return new WaitForSeconds(.17f);//  How long it takes for the closing animation to take place

		// TODO: We should call Consume() on the item here!
		theItems[selectedItem].GetComponent<Consumable>().Consume();

		/*for(int i = 0; i < GameManager.Inventory.Count; i++)
		{
			if(GameManager.Inventory[i] == theItems[selectedItem])
			{
				GameManager.Inventory.RemoveAt(i);
				UpdateInventoryMenu();
				break;
			}
		}*/
	}

	public void TossClicked()
	{
		MenuManager.instance.ConfirmMenu(2);
	}

	public void TossItem()
	{
		if(selectedItem == -1)
		{
			Debug.Log("selectedItem is < 0. How did you even do that?");
			return;
		}

		for(int i = 0; i < GameManager.Inventory.Count; i++)
		{
			if(GameManager.Inventory[i] == theItems[selectedItem])
			{
				GameManager.Inventory.RemoveAt(i);
				UpdateInventoryMenu();
				break;
			}
		}
	}
}
