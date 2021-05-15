using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// A logic script
/// If you're close enough, display an E indicator
/// Once interacted with, start a dialog
/// </summary> 
public class lgcInteractToStartDialog : MonoBehaviour, ISavable
{  
    [TextArea(1, 100)]
    public string[] MyDialog;
    public int DialogIndex;

    public TextAsset OverrideText;

    public UsableIndicator.usableIndcPreset PresetText = UsableIndicator.usableIndcPreset.Talk;
    public bool showOnce;
    bool shown;

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

    UsableIndicator ind_Interactable;
    GameObject Player;

    // Use this for initialization
    void Start()
    {
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = PresetText;
        ind_Interactable.Output = ShowDialogDelegate;

        if(OverrideText != null)
        {
            MyDialog = new string[1];
            DialogIndex = 0;
        }

        if (ind_Interactable.talkText != null)
        {
            ind_Interactable.talkText.gameObject.SetActive(true);
            ind_Interactable.talkText.text = (DialogIndex) + "/" + (MyDialog.Length);
        }

    }


    void ShowDialogDelegate()
    { 
        if (showOnce && shown)
            return;

        if (CutsceneManager.InCutscene)
            return;

        if (CutsceneManager.instance == null)
        {
            Debug.LogError("THERE'S NO CUTSCENEMANAGER IN THIS SCENE.");
            return;
        }


        if (DialogIndex >= MyDialog.Length)
            DialogIndex = 0;

        shown = true;
        if (showOnce)
            ind_Interactable.Disabled = true;

        if (OverrideText != null) 
            CutsceneManager.instance.StartCutscene(OverrideText.text); 
        else
            CutsceneManager.instance.StartCutscene(MyDialog[DialogIndex]);

        DialogIndex++;

        // Update the usable indicators talk text
        if (ind_Interactable.talkText != null)
        {
            ind_Interactable.talkText.gameObject.SetActive(true);
            ind_Interactable.talkText.text = (DialogIndex) + "/" + (MyDialog.Length);
        }



    }

    // TODO: I feel like I shouldn't have to get components in a load OR save method; is there a way to ensure these run after Start()?
    public string Save()
	{
		StringWriter data = new StringWriter();

		ind_Interactable = GetComponentInChildren<UsableIndicator>();
		data.WriteLine(ind_Interactable.Disabled);
		data.WriteLine(DialogIndex);

		return data.ToString();
	}

	public void Load(string[] data)
	{
		ind_Interactable = GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Disabled = bool.Parse(data[0].Trim());

		DialogIndex = int.Parse(data[1]);
	}
}
