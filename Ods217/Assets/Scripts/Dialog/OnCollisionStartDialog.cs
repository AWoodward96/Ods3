using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OnCollisionStartDialog : MonoBehaviour {

    [TextArea(1,100)]
    public string MyDialog;
    BoxCollider myBoxCollider;
    bool shown;

	// Use this for initialization
	void Start () {
        myBoxCollider = GetComponent<BoxCollider>();
        myBoxCollider.isTrigger = true;
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
