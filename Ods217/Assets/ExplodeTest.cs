using System.Collections;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

public class ExplodeTest : MonoBehaviour {

    EffectorExplode explode; 
    [Range(1,359)]
    public float radius;
    [Range(1, 360)]
    public float Angle;
    [Range(-360, 360)]
    public float StartRot;

    [Range(0, 10)]
    public float Strength;

    // Use this for initialization
    void Start () {
        explode = GetComponent<EffectorExplode>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if(Input.GetKeyDown(KeyCode.E))
        {
            explode.ExplodeAt(Vector3.down,radius,Angle,StartRot,Strength);
        }
	}
}
