﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

 

[ExecuteInEditMode]
public class UsableIndicator : MonoBehaviour
{

    public static IndicatorInfo Grab;
    public static IndicatorInfo Hover;  

    public enum usableIndcPreset { None, Interact, PickUp, Loot, Open, Close, Locked, Talk, Disarm, Save };
    public enum usableIndcStyle { Toggle, Hold };
    [Header("Object Data")]
    public usableIndcPreset Preset;
    usableIndcPreset presetPrev;
    public usableIndcStyle Style;
    usableIndcStyle stylePrev;
    public float Range;
    public bool ActiveInd;
    public bool Disabled;
	public bool Laserable = false;

    public IndicatorInfo curGrab;

    [Header("Override Data")]
    public Transform OverrideTransform;
    public Vector3 OverrideAdditionalVec;
    public string OverrideText;
    public float OverrideHoverRange;
    public bool debugHide;

    const float alphaSpeed = .03f;
    float alphaValue;
    Image[] Images;
    Image[] ImageCircles;
    Image eBar;
    Text myText;
    [HideInInspector]
    public Text talkText;
    public Animator eAnim;
	public bool held;
	public bool lasered;

    GameObject player;

    IndicatorInfo resetInd;

    public delegate void outputType();
    public outputType Output;

    // Use this for initialization
    void Awake()
    {
        Images = GetComponentsInChildren<Image>();
        List<Image> img = new List<Image>();
        for(int i = 0; i < Images.Length; i++)
        {
            if(Images[i].name.Contains("Circle"))
            {
                img.Add(Images[i]);
            }

            if(Images[i].name == "eBar")
            {
                eBar = Images[i];
            }
        }

        ImageCircles = img.ToArray();

        Text[] texts = GetComponentsInChildren<Text>();
        for(int i = 0; i < texts.Length; i ++)
        {
            if (texts[i].name == "Text")
                myText = texts[i];

            if (texts[i].name == "TalkCount")
                talkText = texts[i];
        }

        if (Preset != usableIndcPreset.Talk && talkText != null)
            talkText.gameObject.SetActive(false);

		eAnim = GetComponentInChildren<Animator>();

        resetInd.ind = null;
        resetInd.distanceToPlayer = 100000;
        resetInd.distanceToCursor = 100000;

        Grab = resetInd;
        Hover = resetInd;
    } 
    // Update is called once per frame
	void Update()
	{
		curGrab = Grab;
		RunUsable();

		ActiveInd = (UsableIndicator.Grab.ind == this);

		if(ActiveInd && !Disabled && Output != null)
		{
			if(Style == usableIndcStyle.Toggle && Input.GetKeyDown(KeyCode.E))
			{
				Output();
			}
		}
	}

	// Some input is handled in FixedUpdate to avoid the eaten inputs bug
    void FixedUpdate()
    {
        if(ActiveInd && !Disabled && Output != null)
		{
			if(Style == usableIndcStyle.Hold)
			{
				if(Input.GetKey(KeyCode.E))
				{
					Output();
				}

				held = Input.GetKey(KeyCode.E);
			}
		}
		else
		{
			held = false;
		}

		if(eAnim != null && Application.isPlaying)
		{
			eAnim.SetBool("Held", held || lasered);
		}
    }

    void RunUsable()
    {
        handlePresets(); 

        if (!Application.isPlaying)
        {
            if (debugHide)
            {
                alphaValue -= alphaSpeed;
                alphaValue = Mathf.Max(0, alphaValue);
                UpdateAlphaImages();

            } 
            return;
        }

        if (Disabled) // If we're disabled we want to hide this so hide it
        {
            alphaValue -= alphaSpeed;
            alphaValue = Mathf.Max(0, alphaValue);
            UpdateAlphaImages();

            if (Grab.ind == this)
                Grab = resetInd;

            return; // Go no further because it doesn't matter, we're disabled
        }

        handleSelection();

         

        if (Grab.ind == this) // If we're hovering over this object then 
        {
            alphaValue += alphaSpeed;
            alphaValue = Mathf.Min(1.75f, alphaValue);
            UpdateAlphaImages(); 
        }
        else if (Hover.ind == this)
        {
            alphaValue += alphaSpeed / 3;
            alphaValue = Mathf.Min(.1f, alphaValue);
            UpdateAlphaImages();
        }
        else
        {
            alphaValue -= alphaSpeed;
            alphaValue = Mathf.Max(0, alphaValue);
            UpdateAlphaImages();
        }
 
    }

