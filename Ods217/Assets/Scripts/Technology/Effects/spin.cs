using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spin : MonoBehaviour {

    public float rotationsPerMin;

    private void Update()
    {
        transform.Rotate(0, 0, 6.0f * rotationsPerMin * Time.deltaTime);
    }
}
