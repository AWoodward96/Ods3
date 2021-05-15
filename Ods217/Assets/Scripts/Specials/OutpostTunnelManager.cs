using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostTunnelManager : MonoBehaviour {

    public GameObject[] Ships;
    Vector3 initPos;

	// Use this for initialization
	void Start () {
        if(Ships.Length > 0)
        {
            initPos = Ships[0].transform.position;
            StartCoroutine(Run());
        }
    }
	
	
    IEnumerator Run()
    {
        float rndWait = Random.Range(1f, 10f);
        yield return new WaitForSeconds(rndWait); // Stagger the whole station

        while (true)
        {
            rndWait = Random.Range(1f, 5f);
            yield return new WaitForSeconds(rndWait);

            StartCoroutine(SendShip());


            rndWait = Random.Range(1f, 5f);
            yield return new WaitForSeconds(10);
            yield return false;
        }
    }

    IEnumerator SendShip()
    { 

        for(int i = 0; i < Ships.Length; i++)
        {
            Ships[i].transform.position = initPos;
            Ships[i].SetActive(true);
            yield return new WaitForSeconds(.333f); 
        }

        yield return new WaitForSeconds(2);
      
        for(int i = 0; i <Ships.Length; i ++)
        {
            Ships[i].SetActive(false);
        }
    }

}