    void handleSelection()
    {
        // Get the distance to the player and the distance to the cursor
        Vector3 realdistanceToPlayer = Player.transform.position - (((OverrideTransform != null) ? OverrideTransform.position : transform.position)) + OverrideAdditionalVec;
        Vector3 distanceToPlayer = GlobalConstants.ZeroYComponent(realdistanceToPlayer);
        Vector3 distanceToCursor = GlobalConstants.ZeroYComponent(CamScript.CursorLocation - (((OverrideTransform != null) ? OverrideTransform.position : transform.position)) + OverrideAdditionalVec);
        float distToPlayerMag = distanceToPlayer.magnitude;
        float distToCursorMag = distanceToCursor.magnitude; // Because calculating magnitude takes a not-0 amount of time

        /// --------------
        /// Grab targets
        /// --------------
        // If we're not the current grab target
        if (Grab.ind != this)
        {
            if (distToPlayerMag < (Range) && realdistanceToPlayer.y < 6) // If we're close enough to interact with it
            {
                // Check if we're actually closer then the current grab
                if (distToPlayerMag <= Grab.distanceToPlayer)
                {
                    IndicatorInfo i = new IndicatorInfo();
                    i.distanceToPlayer = distToPlayerMag;
                    i.distanceToCursor = distToCursorMag;
                    i.ind = this;
                    Grab = i;
                }
            }
        }else // If we are the grab target
        { 
            // Then update the distance
            Grab.distanceToPlayer = distToPlayerMag;
            Grab.distanceToCursor = distToCursorMag; 
            if (distToPlayerMag >= Range || realdistanceToPlayer.y > 6) // If we're farter away from the player then the range
                Grab = resetInd; // Then we can't be the Grab target
        }



        /// --------------
        /// Hover Targets
        /// --------------
        if(Grab.ind == this && Hover.ind == this)
        {
            // We can't be both the hover and grab ind, so get rid of the hover reference
            Hover = resetInd;
        }

        if(Grab.ind != this) // If we're not actively the grab
        {
            if(Hover.ind != this) // If we're not actively the hover
            {
                // We have the potential to be a hover
                if (distToCursorMag < ((OverrideHoverRange > 0) ? OverrideHoverRange : Range)) // If we're close enough to be considered hover
                {
                    // Check if we can actually be hover
                    if (distToCursorMag < Hover.distanceToCursor)
                    {
                        IndicatorInfo i = new IndicatorInfo();
                        i.distanceToPlayer = distToPlayerMag;
                        i.distanceToCursor = distToCursorMag;
                        i.ind = this;
                        Hover = i;
                    }
                }
            }else // If we are the hover
            {
                // Update the hover
                Hover.distanceToCursor = distToCursorMag;
                Hover.distanceToPlayer = distToPlayerMag;
                if (distToCursorMag >= ((OverrideHoverRange > 0) ? OverrideHoverRange : Range))
                    Hover = resetInd;
            }

        }
    }

    void handlePresets()
    {
		if(Preset != presetPrev)
        {
            switch(Preset)
            {
                case usableIndcPreset.Interact:
                    myText.text = "Interact";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-5, 2.3f, 0); 
                    break;
                case usableIndcPreset.Loot: 
                    myText.text = "Loot";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-22, 2.3f, 0);
                    break;
                case usableIndcPreset.PickUp: 
                    myText.text = "Pick Up";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-8, 2.3f, 0);
                    break;
                case usableIndcPreset.Open:
                    myText.text = "Open";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-20, 2.3f, 0);
                    break;
                case usableIndcPreset.Close:
                    myText.text = "Close";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-18, 2.3f, 0);
                    break;
                case usableIndcPreset.Locked:
                    myText.text = "Locked";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-7, 2.3f, 0);
                    break;
                case usableIndcPreset.Talk:
                    myText.text = "Talk";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-15, 2.3f, 0);
                    break;
                case usableIndcPreset.Save:
                    myText.text = "Save";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-20, 2.3f, 0);
                    break;
                case usableIndcPreset.Disarm:
                    myText.text = "Disarm";
                    eBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(-7, 2.3f, 0);
                    break;

            }
        } 
        presetPrev = Preset;
    }

    GameObject Player
    {
        get
        {
            if(player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                    return this.gameObject;
            }

            return player;

        }
    }

    void UpdateAlphaImages()
    {
        Color c = Color.white;
        c.a = alphaValue;
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i].color = c;
        }

        Color c2 = myText.color;
        c2.a = alphaValue;
        myText.color = c2;

        if (talkText != null)
            talkText.color = c2;
    }

    public static void ResetInd()
    {
        IndicatorInfo i;
        // A copy of the reset ind, hardcoded because static
        i.ind = null;
        i.distanceToPlayer = 100000;
        i.distanceToCursor = 100000;
        Grab = i;
    }

    public static bool IsAvailable
    {
        get { return (Grab.ind != null); }
    }

	void OnDisable()
	{
		if(Grab.ind == this)
		{
			Grab = resetInd;
		}

		if(Hover.ind == this)
		{
			Hover = resetInd;
		}
	}

    private void OnDestroy()
    {
        OnDisable();
    }
}

[System.Serializable]
public struct IndicatorInfo
{
    public float distanceToCursor;
    public float distanceToPlayer;
    public UsableIndicator ind;
}
 

