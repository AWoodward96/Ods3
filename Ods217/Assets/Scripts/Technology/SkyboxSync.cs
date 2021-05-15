using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxSync : MonoBehaviour {

    public Camera CameraSync;
    public float movementRatio = .01f;
    Vector3 myInitialPosition; 

    private void Start()
    {
        myInitialPosition = transform.position;
    }

    // Update is called once per frame
    void Update () {
        transform.position = myInitialPosition + (CameraSync.transform.position * movementRatio);
    }
}
