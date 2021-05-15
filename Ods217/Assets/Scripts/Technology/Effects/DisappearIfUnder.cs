using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearIfUnder : MonoBehaviour {

    public MeshRenderer FadeMe; 
    Color ExteriorColor = Color.white;  

    bool isIn;
     

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () { 
        ExteriorColor = Color.Lerp(ExteriorColor, (isIn ? Color.clear : Color.white), Time.deltaTime * 10);

        FadeMe.material.SetColor("_Color", ExteriorColor);
	}


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            isIn = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            isIn = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            isIn = true;
    }
 
 
}
