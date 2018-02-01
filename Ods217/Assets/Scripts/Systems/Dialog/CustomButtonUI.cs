using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Because Unity Buttons are lame
/// Trying to select the first option in a script using the eventsystem is a chore especially with alternating sprites. If you select an option, then select another option it will leave the initial option in a selected sprite state.
/// So, here's a custom button to work around this restriction
/// </summary>
[RequireComponent(typeof(Image))]
public class CustomButtonUI : MonoBehaviour
{

    public static CustomButtonUI Selected; // There can only be one.
    public Sprite SpriteIdle;
    public Sprite SpriteHighlighted;
    public Sprite SpriteSelected;

    public CustomButtonUI NavLeft;
    public CustomButtonUI NavUp;
    public CustomButtonUI NavRight;
    public CustomButtonUI NavDown;

    Image myImage;

    // Use this for initialization
    void Start()
    {
        myImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
		if (CutsceneManager.InCutscene) // Only handle this if we're in a dialog prompt
        {
            if (Selected == this)
            {
                myImage.sprite = SpriteHighlighted; // If it's selected then change the sprite 
            }
            else
            {
                myImage.sprite = SpriteIdle; // If it isn't the selected button then change the sprite to idle
            }
        }
    }

    // If you hover over a button with the mouse then set that button as selected
    public void HoverSelected(CustomButtonUI _ui)
    {
        Selected = _ui;
    }

    // If it's clicked, let the dialog manager know that a decision was made and pass the boolian value
    public void Clicked(bool _value)
    {
		if (CutsceneManager.InCutscene)
        {
			// No idea how to replace this; to my knowledge, this is the only DialogManager call left
			DialogManager.instance.DecisionMade(_value);
        }
    }
}
