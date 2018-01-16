using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class onBouncePlaySound : MonoBehaviour {
     
    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<AudioSource>().Play();
    }
}
