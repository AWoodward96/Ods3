using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {


    public static GameManager instance; 
    public static int ScrapCount;

    public static List<Item> Inventory;
 
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
        // Make sure that the communicator item is in the inventory
        Inventory.Add((Resources.Load("Prefabs/Items/Communicator") as GameObject).GetComponent<Item>());

	}
	
	// Update is called once per frame
	void Update () {
        scrapcount = ScrapCount;

    
       
    }
}
