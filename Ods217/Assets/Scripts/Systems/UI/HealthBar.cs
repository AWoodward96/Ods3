using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    Image HPBar;
    Image ArmorBar;

    float maxHPVal;
    float maxEnergyVal;
    float currentHPVal;

    float timeSinceModified = 3;

    IArmed myArmed;
    UnitStruct myUnit;
    EnergyManager myEnergyManager;
    ForceFieldScript myForceField;

    bool hasArmor;

    float maxWidthHPBar;
    float maxWidthEnergyBar;

    bool initialized = false;

	// Use this for initialization
	void Start () {
        if (!initialized)
            Init();

        
	}
	
    private void Init()
    {
        GetImportantReferences();
        // Establish the hp bar values
        maxWidthHPBar = HPBar.GetComponent<RectTransform>().sizeDelta.x;
        maxWidthEnergyBar = ArmorBar.GetComponent<RectTransform>().sizeDelta.x;

        if (!GetArmedObject())
            return;

        myForceField = myArmed.gameObject.GetComponentInChildren<ForceFieldScript>();
        hasArmor = (myForceField != null);

        // Set up other references
        myUnit = myArmed.MyUnit;
        maxHPVal = myUnit.MaxHealth;
        maxEnergyVal = myUnit.MaxEnergy;
        currentHPVal = myUnit.CurrentHealth;

        initialized = true;
       
    }


    private void FixedUpdate()
    {
        if(myUnit != null)
        {
            currentHPVal = myUnit.CurrentHealth;
            maxHPVal = myUnit.MaxHealth;

            // Handle the armor bar
            if (hasArmor)
                HandleArmor();
            else 
                ArmorBar.enabled = false;

            // Handle the hp bar
            HandleHP();
        }

        if (timeSinceModified < 3)
            timeSinceModified += Time.deltaTime;
        else
            GetComponent<Canvas>().enabled = false;
    }

    void HandleHP()
    {
        // display the health bar
        float percentage = currentHPVal / maxHPVal;
        float newRectSize = maxWidthHPBar * percentage;
        float newPosition = (maxWidthHPBar - newRectSize) / 2;

        // Now actually change the hp bar to reflect the current stats
        RectTransform HPBartransform = HPBar.GetComponent<RectTransform>();
        Vector2 pos = HPBartransform.anchoredPosition;
        HPBartransform.sizeDelta = new Vector2(newRectSize, HPBartransform.sizeDelta.y);
        HPBartransform.anchoredPosition = new Vector2(-newPosition, pos.y);
         
    }


    public void ShowMenu()
    {
        GetComponent<Canvas>().enabled = true;
        timeSinceModified = 0;
    }

    void HandleArmor()
    { 
        // Get established variables
        float currentArmor = myUnit.CurrentEnergy;
        float maxArmor = myUnit.MaxEnergy;
        // The percent of the bar that should be seen
        float energyPercent = currentArmor / maxArmor;
        // calculate the new armor size
        float newArmorSize = (maxWidthEnergyBar * energyPercent);
        // calculate the new armor bars position
        float newArmorPosition = (maxWidthEnergyBar - newArmorSize) / 2;

        // now update the bar values
        RectTransform armorBarTransform = ArmorBar.GetComponent<RectTransform>();
        Vector2 pos = armorBarTransform.anchoredPosition;
        armorBarTransform.sizeDelta = new Vector2(newArmorSize, armorBarTransform.sizeDelta.y);
        armorBarTransform.anchoredPosition = new Vector2(newArmorPosition, pos.y);

        ArmorBar.enabled = true;
    }

    void GetImportantReferences()
    {
        // Gets all the image objects and other GameObjects that we'll use throughout this script
        Image[] allImages = GetComponentsInChildren<Image>();

        foreach (Image i in allImages)
        {
            if (i.name == "HealthFG")
            {
                HPBar = i;
                continue;
            } 

            if(i.name == "ArmorFG")
            {
                ArmorBar = i;
                continue;
            }
        } 
    }


    bool GetArmedObject()
    {
        // Now check to see if we have a unit
        myArmed = transform.GetComponentInParent<IArmed>();

        if (myArmed == null)
            myArmed = GetComponent<IArmed>();

        if (myArmed == null)
            myArmed = GetComponentInChildren<IArmed>();

        if (myArmed == null)
        {
            Debug.Log("HealthBar: " + this.transform.parent.gameObject.name + " cannot find a armed object. This script will not work otherwise");
            return false;
        }
        else
            return true;
    }
 
}
