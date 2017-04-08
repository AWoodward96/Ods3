using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    public Sprite InventoryIcon;

    public int ID;
    public string Name;
    [TextArea(1, 3)]
    public string Description;

 
}
