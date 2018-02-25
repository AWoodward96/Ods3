using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
	GameObject player;
	UsableIndicator myIndicator;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("Player");
		myIndicator = GetComponentInChildren<UsableIndicator>();

		myIndicator.Output = OnInteract;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnInteract()
	{
		player.GetComponent<PlayerScript>().numHealthpacks++;
		gameObject.SetActive(false);
	}
}
