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

    List<Item> theItems;
    List<Image> theImages;

    Animator descriptionAnimator;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        // Get the items from the game manager
        theItems = GameManager.Inventory;


        // Get all of the images in the description
        Image[] imgArr = GetComponentsInChildren<Image>();
        theImages = new List<Image>();
        foreach (Image img in imgArr)
        {
            if (img == GetComponent<Image>())
                continue;

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


    }


    void Update()
    {
        // If the user right clicks then close the description field
        if (Input.GetMouseButtonDown(1))
        {
            descriptionAnimator.SetBool("Open", false);
        }
    }

    public void UpdateInventoryMenu()
    {
        // go through and update all the inventory slots
        theItems = GameManager.Inventory;
        for (int i = 0; i < theItems.Count; i++)
        {
            Item current = theItems[i];
            theImages[i].color = Color.white;
            theImages[i].sprite = current.InventoryIcon;
        }
    }


    public void HoveredItemSlot(int _slotnumber)
    {
        // If you hover over an item show the description field
        if (_slotnumber < theItems.Count)
        {
            descriptionAnimator.SetBool("Open", true);
            nameText.text = theItems[_slotnumber].Name; // Set the text of the title field to the items title text
            descriptionText.text = theItems[_slotnumber].Description; // Set the text of the description field to the items description text
        }

    }


}
