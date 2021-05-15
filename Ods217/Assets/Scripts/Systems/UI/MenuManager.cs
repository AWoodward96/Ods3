using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A Menu Class
/// Handles hiding and showing all of the menus in the game
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Extentions")]
    public static MenuManager instance;
    public static bool MenuOpen;
    public bool Foo;
    public static bool OtherMenuOpen;
    public static bool ScrapOpen;
    public static bool HealthkitOpen;
    public static bool ShowingDirectionalIndicator;
    public GameObject MenuHolderObject;
    public GameObject ScrapHolderObject;
    public GameObject HealthkitHolderObject;
    public GameObject DirectionalHolderObject;
	public GameObject ObjectiveNotificationObject;

	// Box for comfirming a potentially risky action
	public GameObject ConfirmBox;
	int confirmIndex;

    Canvas myCanvas;
	Canvas uiCanvas;
 

    // Animating things
    Animator MenuAnimator;
    Animator ScrapAnimator;
    Animator HealthkitAnimator;
	Animator ObjectiveAnimator;

    // Health kit info    
    [Space(30)]
    [Header("Health Kit Info")]
    public Sprite[] HealthKitImages;
    List<Image> HealthKits;
    float deltaTScrap;
    float deltatTHealthkit;

    // Scrap info
    Text scrapText;

    // Directional info
    public enum Direction { Right, Up, Left, Down };
    [Space(30)]
    [Header("Directional Info")]
    public Direction ShowingDirection; // Which direction arrow is being shown
    public Image[] DirectionalImageArray; // The array that holds the actual images
    float directionalShowCount; // how many times this arrow has blinked 
 

    // Use this for initialization
    void Awake()
    {
        // Ensure there's only one instance of this script
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);

        MenuOpen = false;

        DontDestroyOnLoad(this.gameObject);

        // Set up the animator 
        MenuAnimator = MenuHolderObject.GetComponent<Animator>();
        ScrapAnimator = ScrapHolderObject.GetComponent<Animator>();
        HealthkitAnimator = HealthkitHolderObject.GetComponent<Animator>();
		ObjectiveAnimator = ObjectiveNotificationObject.GetComponent<Animator>();

        scrapText = ScrapHolderObject.GetComponentInChildren<Text>();

		ConfirmBox.SetActive(false);
		confirmIndex = -1;

        // Set up the canvas'
        Canvas[] myCanvassi = GetComponentsInChildren<Canvas>();
        foreach (Canvas c in myCanvassi)
        {
            if (c.name == "MenuCanvas")
                myCanvas = c;

			else if(c.name == "UICanvas")
				uiCanvas = c;
        }
		MenuHolderObject.SetActive(MenuOpen);


        // Set up the health kit images
        Image[] allHealthKits = HealthkitHolderObject.GetComponentsInChildren<Image>();
        HealthKits = new List<Image>();
        foreach(Image i in allHealthKits)
        {
            if(i.name.Contains("Kit"))
            {
                HealthKits.Add(i);
            }
        }

        HealthKits = HealthKits.OrderBy(obj => obj.name).ToList();

        // Set up the directional images
        DirectionalImageArray = DirectionalHolderObject.GetComponentsInChildren<Image>();
        foreach (Image i in DirectionalImageArray)
            i.enabled = false;
        //DirectionalImageArray = allDirectionalImages.OrderBy(obj => obj.name).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        bool invalidScene = (SceneManager.GetActiveScene().name == "Title");
        // Check the things that would prevent you from opening a menu
        if ((CutsceneManager.InCutscene || OtherMenuOpen || invalidScene) && MenuOpen)
        { 
            MenuOpen = false;
            CloseMenu();
        }


        // Handle opening and closing the inventory menu
		if (!CutsceneManager.InCutscene && !OtherMenuOpen && !invalidScene) // They'll probably be more checks in this if statement later on
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
				if(ConfirmBox.activeInHierarchy)
				{
					ConfirmBox.SetActive(false);
					confirmIndex = -1;
				}

				else
				{
	                MenuOpen = !MenuOpen; 
				}
            }
        }

		bool animState = MenuAnimator.isActiveAndEnabled;
		if(animState)
		{
			animState = MenuAnimator.GetBool("Open");
		}

		if(MenuOpen != animState)
		{
			if (MenuOpen)
			{
				MenuHolderObject.SetActive(MenuOpen);
				myCanvas.worldCamera = Camera.main;
				MenuAnimator.SetBool("Open", true); // hard coding true because unity animations confuse me
				InventoryMenu.instance.UpdateInventoryMenu();
				ObjectivesMenu.instance.UpdateObjectiveMenu();
				WeaponsMenu.instance.UpdateWeaponsMenu();

				ObjectiveAnimator.SetBool("Open", false);
				ScrapOpen = false;
			}
			else
			{
				CloseMenu();
			}
		}


        // Handle opening and closing the scrap menu
        ScrapAnimator.SetBool("Open", ScrapOpen);
        if (ScrapOpen)
        {
			uiCanvas.worldCamera = Camera.main;

            // Close the scrap menu after 2 seconds if you haven't picked up scrap since
            deltaTScrap += Time.deltaTime;
            if (deltaTScrap > 2)
            {
                ScrapOpen = false;
            }

            scrapText.text = GameManager.ScrapCount.ToString();
        }

        HealthkitAnimator.SetBool("Open", HealthkitOpen);
        if(HealthkitOpen)
        {
			uiCanvas.worldCamera = Camera.main;

            for(int i = 0; i < HealthKits.Count; i++)
            {
                HealthKits[i].sprite = (GameManager.HealthKits > i) ? HealthKitImages[0] : HealthKitImages[1];
            }

            deltatTHealthkit += Time.deltaTime;
            if(deltatTHealthkit > 4)
            {
                HealthkitOpen = false;
            }
        }

        if(ShowingDirectionalIndicator)
        {
			uiCanvas.worldCamera = Camera.main;

            // Turn off every other directional image besides the one that we're working on
            for(int i = 0; i < 4; i ++)
            {
                if ((int)ShowingDirection == i)
                    continue;

                DirectionalImageArray[i].enabled = false;
            }
        }

 
        
    }

	void LateUpdate()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			OtherMenuOpen = false;
		}
	}

    // This coroutine is to disable the menu object once it's actually closed so you can't effect it unintentionally when it's not being shown
    IEnumerator closeMenu()
    {
		ConfirmBox.SetActive(false);

        yield return new WaitForSeconds(.17f);//  How long it takes for the closing animation to take place 
		MenuHolderObject.SetActive(false);
		MenuOpen = false;

		confirmIndex = -1;
    }

    // Called when you pick up scrap
    public void ShowScrap()
    {
        ScrapOpen = true;
        deltaTScrap = 0;
    }

    // [Deprecated]
    public void ShowHealthkit()
    {
        HealthkitOpen = true;
        deltatTHealthkit = 0;
    }

    void CloseMenu()
    {
        MenuAnimator.SetBool("Open", false);
        StopAllCoroutines();
        StartCoroutine(closeMenu());
    }

    public void ShowDirectional(Direction _dir)
    {
        ShowingDirectionalIndicator = true;
        ShowingDirection = _dir;
        directionalShowCount = 0;
        DirectionalImageArray[(int)ShowingDirection].enabled = true; // Turn on the direction
        StopCoroutine(blinkingDirectionCRT());
        StartCoroutine(blinkingDirectionCRT());
    }

    IEnumerator blinkingDirectionCRT()
    {
        while(directionalShowCount < 7)
        {
            yield return new WaitForSeconds(1);
            directionalShowCount++;
            DirectionalImageArray[(int)ShowingDirection].enabled = !DirectionalImageArray[(int)ShowingDirection].enabled;
        }
        DirectionalImageArray[(int)ShowingDirection].enabled = false; // Ensure that the thing is turned off
        ShowingDirectionalIndicator = false;
    }

    public void StopDirectional()
    {
        StopCoroutine(blinkingDirectionCRT());
        
        ShowingDirectionalIndicator = false;
        foreach (Image i in DirectionalImageArray)
            i.enabled = false;
    }

	public void ConfirmMenu(int index)
	{
		confirmIndex = index;
		ConfirmBox.SetActive(true);

		Text message = ConfirmBox.GetComponentInChildren<Text>();
		message.text = "Are you sure you want to ";

		if(index == 0)
		{
			message.text += "load from your last save? All unsaved progress will be lost.";
		}
		else if(index == 1)
		{
			message.text += "quit? All unsaved progress will be lost.";
		}
		else if(index == 2)
		{
			message.text += "toss that item?";
			return;
		}
	}

	public void Confirmed(bool result)
	{
		if(result)
		{
			if(confirmIndex == 0)
			{
				StartCoroutine(LoadGame());
			}

			else if(confirmIndex == 1)
			{
				StartCoroutine(QuitGame());
			}

			else if(confirmIndex == 2)
			{
				InventoryMenu.instance.TossItem();
				ConfirmBox.SetActive(false);
			}
		}
		else
		{
			ConfirmBox.SetActive(false);
		}

		confirmIndex = -1;
	}

	IEnumerator LoadGame()
	{
		Camera.main.GetComponent<CamScript>().FadeOut(1.0f);

		yield return new WaitForSeconds(1);

		StartCoroutine(closeMenu());

		yield return new WaitForSeconds(1);

		GameManager.instance.LoadSaveFile(true);
	}

	IEnumerator QuitGame()
	{
		Camera.main.GetComponent<CamScript>().FadeOut(1.0f);

		yield return new WaitForSeconds(1);

		StartCoroutine(closeMenu());

		yield return new WaitForSeconds(1);

		SceneManager.LoadScene("Title");
	}

	public void FlashObjectiveNotification()
	{
		ObjectiveAnimator.SetBool("Open", true);
		StartCoroutine(RetractObjectiveNotification());
	}

	IEnumerator RetractObjectiveNotification()
	{
		yield return new WaitForSeconds(2.0f);
		ObjectiveAnimator.SetBool("Open", false);
	}
}
