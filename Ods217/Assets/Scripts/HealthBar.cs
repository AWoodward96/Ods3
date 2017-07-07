using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour {

    // Important GameObjects
    GameObject BulletBarParent;

    // Image objects
    Image BulletBar;
    Image BulletUsedBar;
    Image BulletCover;
    Image HPBar;
    Image ReloadIcon;
    Image ArmorBar;

    Image[] BulletArray = new Image[5];
    Image[] BulletBGArray = new Image[5]; // For the backgrounds


    Image[] BulletBarCovers = new Image[5];

    // Statistics
    float maxHPVal;
    float currentHPVal;

    float timeSinceModified; // How long it's been since the menu has been shown. Hide it if it's been too long

    // Extentions
    IUnit myUnitObject;
    UnitStruct myUnit;
    ForceFieldScript myArmor;
    bool hasWeapon;
    bool hasArmor;

    // Dimensions
    float maxWidthHPBar;
    float maxWidthBulletBar;
    float dividedWidthBulletBar;

    // Use this for initialization
    void Start () {
        GetImportantReferences();
        maxWidthHPBar = HPBar.GetComponent<RectTransform>().sizeDelta.x;
        maxWidthBulletBar = BulletBar.GetComponent<RectTransform>().sizeDelta.x;

        // Now check to see if we have a unit
        myUnitObject = transform.GetComponentInParent<IUnit>();

        if (myUnitObject == null)
            myUnitObject = GetComponent<IUnit>();

        if (myUnitObject == null)
            myUnitObject = GetComponentInChildren<IUnit>();

        if (myUnitObject == null)
        {
            Debug.Log("HealthBar: " + this.gameObject + " cannot find a unit. This script will not work otherwise");
            return;
        }

        // Check if the unit has a shield
        myArmor = myUnitObject.gameObject.GetComponentInChildren<ForceFieldScript>();
        hasArmor = (myArmor != null);

        // Set up preliminary values
        myUnit = myUnitObject.MyUnit; // Because the unit struct is within the IUnit
        maxHPVal = myUnit.MaxHealth;
        currentHPVal = myUnit.CurrentHealth;

        // check if the unit actually has a weapon
        hasWeapon = (myUnitObject.MyWeapon != null);

        if(hasWeapon)
        {
            SetUpAmmoBar();
        }else
        {
            BulletBarParent.SetActive(false);
        }
         

    }
	
	// Fixed Upate > Update
	void FixedUpdate () {
		if(myUnit != null)
        {
            currentHPVal = myUnit.CurrentHealth;
            maxHPVal = myUnit.MaxHealth;

            // Get the information we need
            if(!hasArmor)
            {
                // If we have no armor then just display the health bar
                float percentage = currentHPVal / maxHPVal;
                float newRectSize = maxWidthHPBar * percentage;
                float newPosition = (maxWidthHPBar - newRectSize) / 2;

                // Now actually change the hp bar to reflect the current stats
                RectTransform HPBartransform = HPBar.GetComponent<RectTransform>();
                Vector2 pos = HPBartransform.anchoredPosition;
                HPBartransform.sizeDelta = new Vector2(newRectSize, HPBartransform.sizeDelta.y);
                HPBartransform.anchoredPosition = new Vector2(-newPosition, pos.y);
            }
            else
            {
                // the health bar should be shrunk to fit in the armor bar
                float armorHealth = myArmor.MaxHealth;
                float totalHealth = maxHPVal + myArmor.Health;
                // Get the percentages
                float hpPercent = currentHPVal / totalHealth;
                float armorPercent = myArmor.Health / totalHealth;
                // Calculate the new sizes
                float newHPSize = (maxWidthHPBar * hpPercent);
                float newArmorSize = (maxWidthHPBar * armorPercent);
                // Calculate the new positions
                float newHPPosition = (maxWidthHPBar - newHPSize) / 2;
                float newArmorPosition = (maxWidthHPBar - newArmorSize) / 2;

                // Now change the values
                RectTransform HPBartransform = HPBar.GetComponent<RectTransform>();
                Vector2 pos = HPBartransform.anchoredPosition;
                HPBartransform.sizeDelta = new Vector2(newHPSize, HPBartransform.sizeDelta.y);
                HPBartransform.anchoredPosition = new Vector2(-newHPPosition, pos.y);
                // now Armor
                RectTransform ArmorBartransform = ArmorBar.GetComponent<RectTransform>();
                Vector2 armorPos = ArmorBartransform.anchoredPosition;
                ArmorBartransform.sizeDelta = new Vector2(newArmorSize, ArmorBartransform.sizeDelta.y);
                ArmorBartransform.anchoredPosition = new Vector2(newArmorPosition, armorPos.y);

            }


            // Now handle the bullets
            if(hasWeapon)
            {
                Weapon myWeapon = myUnitObject.MyWeapon;

                // If we're reloading bring up the bullets
                if(myUnitObject.MyWeapon.isReloading)
                {

                }else
                {
                    float bulPercentage = myWeapon.CurrentClip / (float)myWeapon.MaxClip; 
                    float newBulRectSize = maxWidthBulletBar * bulPercentage;
                    float newBulPosition = (maxWidthBulletBar - newBulRectSize) / 2;

                    RectTransform BulBartransform = BulletBar.GetComponent<RectTransform>();
                    Vector2 bulPos = BulBartransform.anchoredPosition;
                    BulBartransform.sizeDelta = new Vector2(newBulRectSize, BulBartransform.sizeDelta.y);
                    BulBartransform.anchoredPosition = new Vector2(-newBulPosition, bulPos.y);
                }

            }
        }

        // If nothings been changed in 3 seconds then hide the health bar
        if (timeSinceModified < 3)
            timeSinceModified += Time.deltaTime;
        else
            GetComponent<Canvas>().enabled = false;
    }


    void SetUpAmmoBar()
    {
        // The total bars that we want should be equal to the maximum clip of the weapon
        int totalBars = myUnitObject.MyWeapon.MaxClip;
        totalBars--; // It should be total bars - 1 because we're calculating for divisions, not actual shots

        if(totalBars <= 0)
        {
            BulletCover.enabled = false;
            return;
        }

        BulletBarCovers = new Image[totalBars];
        BulletBarCovers[0] = BulletCover;

        // Get the size of these cover bars
        dividedWidthBulletBar = Mathf.Max(35 - (totalBars * 6), 1);


        // change the size of the bar
        RectTransform rectT = BulletBarCovers[0].GetComponent<RectTransform>();
        rectT.sizeDelta = new Vector2(dividedWidthBulletBar, rectT.sizeDelta.y);
        rectT.anchoredPosition = new Vector2((-maxWidthBulletBar / 2) + (((float)1 / (float)myUnitObject.MyWeapon.MaxClip) * maxWidthBulletBar), rectT.anchoredPosition.y);

        for(int i = 1; i < BulletBarCovers.Length+1; i++)
        {
            Image img = Instantiate(BulletBarCovers[0], rectT.parent).GetComponent<Image>();
            RectTransform rt = img.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((-maxWidthBulletBar / 2) + (((float)i / (float)myUnitObject.MyWeapon.MaxClip) * maxWidthBulletBar), rectT.anchoredPosition.y);
        }



        //BulletArray = new Image[totalBars];
        //BulletBGArray = new Image[totalBars];

        //// get the size of the bars
        //float temp = maxWidthBulletBar / totalBars;
        //dividedWidthBulletBar = temp - 5; // tune it down a bit so we have some space

        //// Dump the first bar into the array because we're gonne duplicate it
        //BulletArray[0] = BulletBar;
        //BulletBGArray[0] = BulletUsedBar;

        //// Change the size of the bar
        //RectTransform rectT = BulletArray[0].GetComponent<RectTransform>();
        //rectT.sizeDelta = new Vector2(dividedWidthBulletBar, rectT.sizeDelta.y);
        //rectT.anchoredPosition = new Vector2(-((maxWidthBulletBar - dividedWidthBulletBar) / 2), rectT.anchoredPosition.y);
        //// Now do the same for the background bars
        //RectTransform bgrectT = BulletBGArray[0].GetComponent<RectTransform>();
        //bgrectT.sizeDelta = new Vector2(dividedWidthBulletBar, bgrectT.sizeDelta.y);
        //bgrectT.anchoredPosition = new Vector2(-((maxWidthBulletBar - dividedWidthBulletBar) / 2), bgrectT.anchoredPosition.y);


        //for (int i = 1; i < BulletBGArray.Length; i++)
        //{
        //    // Do the same thing with the background bars
        //    Image bgimg = Instantiate(BulletBGArray[0], bgrectT.parent).GetComponent<Image>();
        //    RectTransform bgrt = bgimg.GetComponent<RectTransform>();
        //    // Move that into place
        //    bgrt.anchoredPosition = new Vector2(bgrectT.anchoredPosition.x + ((dividedWidthBulletBar + 5) * i), bgrectT.anchoredPosition.y);
        //}

        //// Ok now do the rest of them
        //for (int i = 1; i < BulletArray.Length; i++) // Start at one because we've already done the first one
        //{
        //    // Now we're going to copy the initial bar
        //    Image img = Instantiate(BulletArray[0], rectT.parent).GetComponent<Image>();
        //    RectTransform rt = img.GetComponent<RectTransform>();
        //    // Now just move it into place
        //    rt.anchoredPosition = new Vector2(rectT.anchoredPosition.x + ((dividedWidthBulletBar + 5) * i), rectT.anchoredPosition.y);

        //}


    }

    void GetImportantReferences()
    {
        // Gets all the image objects and other GameObjects that we'll use throughout this script
        Image[] allImages = GetComponentsInChildren<Image>();

        foreach(Image i in allImages)
        {
            if (i.name == "HealthFG")
            {
                HPBar = i;
                continue; 
            }

            if(i.name == "AmmoFG")
            {
                BulletBar = i;
                continue;
            }

            if (i.name == "AmmoUsed")
            {
                BulletUsedBar = i;
                continue;
            }

            if (i.name == "Reload")
            {
                ReloadIcon = i;
                continue;
            }

            if(i.name == "ArmorFG")
            {
                ArmorBar = i;
                continue;
            }

            if (i.name == "AmmoCover")
            {
                BulletCover = i;
                continue;
            }
        }

        BulletBarParent = transform.Find("AmmoBar").gameObject;
    }

    public void ShowMenu()
    {
        GetComponent<Canvas>().enabled = true;
        timeSinceModified = 0;
    }
}
