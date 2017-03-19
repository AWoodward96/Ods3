using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTiletexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnDrawGizmos()
    {

        this.gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(this.gameObject.transform.lossyScale.x / 2, this.gameObject.transform.lossyScale.z / 2));
        
    }
}
