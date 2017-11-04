using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UsableIndicator))]
[ExecuteInEditMode]
public class weaponDispenser : MonoBehaviour {

    [Header("Weapon Data")]
    public GameObject WeaponPrefab;
    public GameObject WeaponReference;

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

        visualizerRenderer.sprite = WeaponPrefab.GetComponentInChildren<SpriteRenderer>().sprite;

        ind_Indicator = GetComponentInChildren<UsableIndicator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.isPlaying)
        { 

             if (Player == null) // Ensure that we have the player
                Player = GameObject.FindGameObjectWithTag("Player");

            // Check to make sure we can even do this calculation
            // At this point we'll know if there's a player in the scene at all
            if (Player != null)
            {
                Vector3 dist = transform.position - Player.transform.position;
                ind_Indicator.ind_Enabled = (dist.magnitude <= Range) && (WeaponReference == null); 
            }
             
            // If we get input that we want to interact, and we're able to interact with it
            if (Input.GetKeyDown(KeyCode.E) && ind_Indicator.ind_Enabled && WeaponReference == null)
            {
                GameObject obj = Instantiate(WeaponPrefab) as GameObject;
                obj.GetComponent<usableWeapon>().PickedUp();
                WeaponReference = obj;
            }

            visualizerRenderer.enabled = (WeaponReference == null);
        }
	}

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }
}
