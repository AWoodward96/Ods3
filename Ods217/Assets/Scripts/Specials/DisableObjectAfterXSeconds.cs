using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableObjectAfterXSeconds : MonoBehaviour {

    public float Time;

	// Use this for initialization
	void Start () {
        StartCoroutine(Wait());
	}
	
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(Time);
        gameObject.SetActive(false);
    }
}
