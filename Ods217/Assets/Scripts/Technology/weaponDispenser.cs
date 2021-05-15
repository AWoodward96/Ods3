using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(UsableIndicator))]
[ExecuteInEditMode]
public class weaponDispenser : MonoBehaviour, ISavable {

    [Header("Weapon Data")]
    public WeaponBase WeaponPrefab;
    public WeaponBase WeaponReference;

    [Header("Object Data")] 
    SpriteRenderer visualizerRenderer;
    UsableIndicator ind_Indicator;

    [HideInInspector]
    public bool hasWeapon = true;

	[Header("ISavable Variables")]
	public int saveID = -1;

	[HideInInspector]
	public bool saveIDSet = false;

	public int SaveID
	{
		get
		{
			return saveID;
		}
		set
		{
			saveID = value;
		}
	}

	public bool SaveIDSet
	{
		get
		{
			return saveIDSet;
		}
		set
		{
			saveIDSet = value;
		}
	}

    AudioSource src;

    GameObject Player;

	// Use this for initialization
	void Start () {
        
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < sprites.Length; i ++)
        {
            if(sprites[i].name == "WeaponImage")
            {
                visualizerRenderer = sprites[i];
            }
        }

        visualizerRenderer.sprite = WeaponPrefab.RotateObject.GetComponent<SpriteRenderer>().sprite;

        if (Application.isPlaying)
        {
            ind_Indicator = GetComponentInChildren<UsableIndicator>();
            ind_Indicator.Preset = UsableIndicator.usableIndcPreset.PickUp;
            ind_Indicator.Output = InteractDelegate;
        }


        Player = GameObject.FindGameObjectWithTag("Player");

        // Set up the weapon reference
        if(WeaponReference == null && Application.isPlaying)
        { 
            GameObject obj = Instantiate(WeaponPrefab.gameObject) as GameObject;
            
            WeaponBase b = obj.GetComponent<WeaponBase>();

            obj.name = WeaponPrefab.name;

            WeaponReference = b;
            WeaponReference.gameObject.SetActive(false);
        }

        src = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.isPlaying)
        {
            bool pickedup = isPickedUp;
 
            visualizerRenderer.enabled = !pickedup;
			ind_Indicator.Disabled = pickedup;
        }
	}

    void InteractDelegate()
    { 
        WeaponReference.gameObject.SetActive(true);
        WeaponReference.heldData.PickUp(Player.GetComponent<IMultiArmed>());

        if (src != null)
            src.Play();

		hasWeapon = false;
    }
 

    bool isPickedUp
    {
        get
        {
			if(hasWeapon == false)
			{
				return true;
			}

			if (WeaponReference != null)
                if (WeaponReference.gameObject.activeInHierarchy)
                    return true;
                else
                    return WeaponReference.heldData.PickedUp;
            else
                return false; 
        }

    }

	public string Save()
	{
		StringWriter data = new StringWriter();

		data.WriteLine(hasWeapon);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		hasWeapon = bool.Parse(data[0].Trim());
	}
}
