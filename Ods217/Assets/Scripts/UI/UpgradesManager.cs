using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class UpgradesManager : MonoBehaviour {

    public static UpgradesManager instance;
    public static bool MenuOpen;

    [Range(.1f, 10)]
    public float Range;
    public bool Interactable;
    GameObject Player;

    UsableIndicator ind_Interactable;

    public UpgradesBox[] WeaponReload;
    public UpgradesBox[] WeaponClip;
    public UpgradesBox[] WeaponFire;
    public UpgradesBox[] WeaponExtra;
     
    Canvas myCanvas;
    public Image HoverImage;
    public Image PurchaseImage;
    Text HoverText;
    Text bottomText;
    Text purchaseText;
    Image ScrapIcon;
    int currentSetNum;

    // For the textbox
    int currentCharacter;
    string currentString;
    AudioSource mySource;
    public Text TextArea;
    public AudioClip[] AudioClips;
    bool tryingToPurchase;
    UpgradesBox SelectedUpgrade;

    // Use this for initialization
    void Start () {
        instance = this;
        ind_Interactable = GetComponent<UsableIndicator>();
        myCanvas = GetComponentInChildren<Canvas>();
        myCanvas.enabled = false;
        mySource = GetComponent<AudioSource>();


        Text[] Textss = HoverImage.GetComponentsInChildren<Text>();
        foreach(Text t in Textss)
        {
            if(t.name == "TopText")
            {
                HoverText = t;
            }

            if(t.name == "BottomText")
            {
                bottomText = t;
            }
        }

        Image[] images = HoverImage.GetComponentsInChildren<Image>();
        foreach (Image i in images)
        {
            if (i.name == "Scrap")
            {
                ScrapIcon = i;
            }
        }


        Textss = PurchaseImage.GetComponentsInChildren<Text>();
        foreach(Text t in Textss)
        {
            if(t.name == "PurchaseText")
            {
                purchaseText = t;
            }
        }

        HoverImage.gameObject.SetActive(false);
        PurchaseImage.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        myCanvas.enabled = MenuOpen;


        if (Player == null) // Ensure that we have the player
            Player = GameObject.FindGameObjectWithTag("Player");

        // Check to make sure we can even do this calculation
        // At this point we'll know if there's a player in the scene at all
        if (Player != null)
        {
            Vector3 dist = transform.position - Player.transform.position;
            Interactable = (dist.magnitude <= Range);
            ind_Interactable.ind_Enabled = Interactable;
             
        }

        // If we get input that we want to interact, and we're able to interact with it
        if (Input.GetKeyDown(KeyCode.E) && Interactable)
        {
            MenuOpen = true;
            UpdateMenu();
            if(Player != null)
            {
                // don't allow the player to move if the menu is open
                CController playerCC = Player.GetComponent<CController>();
                playerCC.canMove = false;
            }
            StopAllCoroutines();
            StartCoroutine(PrintLine("Welcome to the upgrade station! :D"));
        }

        if(MenuOpen)
        {
            if(HoverImage.IsActive())
            {
                HoverImage.rectTransform.position = Input.mousePosition + new Vector3(65, 40, 0);
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                MenuOpen = false;
                HoverImage.gameObject.SetActive(false);
                if (Player != null)
                {
                    // re-allow the player to move
                    CController playerCC = Player.GetComponent<CController>();
                    playerCC.canMove = true;
                }
            }
           
        }
         
    }


    void UpdateMenu()
    {
        Upgrades currentUpgrades = GameManager.instance.UpgradeData;


        // Weapon set 2 is clip size
        for(int i = 0; i < currentUpgrades.ClipSize.Length; i++)
        { 
            if (currentUpgrades.ClipSize[i])
            {
                WeaponClip[i].State = UpgradesBox.UpgradeBoxState.Unlocked;
            }else
            {
                if (i > 0)
                {
                    if (WeaponClip[i - 1].State == UpgradesBox.UpgradeBoxState.Unlocked)
                    {
                        WeaponClip[i].State = UpgradesBox.UpgradeBoxState.CanPurchase;
                    }else
                    {
                        WeaponClip[i].State = UpgradesBox.UpgradeBoxState.Locked;
                    }
                }else
                {
                    WeaponClip[i].State = UpgradesBox.UpgradeBoxState.CanPurchase;
                } 
            } 
        }


        // Weapon set 1 is reload speed
        for (int i = 0; i < currentUpgrades.ReloadSpeed.Length; i++)
        {
            if (currentUpgrades.ReloadSpeed[i])
            {
                WeaponReload[i].State = UpgradesBox.UpgradeBoxState.Unlocked;
            }
            else
            {
                if (i > 0)
                {
                    if (WeaponReload[i - 1].State == UpgradesBox.UpgradeBoxState.Unlocked)
                    {
                        WeaponReload[i].State = UpgradesBox.UpgradeBoxState.CanPurchase;
                    }
                    else
                    {
                        WeaponReload[i].State = UpgradesBox.UpgradeBoxState.Locked;
                    }
                }
                else
                {
                    WeaponReload[i].State = UpgradesBox.UpgradeBoxState.CanPurchase;
                }
            }
        }

        // Weapon set 3 is fire rate
        for (int i = 0; i < currentUpgrades.FireRate.Length; i++)
        {
            if (currentUpgrades.ReloadSpeed[i])
            {
                WeaponFire[i].State = UpgradesBox.UpgradeBoxState.Unlocked;
            }
            else
            {
                if (i > 0)
                {
                    if (WeaponFire[i - 1].State == UpgradesBox.UpgradeBoxState.Unlocked)
                    {
                        WeaponFire[i].State = UpgradesBox.UpgradeBoxState.CanPurchase;
                    }
                    else
                    {
                        WeaponFire[i].State = UpgradesBox.UpgradeBoxState.Locked;
                    }
                }
                else
                {
                    WeaponFire[i].State = UpgradesBox.UpgradeBoxState.CanPurchase;
                }
            }
        }
    }

    public void Hovering(int _index)
    {
        if (tryingToPurchase)
            return;

        HoverImage.gameObject.SetActive(true);
        switch (_index)
        {
            case 0: 
                HoverText.text = "Reload Speed 1";
                SetUpHover(WeaponReload[0], 0);
                if(currentSetNum != 1)
                {
                    currentSetNum = 1;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("The 'Reload Speed' upgrade will let you reload faster so you can get back to killing faster! ;)")); 
                }
                break;
            case 1: 
                HoverText.text = "Reload Speed 2";
                SetUpHover(WeaponReload[1], 1);
                if (currentSetNum != 1)
                {
                    currentSetNum = 1;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("The 'Reload Speed' upgrade will let you reload faster so you can get back to killing faster! ;)"));
                }
                break;
            case 2:
                HoverText.text = "Reload Speed 3";
                SetUpHover(WeaponReload[2], 2);
                if (currentSetNum != 1)
                {
                    currentSetNum = 1;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("The 'Reload Speed' upgrade will let you reload faster so you can get back to killing faster! ;)"));
                }
                break;
            case 3:
                HoverText.text = "Clip Size 1";
                SetUpHover(WeaponClip[0], 0);
                if (currentSetNum != 2)
                {
                    currentSetNum = 2;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("Clip Size upgrades let you hold more bullets in the chamber, meaning more shots before you have to reload! :D"));
                }
                break;
            case 4:
                HoverText.text = "Clip Size 2";
                SetUpHover(WeaponClip[1], 1);
                if (currentSetNum != 2)
                {
                    currentSetNum = 2;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("Clip Size upgrades let you hold more bullets in the chamber, meaning more shots before you have to reload! :D"));
                }
                break;
            case 5:
                HoverText.text = "Clip Size 3";
                SetUpHover(WeaponClip[2], 2);
                if (currentSetNum != 2)
                {
                    currentSetNum = 2;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("Clip Size upgrades let you hold more bullets in the chamber, meaning more shots before you have to reload! :D"));
                }
                break;
            case 6:
                HoverText.text = "Fire Rate 1"; 
                SetUpHover(WeaponFire[0], 0);
                if (currentSetNum != 3)
                {
                    currentSetNum = 3;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("It makes you shoot faster! What's not to like? ^.^"));
                }
                break;
            case 7:
                HoverText.text = "Fire Rate 2";
                SetUpHover(WeaponReload[1], 1);
                if (currentSetNum != 3)
                {
                    currentSetNum = 3;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("It makes you shoot faster! What's not to like? ^.^"));
                }
                break;
            case 8:
                HoverText.text = "Fire Rate 3";
                SetUpHover(WeaponReload[2], 2);
                if (currentSetNum != 3)
                {
                    currentSetNum = 3;
                    StopAllCoroutines();
                    StartCoroutine(PrintLine("It makes you shoot faster! What's not to like? ^.^"));
                }
                break;
            case 9:
                HoverText.text = "Piercing Shot";
                break;
            case 10:
                HoverText.text = "Explosive Shot";
                break;
            case 11:
                HoverText.text = "Rubber Shot";
                break;
        }
    }

    void SetUpHover(UpgradesBox _Box, int index)
    {
        switch(_Box.State)
        {
            case UpgradesBox.UpgradeBoxState.Unlocked: 
                bottomText.text = "Unlocked";
                ScrapIcon.enabled = false;
                break;
            case UpgradesBox.UpgradeBoxState.Locked:
                bottomText.text = "Locked";
                ScrapIcon.enabled = false;
                break;
            case UpgradesBox.UpgradeBoxState.CanPurchase:
                int cost = 0;
                switch (_Box.UpgradeRank)
                {
                    case 0:
                        cost = 50;
                        break;
                    case 1:
                        cost = 125;
                        break;
                    case 2:
                        cost = 200;
                        break;
                }

                bottomText.text = cost.ToString();
                ScrapIcon.enabled = true;
                break;
        } 
    }

    UpgradesBox ReturnUpgradeBasedOnIndex(int _index)
    {
        if(_index < 3)
        {
            return WeaponReload[_index];
        }

        if(_index < 6)
        {
            return WeaponClip[_index - 3];
        }

        if(_index < 9)
        {
            return WeaponFire[_index - 6];
        }

        return WeaponExtra[_index - 9];
        
    }

    public void TryPurchase(int _index)
    {
        // Check if you have enough scrap
        SelectedUpgrade = ReturnUpgradeBasedOnIndex(_index);
        int cost = 0;
        switch (SelectedUpgrade.UpgradeRank)
        {
            case 0:
                cost = 50;
                break;
            case 1:
                cost = 125;
                break;
            case 2:
                cost = 200;
                break;
        }

        if(cost > GameManager.ScrapCount)
        {
            StopAllCoroutines();
            StartCoroutine(PrintLine("You don't have enough scrap for that... :(")); 
            return;
        }

        if (SelectedUpgrade.State == UpgradesBox.UpgradeBoxState.CanPurchase)
        {
            StopAllCoroutines();
            StartCoroutine(PrintLine("Would you like to purchase the " + SelectedUpgrade.UpgradeName + " upgrade?"));

            PurchaseImage.gameObject.SetActive(true);
            purchaseText.text = "Purchse the " + SelectedUpgrade.UpgradeName + " upgrade?";
            tryingToPurchase = true; 
        }else
        {
            tryingToPurchase = false;
            PurchaseImage.gameObject.SetActive(false);

            StopAllCoroutines();
            if (SelectedUpgrade.State == UpgradesBox.UpgradeBoxState.Unlocked)
            {
                StartCoroutine(PrintLine("You've already unlocked this upgrade you doof! XD"));
            }else
            {
                StartCoroutine(PrintLine("You need to unlock the previous upgrades before you can purchase this upgrade."));
            }
        } 
    }

    public void PurchaseAnswer(bool _value)
    {
        if(tryingToPurchase)
        {
            tryingToPurchase = false;
            PurchaseImage.gameObject.SetActive(false);
            if (_value)
            {
                if(SelectedUpgrade.UpgradeName.ToUpper().Contains("RELOAD"))
                {
                    GameManager.instance.UpgradeData.ReloadSpeed[SelectedUpgrade.UpgradeRank] = true; 
                }

                if (SelectedUpgrade.UpgradeName.ToUpper().Contains("CLIP"))
                {
                    GameManager.instance.UpgradeData.ClipSize[SelectedUpgrade.UpgradeRank] = true;
                }

                if (SelectedUpgrade.UpgradeName.ToUpper().Contains("RATE"))
                {
                    GameManager.instance.UpgradeData.FireRate[SelectedUpgrade.UpgradeRank] = true;
                }

                UpdateMenu();

                int cost = 0;
                switch (SelectedUpgrade.UpgradeRank)
                {
                    case 0:
                        cost = 50;
                        break;
                    case 1:
                        cost = 125;
                        break;
                    case 2:
                        cost = 200;
                        break;
                }

                GameManager.ScrapCount -= cost;
                MenuManager.instance.ShowScrap(); 
            }
        }
    }

    public void NotHovering()
    {
        HoverImage.gameObject.SetActive(false);
    }

    IEnumerator PrintLine(string _Line)
    {
        string fullLine = _Line;
        currentCharacter = 0;
        currentString = "";

        // Set the dialog clip to the talking clip
        mySource.clip = AudioClips[0];

        bool readingText = true; // Lets start an infinite loop
        while (readingText) // WEEEEEEEE
        {
            // If the string is less then the full line then keep writing
            if (currentString.Length < fullLine.Length)
            {
                // This will write the text out all scrolly n shit
                currentString = fullLine.Substring(0, currentCharacter);
                currentCharacter++;
                mySource.Play();
                TextArea.text = currentString;

                // If we press space, before the line is complete, just complete the line. Because yeah.
                if (Input.GetKeyDown(KeyCode.Space))
                    currentCharacter = fullLine.Length;
            }

            if (Input.GetKeyDown(KeyCode.Space) && currentString.Length == fullLine.Length)// If the line is complete, then lets gtfo of here
            {
                // break out
                readingText = false; 
            }

            // This makes it so we don't get an infinite loop error
            yield return null;
        }


        yield break;
    }


    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }


}
