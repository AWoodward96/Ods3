//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

///// <summary>
///// A script for a canvas that is used for a units healthbar
///// </summary>
//public class HealthVisualizer : MonoBehaviour
//{

//    float HBMaxWidth;
//    float BBMaxWidth;
//    Image BulletBar;
//    Image BulletBarBG;
//    Image HPBar;
//    Image Reload;

//    Image[] BBars = new Image[5];
//    Image[] SBBars = new Image[0]; // Secondary Bars

//    public float maxVal;
//    public float currentVal;
//    float bulBarSize;

//    public GameObject myUnit;
//    IUnit unit;
//    CController unitCC;

//    public bool HideAmmo;
//    bool hasWeapon;

//    float timeSinceModified;

//    // Use this for initialization
//    void Start()
//    { 
//        GetVisualizerComponents();
//        HBMaxWidth = HPBar.GetComponent<RectTransform>().sizeDelta.x;
//        BBMaxWidth = BulletBar.GetComponent<RectTransform>().sizeDelta.x;

//        // Now that we have the max with and hopefully the unit
//        if(myUnit != null)
//        {
//            unit = myUnit.GetComponent<IUnit>();
//            maxVal = unit.MyUnit.MaxHealth;
//            currentVal = unit.MyUnit.CurrentHealth;

//            unitCC = myUnit.GetComponent<CController>();

//            hasWeapon = (unit.MyWeapon != null);
//            if (hasWeapon)
//                GetBBars();
//            else
//            {
//                BulletBarBG.enabled = false;
//                BulletBar.enabled = false;
//            }

//            if(HideAmmo)
//            {
//                BulletBar.enabled = false;
//                BulletBarBG.enabled = false;
//            }
//        }
//    }

//    // Update is called once per frame
//    void FixedUpdate()
//    {
//        if(unit != null)
//        {
//            currentVal = unit.MyUnit.CurrentHealth;
//            maxVal = unit.MyUnit.MaxHealth;
//        }

//        float percentage = currentVal / maxVal;
//        float newRectSize = HBMaxWidth * percentage;
//        float newPosition = (HBMaxWidth - newRectSize) / 2;

//        // Handle the hp bar
//        RectTransform HPBartransform = HPBar.GetComponent<RectTransform>();
//        Vector2 pos = HPBartransform.anchoredPosition; 
//        HPBartransform.sizeDelta = new Vector2(HBMaxWidth * percentage, HPBartransform.sizeDelta.y);
//        HPBartransform.anchoredPosition = new Vector2(-newPosition, pos.y);

//        // Now handle the ammo system
//        if(myUnit != null && hasWeapon)
//        {
//            // Primary
//            IUnit u = myUnit.GetComponent<IUnit>();
//            int availablebullets = u.MyWeapon.CurrentClip;
//            int current = 0;

//            for(int i = 0; i < availablebullets; i++)
//            {
//                BBars[i].enabled = true;
//                current++;
//            }
    
//            for(int i = current; i < BBars.Length; i++)
//            { 
//                BBars[i].enabled = false;
//            }

//            // Secondary
//            if(u.MyWeapon.SecondaryType != Weapon.SecondaryTypes.None)
//            {
//                SecondaryBBars();

//                availablebullets = u.MyWeapon.CurrentSecondaryClip;
//                current = 0;
//                for (int i = 0; i < availablebullets; i++)
//                {
//                    // enable every bar for each bullet in the clip
//                    SBBars[i].enabled = true;
//                    current++;
//                }

//                // disable every bar for each bullet not in the clip
//                for (int i = current; i < SBBars.Length; i++)
//                {
//                    // Need to disable some bars here
//                    SBBars[i].enabled = false;
//                }
//            }else
//            {
//                // disable all the bars
//                for (int i = 0; i < SBBars.Length; i++)
//                {
                     
//                    SBBars[i].enabled = false;
//                }
//            }




//            // If the unit is reloading then display the reload icon
//            if (Reload)
//            {
//                Reload.enabled = u.MyWeapon.isReloading;
//            }

//        }


//        // If nothings been changed in 3 seconds then hide the visualizer
//        if (timeSinceModified < 3)
//            timeSinceModified += Time.deltaTime;
//        else
//            GetComponent<Canvas>().enabled = false;
//    }

