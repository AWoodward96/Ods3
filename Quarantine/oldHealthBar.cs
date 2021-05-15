using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class oldHealthBar : MonoBehaviour {

    // Important GameObjects
    GameObject BulletBarParent;

    // Image objects
    Image BulletBar;
    Image BulletCover;
    Image HPBar;
    Image ReloadIcon;
    Image ArmorBar;

    public Image[] BulletBarCovers = new Image[5];

    // Statistics
    float maxHPVal;
    float currentHPVal;

    float timeSinceModified; // How long it's been since the menu has been shown. Hide it if it's been too long

    // Extentions
    IArmed myArmedObject;
    UnitStruct myUnit;
    ForceFieldScript myArmor;
    bool hasWeapon;
    bool hasArmor;

    // Dimensions
    float maxWidthHPBar;
    float maxWidthBulletBar;
    float dividedWidthBulletBar;

    bool DEBUG = false;
    bool initialized = false;

    // Use this for initialization
    void Start () {
        if(!initialized)
            Init();

        // check if the unit actually has a weapon
        hasWeapon = (myArmedObject.myWeapon != null);
         
        if (hasWeapon)
        {
            BuildAmmoBar();
        } 

    }

    private void Init()
    {
        GetImportantReferences();
        maxWidthHPBar = HPBar.GetComponent<RectTransform>().sizeDelta.x;
        maxWidthBulletBar = BulletBar.GetComponent<RectTransform>().sizeDelta.x;


        if (!GetArmedObject())
            return;

        // Check if the unit has a shield
        myArmor = myArmedObject.gameObject.GetComponentInChildren<ForceFieldScript>();
        hasArmor = (myArmor != null);
 

        // Set up preliminary values
        myUnit = myArmedObject.MyUnit;
        maxHPVal = myUnit.MaxHealth;
        currentHPVal = myUnit.CurrentHealth;
        initialized = true;
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

                ArmorBar.enabled = false;
            }
            else
            {
                // the health bar should be shrunk to fit in the armor bar
				float armorHealth = myUnit.MaxEnergy;
				float totalHealth = maxHPVal + myUnit.CurrentEnergy;
                // Get the percentages
                float hpPercent = currentHPVal / totalHealth;
				float armorPercent = myUnit.CurrentEnergy / totalHealth;
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

                ArmorBar.enabled = true;
            }


            // Now handle the bullets
            hasWeapon = (myArmedObject.myWeapon != null);
            if (hasWeapon)
            {
                WeaponBase myWeapon = myArmedObject.myWeapon; 
				float bulPercentage = myUnit.CurrentEnergy / (float)myUnit.MaxEnergy; 
                float newBulRectSize = maxWidthBulletBar * bulPercentage;
                float newBulPosition = (maxWidthBulletBar - newBulRectSize) / 2;

                RectTransform BulBartransform = BulletBar.GetComponent<RectTransform>();
                Vector2 bulPos = BulBartransform.anchoredPosition;
                BulBartransform.sizeDelta = new Vector2(newBulRectSize, BulBartransform.sizeDelta.y);
                BulBartransform.anchoredPosition = new Vector2(-newBulPosition, bulPos.y);

                ReloadIcon.enabled = false; 
            }else
            {
                ReloadIcon.enabled = false;
            }
        }

        // If nothings been changed in 3 seconds then hide the health bar
        if (timeSinceModified < 3)
            timeSinceModified += Time.deltaTime;
        else
            GetComponent<Canvas>().enabled = false;
    }


    public void BuildAmmoBar()
    {
        if (!initialized)
        {
            if (DEBUG) Debug.Log("Recalled Init");
            Init();
        }

        // First check to see if we have a weapon 
        if(myArmedObject.myWeapon == null)
        {
            if (DEBUG) Debug.Log("No weapon detected");
            // If not, reset the bullet bar equal to how many current covers we have
            // This should completely clear out the bullet bar
            ResetBulletBar(BulletBarCovers.Length);
            if (BulletBarParent.activeSelf) // also set the actual bullet bar to false so we can't see it. We don't have a weapon, therefore we don't need an ammo bar
                BulletBarParent.SetActive(false);
            return;
        }else
        {
            // If we do have a weapon then we might want to see the ammo bar
            if (!BulletBarParent.activeSelf)
                BulletBarParent.SetActive(true);
        }

        // The total bars that we want should be equal to the maximum clip of the weapon  
		int totalBars = (int)myUnit.MaxEnergy;
        totalBars--; // It should be total bars - 1 because we're calculating for divisions, not actual shots
        if (DEBUG) Debug.Log("total bars should be: " + totalBars);

        // If there is no need for a divider or there are too many bars then get rid of every excess bar and then disable the origional cover 
        if (totalBars <= 0 || totalBars >= 16)
        {
            if (DEBUG) Debug.Log("Too many or too little bars detected, resetting: " + myArmedObject);
            ResetBulletBar(BulletBarCovers.Length);
            return;
        }

        if (totalBars == BulletBarCovers.Length) // If we're already good, get out
        {
            if (DEBUG) Debug.Log("Early exit: totalbars = bulletbarcovers");
            return;
        }
        else
            ResetBulletBar(totalBars);

        // Set up the bullet bars
        BulletBarCovers = new Image[totalBars];
        BulletBarCovers[0] = BulletCover;
        BulletCover.enabled = true;
  

        // Get the size of these cover bars
        dividedWidthBulletBar = Mathf.Max(35 - (totalBars * 6), 1);


        // change the size of the bar
        RectTransform rectT = BulletBarCovers[0].GetComponent<RectTransform>();
        rectT.sizeDelta = new Vector2(dividedWidthBulletBar, rectT.sizeDelta.y);
		rectT.anchoredPosition = new Vector2((-maxWidthBulletBar / 2) + (((float)totalBars / (float)myUnit.MaxEnergy) * maxWidthBulletBar), rectT.anchoredPosition.y);

        for(int i = 1; i < BulletBarCovers.Length; i++)
        {
            Image img = Instantiate(BulletBarCovers[0], rectT.parent).GetComponent<Image>();
            RectTransform rt = img.GetComponent<RectTransform>();
			rt.anchoredPosition = new Vector2((-maxWidthBulletBar / 2) + (((float)i / (float)myUnit.MaxEnergy) * maxWidthBulletBar), rectT.anchoredPosition.y); 
                BulletBarCovers[i] = img;
        }

    }

    void ResetBulletBar(int _totalBars)
    { 
        if (_totalBars != 0) // If we have bars to reset
        {
            if (DEBUG) Debug.Log("Deleting " + BulletBarCovers.Length + " bars");
            // Destroy all bullet bar covers except for the origional
            for (int i = 1; i < BulletBarCovers.Length; i++)
            {
                Destroy(BulletBarCovers[i].gameObject);
            }
        }
        
        // Then set that origional bullet bar cover back to the one slot left in the array
        BulletBarCovers = new Image[1];
        BulletBarCovers[0] = BulletCover;
        BulletCover.enabled = false; 
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

    bool GetArmedObject()
    {
        // Now check to see if we have a unit
        myArmedObject = transform.GetComponentInParent<IArmed>();

        if (myArmedObject == null)
            myArmedObject = GetComponent<IArmed>();

        if (myArmedObject == null)
            myArmedObject = GetComponentInChildren<IArmed>();

        if (myArmedObject == null)
        {
            Debug.Log("HealthBar: " + this.transform.parent.gameObject.name + " cannot find a armed object. This script will not work otherwise");
            return false;
        }
        else
            return true;
    }
     
}
