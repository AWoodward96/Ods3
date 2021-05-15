using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Conitor : MonoBehaviour {


    [TextArea(1, 100)]
    public string[] OverrideText;
    public int LocalCount = 0;
    int LocalOptionNum = 0;

    UsableIndicator ind_Interactable;
    GameObject Player;

    // Use this for initialization
    void Start()
    {
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Talk;
        ind_Interactable.Output = ShowDialogDelegate;

        if (ind_Interactable.talkText != null)
        {
            ind_Interactable.talkText.gameObject.SetActive(true);
            ind_Interactable.talkText.text = (LocalOptionNum + LocalCount) + "/" + (ConnieManager.instance.DialogOptions.Length + OverrideText.Length);
        }
         
    }


    void ShowDialogDelegate()
    { 

        if (CutsceneManager.InCutscene)
            return;

        if (CutsceneManager.instance == null)
        {
            Debug.LogError("THERE'S NO CUTSCENEMANAGER IN THIS SCENE.");
            return;
        }


        string[] dialog = ConnieManager.instance.DialogOptions;

        if (LocalOptionNum >= dialog.Length)
        {
            LocalOptionNum = 0;
            LocalCount = 0;
        }



        if (LocalCount >= OverrideText.Length)
        {
            // If we've run through the local dialog run through the conniemanager dialog
            CutsceneManager.instance.StartCutscene(dialog[LocalOptionNum]);
            LocalOptionNum++;
        }
        else
        {
            // go through the local dialog first
            CutsceneManager.instance.StartCutscene(OverrideText[LocalCount]);
            LocalCount++; 
        }

        // Update the usable indicators talk text
        if (ind_Interactable.talkText != null)
        {
            ind_Interactable.talkText.gameObject.SetActive(true);
            ind_Interactable.talkText.text = (LocalOptionNum + LocalCount) + "/" + (ConnieManager.instance.DialogOptions.Length + OverrideText.Length);
        }

    }

}
