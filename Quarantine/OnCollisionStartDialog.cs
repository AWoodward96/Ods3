using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script that starts a dialog prompt when you run into this objects collider
/// Can be used for cutscene starters
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class OnCollisionStartDialog : MonoBehaviour
{

    [TextArea(1, 100)]
    public string MyDialog; // The dialog prompt

    BoxCollider myBoxCollider;
    bool shown; // A variable to stop the cutscene from triggering more then once

    // Use this for initialization
    void Start()
    {
        myBoxCollider = GetComponent<BoxCollider>();
        myBoxCollider.isTrigger = true; // Ensure that the object is a trigger
        shown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !shown)
        {
            DialogManager.instance.ShowDialog(MyDialog);
            shown = true;
        }
    }
}
