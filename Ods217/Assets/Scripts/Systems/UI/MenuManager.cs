using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    public static bool ScrapOpen;
    public static bool HealthkitOpen;
    public static bool ShowingDirectionalIndicator;
    public GameObject MenuHolderObject;
    public GameObject ScrapHolderObject;
    public GameObject HealthkitHolderObject;
    public GameObject DirectionalHolderObject;
    Canvas myCanvas;
 

    // Animating things
    Animator MenuAnimator;
    Animator ScrapAnimator;
    Animator HealthkitAnimator;

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
    void Start()
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

        scrapText = ScrapHolderObject.GetComponentInChildren<Text>();

        // Set up the canvas'
        Canvas[] myCanvassi = GetComponentsInChildren<Canvas>();
        foreach (Canvas c in myCanvassi)
        {
            if (c.name == "MenuCanvas")
                myCanvas = c;
             
        }
        myCanvas.gameObject.SetActive(MenuOpen);


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

        // Check the things that would prevent you from opening a menu
        if (DialogManager.InDialog)
            MenuOpen = false;

        // Handle opening and closing the inventory menu
        if (!DialogManager.InDialog) // They'll probably be more checks in this if statement later on
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {

                MenuOpen = !MenuOpen;

                if (MenuOpen)
                {
                    myCanvas.gameObject.SetActive(MenuOpen);
                    MenuAnimator.SetBool("Open", true); // hard coding true because unity animations confuse me
                    InventoryMenu.instance.UpdateInventoryMenu();
                }
                else
                {
                    MenuAnimator.SetBool("Open", false);
                    StopAllCoroutines();
                    StartCoroutine(closeMenu());
                }

            }
        }


        // Handle opening and closing the scrap menu
        ScrapAnimator.SetBool("Open", ScrapOpen);
        if (ScrapOpen)
        {
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
            // Turn off every other directional image besides the one that we're working on
            for(int i = 0; i < 4; i ++)
            {
                if ((int)ShowingDirection == i)
                    continue;

                DirectionalImageArray[i].enabled = false;
            }
        }

 
        
    }

    // This coroutine is to disable the menu object once it's actually closed so you can't effect it unintentionally when it's not being shown
    IEnumerator closeMenu()
    {
        yield return new WaitForSeconds(.17f);//  How long it takes for the closing animation to take place 
        myCanvas.gameObject.SetActive(false);
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
}
