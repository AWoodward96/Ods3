using UnityEngine;
using System.Collections;


[RequireComponent (typeof(SpriteRenderer))]
// This is a script I dump on any 2d SpriteRenderer to get access to casting shadows
// All these options are available but not public for some reason
public class ShadowsPlease : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().receiveShadows = true;
        GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}

}
