using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class RandomBlink : MonoBehaviour {

    [Range(.1f, 5f)]
    public float TimerMin;
    [Range(.1f, 10f)]
    public float TimerMax;

    float randomTime;
    float randomBlink;
    float currentTime;
    Light myLight;

    // Use this for initialization
    void Start () {
        currentTime = 0;
        randomTime = Random.Range(TimerMin, TimerMax);
        randomBlink = Random.Range(0, .2f);
        myLight = GetComponent<Light>();	
	}
	
	// Update is called once per frame
	void Update () {
        currentTime += Time.deltaTime;

        if(currentTime > randomTime)
        {
            myLight.enabled = !myLight.enabled;

            if (currentTime > randomTime + randomBlink)
            {
                currentTime = 0;
                randomTime = Random.Range(TimerMin, TimerMax); 
            }
        

        }
	}
}
