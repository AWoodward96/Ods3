using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager: MonoBehaviour {
    
    public static MenuManager instance;
    public static bool MenuOpen;
    public static bool ScrapOpen;
    public GameObject MenuHolderObject;
    public GameObject ScrapHolderObject;
    Canvas myCanvas;
    Canvas myUI;

    Animator MenuAnimator;
    Animator ScrapAnimator;
    Text scrapText;

    float deltaTScrap;
	// Use this for initialization
	void Start () {
        // Ensure there's only one
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);

        MenuOpen = false;

        DontDestroyOnLoad(this.gameObject);

        // Set up the animator 
        MenuAnimator = MenuHolderObject.GetComponent<Animator>();
        ScrapAnimator = ScrapHolderObject.GetComponent<Animator>();

        scrapText = ScrapHolderObject.GetComponentInChildren<Text>();

       Canvas[] myCanvassi = GetComponentsInChildren<Canvas>();
       foreach(Canvas c in myCanvassi)
       {
            if (c.name == "MenuCanvas")
                myCanvas = c;

            if (c.name == "UICanvas")
                myUI = c;
       }
        myCanvas.gameObject.SetActive(MenuOpen);
    }
	
	// Update is called once per frame
	void Update () {

        // Check the things that would prevent you from opening a menu
        if (DialogManager.InDialog)
            MenuOpen = false;

        if(!DialogManager.InDialog) // They'll probably be more checks in this if statement later on
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {

                MenuOpen = !MenuOpen;

                if(MenuOpen)
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


        ScrapAnimator.SetBool("Open", ScrapOpen);
        if(ScrapOpen)
        {
            deltaTScrap += Time.deltaTime;
            if(deltaTScrap > 2)
            {
                ScrapOpen = false;
            }

            scrapText.text = GameManager.ScrapCount.ToString();
        }
    }

    IEnumerator closeMenu()
    {
        yield return new WaitForSeconds(.17f);//  How long it takes for the closing animation to take place 
        myCanvas.gameObject.SetActive(false);
    }

    public void ShowScrap()
    {
        ScrapOpen = true;
        deltaTScrap = 0;
    }
}
