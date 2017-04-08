using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {


    public static GameManager instance; 
    public static int ScrapCount;

    public static List<Item> Inventory;

    public Item AddMe;
    public Item AddMe2;
    public int scrapcount;

    // Use this for initialization
	void Start () {
        // There can only be one
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        Inventory = new List<Item>();
        Inventory.Add(AddMe);
        Inventory.Add(AddMe2);

	}
	
	// Update is called once per frame
	void Update () {
        scrapcount = ScrapCount;

    
       
    }
}
