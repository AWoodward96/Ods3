using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager: MonoBehaviour {
    
    public static MenuManager instance;
    public static bool MenuOpen;
    public GameObject MenuHolderObject;
    Canvas myCanvas;

    Animator MenuAnimator;
	// Use this for initialization
	void Start () {
        // Ensure there's only one
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);

        MenuOpen = false;


        // Set up the animator 
        MenuAnimator = MenuHolderObject.GetComponent<Animator>();

        myCanvas = GetComponentInChildren<Canvas>();
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
            
	}

    IEnumerator closeMenu()
    {
        yield return new WaitForSeconds(.17f);//  How long it takes for the closing animation to take place 
        myCanvas.gameObject.SetActive(false);
    }
}
