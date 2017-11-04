using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Special Effect 
/// Randomly flickers a light
/// </summary>
[RequireComponent(typeof(Light))]
public class RandomBlink : MonoBehaviour
{

    [Range(.1f, 5f)]
    public float TimerMin; // The minimum range of the random variable
    [Range(.1f, 10f)]
    public float TimerMax; // The maximum range of the random variable

    float randomTime;
    float randomBlink;
    float currentTime;
    Light myLight;

    // Use this for initialization
    void Start()
    {
        currentTime = 0;
        randomTime = Random.Range(TimerMin, TimerMax);
        randomBlink = Random.Range(0, .2f); // An extra couple of milliseconds to freeze the flicker so you actually can see it
        myLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        // If the current time is greater then the selected random time
        if (currentTime > randomTime)
        {
            // Flicker the light
            myLight.enabled = !myLight.enabled;

            // Let it stay on that state at least for a while
            if (currentTime > randomTime + randomBlink)
            {
                currentTime = 0;
                randomTime = Random.Range(TimerMin, TimerMax);
            }


        }
    }
}
