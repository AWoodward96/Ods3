using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UsableIndicator))]
[ExecuteInEditMode]
public class weaponDispenser : MonoBehaviour {

    [Header("Weapon Data")]
    public WeaponBase WeaponPrefab;
    public WeaponBase WeaponReference;

    [Header("Object Data")]
    [Range(1,50)]
    public float Range = 5;
    SpriteRenderer visualizerRenderer;
    UsableIndicator ind_Indicator;

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
        ind_Indicator = GetComponentInChildren<UsableIndicator>();
        ind_Indicator.Preset = UsableIndicator.usableIndcPreset.PickUp;
        ind_Indicator.Output = InteractDelegate;

        Player = GameObject.FindGameObjectWithTag("Player");

        // Set up the weapon reference
        if(WeaponReference == null && Application.isPlaying)
        { 
            GameObject obj = Instantiate(WeaponPrefab.gameObject) as GameObject;
            WeaponBase b = obj.GetComponent<WeaponBase>();

            WeaponReference = b;
            WeaponReference.gameObject.SetActive(false);
        }
  
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.isPlaying)
        {
            bool pickedup = isPickedUp;
 
            visualizerRenderer.enabled = !pickedup;
        }
	}

    void InteractDelegate()
    { 
        WeaponReference.gameObject.SetActive(true);
        WeaponReference.heldData.PickUp(Player.GetComponent<IMultiArmed>());
    }

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }

    bool isPickedUp
    {
        get
        {
            if (WeaponReference != null)
                if (WeaponReference.gameObject.activeInHierarchy)
                    return true;
                else
                    return WeaponReference.heldData.PickedUp;
            else
                return false; 
        }

    }
}
