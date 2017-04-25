using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Because Unity Buttons are lame
/// You can't select them from a script! So if you want to highlight an initial decision from a script you're just fucked! : )
/// So, here's a custom button to work around this restriction
/// </summary>
[RequireComponent(typeof(Image))] 
public class CustomButtonUI : MonoBehaviour {

    public static CustomButtonUI Selected; // There can only be one.
    public Sprite SpriteIdle;
    public Sprite SpriteHighlighted;
    public Sprite SpriteSelected;

    //public delegate void OnCall();

    public CustomButtonUI NavLeft;
    public CustomButtonUI NavUp;
    public CustomButtonUI NavRight;
    public CustomButtonUI NavDown;

    Image myImage;
    Text myText;

	// Use this for initialization
	void Start () {
        myImage = GetComponent<Image>();
 

        myText = GetComponentInChildren<Text>();
 
	}
	
	// Update is called once per frame
	void Update () {
		if(DialogManager.InDialog) // Only handle this if we're in dialog
        {
            if(Selected == this)
            {
                myImage.sprite = SpriteHighlighted;
            }else
            {
                myImage.sprite = SpriteIdle;
            }
        }
	}

    public void HoverSelected(CustomButtonUI _ui)
    {
        Selected = _ui;
    }

    public void Clicked(bool _value)
    {
        if(DialogManager.InDialog)
        {
            DialogManager.instance.DecisionMade(_value);
        }
    }
}
