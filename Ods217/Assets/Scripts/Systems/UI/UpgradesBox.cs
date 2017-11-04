using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesBox : MonoBehaviour {

    public enum UpgradeBoxState { Locked, CanPurchase, Unlocked };
    public UpgradeBoxState State;
    public int UpgradeRank;
    public string UpgradeName;

    public Sprite Locked;
    public Sprite CanPurchase;
    public Sprite Unlocked;

    Image myImage;
     

	// Use this for initialization
	void Start () {
        myImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
        switch(State)
        {
            case UpgradeBoxState.CanPurchase:
                myImage.sprite = CanPurchase;
                break;
            case UpgradeBoxState.Locked:
                myImage.sprite = Locked;
                break;
            case UpgradeBoxState.Unlocked:
                myImage.sprite = Unlocked;
                break;
        }
	}
}
