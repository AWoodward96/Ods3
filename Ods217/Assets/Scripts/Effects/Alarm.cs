using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class Alarm : MonoBehaviour {

    public Color Color1;
    public Color Color2;

    Light myLight;

	// Use this for initialization
	void Start () {
        myLight = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
        myLight.color = Color.Lerp(Color1, Color2, Mathf.PingPong(Time.time, 1));
    }
}
