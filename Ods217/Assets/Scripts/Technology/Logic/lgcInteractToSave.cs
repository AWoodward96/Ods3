using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effects Script
/// When it's interacted with by pressing [E] it will save the game.
/// </summary>

[RequireComponent(typeof(UsableIndicator))]
public class lgcInteractToSave : MonoBehaviour
{

	[Range(.1f, 10)]
	public float Range;
	public bool Interactable;

	UsableIndicator ind_Interactable;
	GameObject Player;

	// Use this for initialization
	void Start ()
	{
		ind_Interactable = GetComponent<UsableIndicator>();
	}

	// Update is called once per frame
	void Update ()
	{
		if (Player == null) // Ensure that we have the player
			Player = GameObject.FindGameObjectWithTag("Player");

		// Check to make sure we can even do this calculation
		// At this point we'll know if there's a player in the scene at all
		if (Player != null)
		{
			Vector3 dist = transform.position - Player.transform.position;
			Interactable = (dist.magnitude <= Range);
			ind_Interactable.ind_Enabled = Interactable;
		}


		// If we get input that we want to interact, and we're able to interact with it, save!
		if (Input.GetKeyDown(KeyCode.E) && Interactable)
		{
			GameObject.Find("GameManager").GetComponent<GameManager>().WriteToCurrentSave();
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