//    // Set up all the parameters by getting all of the actual gameobjects
//    void GetVisualizerComponents()
//    {
//        Image[] allImages = GetComponentsInChildren<Image>();

//        foreach (Image i in allImages)
//        {
//            if (i.name == "HealthFG")
//            {
//                HPBar = i; continue;
//            }

//            if (i.name == "AmmoFG")
//            {
//                BulletBar = i; continue;
//            }

//            if(i.name == "AmmoBG")
//            {
//                BulletBarBG = i; continue;
//            }

//            if(i.name == "Reload")
//            {
//                Reload = i; continue;
//            }
//        } 
//    }

//    void GetBBars()
//    {
//        // First initialize the IUnit
//        IUnit u = myUnit.GetComponent<IUnit>();
//        // Initialize the arrays   
//        BBars = new Image[u.MyWeapon.MaxClip];
//        // Get the size of the bars
//        float temp = BBMaxWidth / u.MyWeapon.MaxClip;
//        bulBarSize = temp - 5;

//        // Dump the first Bar into the array
//        BBars[0] = BulletBar;
    
//        // Change the size of the bar 
//        RectTransform rectT = BBars[0].GetComponent<RectTransform>();
//        rectT.sizeDelta = new Vector2(bulBarSize, rectT.sizeDelta.y);
//        rectT.anchoredPosition = new Vector2(-((BBMaxWidth - bulBarSize) / 2),rectT.anchoredPosition.y);

//        for(int i = 1; i < BBars.Length; i ++)
//        {
//            // First instantiate the variable
//            Image img = Instantiate(BBars[0], rectT.parent).GetComponent<Image>();
//            RectTransform rt = img.GetComponent<RectTransform>();
//            rt.anchoredPosition = new Vector2(rectT.anchoredPosition.x + ((bulBarSize + 5) * i), rectT.anchoredPosition.y);
//            BBars[i] = img;
//        }
//    }

//    void SecondaryBBars()
//    {
//        // First initialize the IUnit
//        IUnit u = myUnit.GetComponent<IUnit>();
//        // If the current max secondary clip is already equal to the SBBars length then we don't need to bother reinitializing
//        if (u.MyWeapon.CurrentMaxSecondaryClip == SBBars.Length)
//            return;
 

//        // If we're reinitializing then delete the whole array because we want a new one
//        foreach(Image i in SBBars)
//        {
//            Destroy(i);
//        }

//        // Make a new array
//        SBBars = new Image[u.MyWeapon.CurrentMaxSecondaryClip];

//        // Get the size of the bars
//        float temp = BBMaxWidth / u.MyWeapon.CurrentMaxSecondaryClip;
//        bulBarSize = temp - 5;

//        // Gotta initialize a starter variable
//        SBBars[0] = Instantiate(BBars[0], BBars[0].transform.position, Quaternion.identity, BBars[0].transform.parent);
//        // Change the size of the bar 
//        RectTransform rectT = SBBars[0].GetComponent<RectTransform>();
//        rectT.sizeDelta = new Vector2(bulBarSize, rectT.sizeDelta.y - 10);
//        rectT.anchoredPosition = new Vector2(-((BBMaxWidth - bulBarSize) / 2), rectT.anchoredPosition.y - 10);
//        SBBars[0].color = new Color((float)102 / 255, (float)150 / 255, (float)224 / 255);

//        for (int i = 1; i < SBBars.Length; i++)
//        {
//            // First instantiate the variable
//            Image img = Instantiate(SBBars[0], rectT.parent).GetComponent<Image>();
//            RectTransform rt = img.GetComponent<RectTransform>();
//            rt.anchoredPosition = new Vector2(rectT.anchoredPosition.x + ((bulBarSize + 5) * i), rectT.anchoredPosition.y);
//            img.color = new Color((float) 102/255, (float)150 /255, (float)224 /255);
//            SBBars[i] = img; // 
//        }

//    }

//    public void UpdateValues(float _Max, float _Current)
//    {
//        maxVal = _Max;
//        currentVal = _Current;
//        ShowMenu();
//    }

//    public void ShowMenu()
//    {
//        GetComponent<Canvas>().enabled = true;
//        timeSinceModified = 0;
//    }
//}
