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
		ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
        ind_Interactable.Output = InteractDelegate;
	}
 

    void InteractDelegate()
    {
        //GameObject.Find("GameManager").GetComponent<GameManager>().WriteToCurrentSave();

		// TODO: Replace this with its analogue in the cutscene manager!
		//DialogManager.instance.ShowDialog("Would you like to save?");

		CutsceneManager.instance.StartCutscene
		(
			"HaltPlayer()\n" +
			"Loadchar(Con1,Console)\n" +
			"Say(Console,Would you like to save?\n" +
			"Decision(Save,Yes,No)\n"
		);
    }

	private void OnDrawGizmos()
	{
		Color c = Color.blue;
		c.a = .1f;
		Gizmos.color = c;
		Gizmos.DrawSphere(transform.position, Range);
	}
}