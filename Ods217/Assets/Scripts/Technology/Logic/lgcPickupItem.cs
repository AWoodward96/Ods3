using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lgcPickupItem : MonoBehaviour {


    public bool Respawns;
    public GameObject ItemRef;

    UsableIndicator ind_Usable;

	// Use this for initialization
	void Start () {
        ind_Usable = GetComponentInChildren<UsableIndicator>();
        ind_Usable.Output = Pickup;

        if (GameManager.Inventory.Contains(ItemRef.GetComponent<Item>()))
            gameObject.SetActive(false);
	}
	
    void Pickup()
    {
        Item i = ItemRef.GetComponent<Item>();

        if (!GameManager.Inventory.Contains(i)) 
            GameManager.Inventory.Add(i);

        gameObject.SetActive(false);
    }
}
