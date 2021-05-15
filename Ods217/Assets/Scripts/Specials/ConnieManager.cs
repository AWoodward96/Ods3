using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnieManager : MonoBehaviour {


    public static ConnieManager instance;
    public Animator[] Conimators;

    [TextArea(3, 100)]
    public string[] DialogOptions; 
    GameObject PlayerObj;

 
    private void OnEnable()
    {
        PlayerObj = GameObject.FindGameObjectWithTag("Player");
        instance = this;
    }
     

    // Update is called once per frame
    void Update ()
    {
        /// Side note: 
        /// 'Conitor' is a word derived from the phrase "Connie Monitor"
        /// Connie is the Console, actually named Console 13
        /// This is just rediculous at this point

        GameObject conitorGameObject;
        float maxDist = 10000;
        float curDist = 0;
        float index = -1;

        // Get which conitor is closest to the player
        for (int i =0; i < Conimators.Length; i ++)
        {
            conitorGameObject = Conimators[i].gameObject;
            
            curDist =  Vector3.Distance(playerRef.transform.position, conitorGameObject.transform.position);

            if(curDist < maxDist)
            {
                index = i;
                maxDist = curDist;
            }
        }

        // Now go through each conitor and turn the proper one on, while turning the rest off
        for(int i = 0; i < Conimators.Length; i ++)
        {
            Conimators[i].SetBool("State", index == i);
        }
	}

    GameObject playerRef
    {
        get
        {
            if(PlayerObj == null)
                PlayerObj = GameObject.FindGameObjectWithTag("Player");
            return PlayerObj;
        }
    }
}
