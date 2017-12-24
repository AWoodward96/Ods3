using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A general script used for adding some input to the player when you walk up to an object
/// Show an indicator if the player is within range of this object
/// </summary>
[ExecuteInEditMode]
public class UsableIndicator : MonoBehaviour
{

    // ind_ stands for indicator
    public Sprite ind_Sprite;
    public GameObject ind_Object;
    public Vector3 ind_LocalLocation;
    public bool ind_Enabled;
    SpriteRenderer ind_Renderer;

    // Use this for initialization
    void Start()
    {
        if (ind_Object)
        {
            ind_Renderer = ind_Object.GetComponent<SpriteRenderer>();
        }  

    }

    // Update is called once per frame
    void Update()
    {
        RunUsable();
    }

    void RunUsable()
    {
        if (ind_Object == null)
        { 
            GameObject o = new GameObject("MyIndicator"); 
            o.transform.parent = this.gameObject.transform;
            o.transform.localScale = new Vector3(1, 2, 1);
            ind_Renderer = o.AddComponent<SpriteRenderer>();
            ind_Renderer.sortingOrder = 10;
            ind_Renderer.sprite = ind_Sprite;
            ind_Object = o;
        }
        else
        {
            ind_Object.transform.localPosition = ind_LocalLocation;
            ind_Renderer.sprite = ind_Sprite;

            ind_Object.SetActive(ind_Enabled);
        }
    }

}
