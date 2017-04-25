using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour {

    public static InventoryMenu instance; // Temporary

    public Image DescriptionSection;
    Text descriptionText;
    Text nameText;

    List<Item> theItems;
    List<Image> theImages;
    int selectedSlot;
    bool lockedSelection;

    Animator descriptionAnimator;

	// Use this for initialization
	void Awake () {
        // Get the items from the game manager
        theItems = GameManager.Inventory;


        // Get all of the images in the description
        Image[] imgArr = GetComponentsInChildren<Image>();
        theImages = new List<Image>(); 
        foreach(Image img in imgArr)
        {
            if (img == GetComponent<Image>())
                continue;

            theImages.Add(img);
        }

        // Sort them
        theImages = theImages.OrderBy(t => t.name).ToList();

        // Set up the description drop down 
        descriptionAnimator = DescriptionSection.GetComponent<Animator>();
        Text[] textArr = DescriptionSection.GetComponentsInChildren<Text>();
        foreach(Text t in textArr)
        {
            if (t.name == "NameText")
                nameText = t;

            if (t.name == "DescriptionText")
                descriptionText = t;
        }



        //Temp
        instance = this;
	}


    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            lockedSelection = false;
            descriptionAnimator.SetBool("Open", false);
        }
    }

    public void UpdateInventoryMenu()
    {
        theItems = GameManager.Inventory;
        for(int i = 0; i < theItems.Count; i++)
        {
            Item current = theItems[i];
            theImages[i].color = Color.white;
            theImages[i].sprite = current.InventoryIcon;
        }
    }


    public void HoveredItemSlot(int _slotnumber)
    { 
        if (_slotnumber < theItems.Count)
        {
            descriptionAnimator.SetBool("Open", true);
            nameText.text = theItems[_slotnumber].Name;
            descriptionText.text = theItems[_slotnumber].Description; 
        }

    }

 
}
