using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonationObstacles : MonoBehaviour,IPermanent {

    ZoneScript z;
    public bool State;
    public GameObject ObstaclePrefab;
    List<GameObject> Obstacles = new List<GameObject>();
    public int ObstacleCnt = 5;
    int obsIndex = 0;
    GameObject Player;

    float dTime;
    float dVal;

    // Use this for initialization
    void Start () {
        Obstacles = new List<GameObject>();
        for(int i = 0; i < ObstacleCnt; i++)
        {
            GameObject o = (GameObject)Instantiate(ObstaclePrefab);
            o.SetActive(false);
            Obstacles.Add(o);
        }

        Player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
	    if(Triggered)
        {
            dTime += Time.deltaTime;
            if (dTime < dVal)
                return;

            dTime = 0;
            dVal = UnityEngine.Random.Range(1, 3);


            // Get a valid obstalce that we can drop
            GameObject o = Obstacles[obsIndex]; 
            obsIndex++;
            if (obsIndex >= ObstacleCnt)
                obsIndex = 0;
             
            // Ok now spawn that object
            // Check if player is null
            if (Player == null)
                Player = GameObject.FindGameObjectWithTag("Player");
            else
            {
                o.transform.position = Player.transform.position;
                o.transform.position += Vector3.up * 40;
                o.transform.position += (UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(1, 4));
                o.transform.position += (Player.GetComponent<CController>().ProjectedPosition - Player.transform.position) * 3;
                o.GetComponent<Rigidbody>().velocity = Vector3.down;
                o.SetActive(true);
            }

        }
	}




    public ZoneScript myZone
    {
        get
        {
            return z;
        }

        set
        {
            z = value;    
        }
    }

    public bool Triggered
    {
        get
        {
            return State;
        }

        set
        {
            State = value;
        }
    }

    public void Activate()
    { 
    }
}
