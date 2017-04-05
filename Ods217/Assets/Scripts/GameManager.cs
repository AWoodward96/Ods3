using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {


    public static GameManager instance; 
    public static int ScrapCount;

    public int scrapcount;
    // Use this for initialization
	void Start () {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

	}
	
	// Update is called once per frame
	void Update () {
        scrapcount = ScrapCount;
	}
}
