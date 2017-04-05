using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterXSeconds : MonoBehaviour {

    public float time;
	// Use this for initialization
	void Start () {
        StartCoroutine(death());
	}
	
    IEnumerator death()
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}
