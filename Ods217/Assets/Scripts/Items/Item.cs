using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used only in prefabs
/// Items aren't actually spawned in the scene because they only exist in the inventory of the player
/// Any item seen in scene is actually just either a dialog script or something to act as a visual
/// </summary>
public class Item : MonoBehaviour
{

    public Sprite InventoryIcon;

    public int ID;
    public string Name;
    [TextArea(1, 3)]
    public string Description;


}
