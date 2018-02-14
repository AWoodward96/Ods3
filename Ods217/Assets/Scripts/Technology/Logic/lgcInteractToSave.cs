using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Effects Script
/// When it's interacted with by pressing [E] it will save the game.
/// </summary>
 
public class lgcInteractToSave : MonoBehaviour
{ 
	public bool HealOnSave;

	UsableIndicator ind_Interactable;
	GameObject Player;
    AudioSource src;

	// Use this for initialization
	void Start ()
	{
		ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Interact;
        ind_Interactable.Output = InteractDelegate;

        src = GetComponent<AudioSource>();
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
			"Decision(Save,Yes,No)"
		);

        if(HealOnSave)
        {
            PlayerScript p =GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
            if (p)
            {
                p.myVisualizer.ShowMenu();
                p.myUnit.CurrentHealth = p.myUnit.MaxHealth;
            }
        }
    }
 
}