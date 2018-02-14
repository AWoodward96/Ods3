using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A logic script
/// If you're close enough, display an E indicator
/// Once interacted with, start a dialog
/// </summary>
[RequireComponent(typeof(UsableIndicator))]
public class lgcInteractToStartDialog : MonoBehaviour
{
    [Range(.1f, 10)]
    public float Range;
    public bool Interactable;

    [TextArea(1, 100)]
    public string MyDialog;

    public bool showOnce;
    bool shown;

    UsableIndicator ind_Interactable;
    GameObject Player;

    // Use this for initialization
    void Start()
    {
        ind_Interactable = GetComponentInChildren<UsableIndicator>();
        ind_Interactable.Preset = UsableIndicator.usableIndcPreset.Talk;
        ind_Interactable.Output = ShowDialogDelegate;
    }
 

    void ShowDialogDelegate()
    { 
        if (showOnce && shown)
            return;

        CutsceneManager.instance.StartCutscene(MyDialog);
        shown = true;
        if (showOnce)
            ind_Interactable.Disabled = true;
    }

    private void OnDrawGizmos()
    {
        Color c = Color.blue;
        c.a = .1f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, Range);
    }
}
