using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostPodSendoff : MonoBehaviour, IPermanent {

    ZoneScript z;
    public Animator myAnimator;
    public Animator[] myEnginesAnimator;

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
            return false;
        }

        set
        { 
            StartCoroutine(Run());
        }
    }

    IEnumerator Run()
    {
        myAnimator.SetTrigger("Move");
        yield return new WaitForSeconds(5);
        foreach (Animator a in myEnginesAnimator)
            a.SetBool("State", true);
        yield return new WaitForSeconds(2);
        GetComponent<SimpleMove>().enabled = true;
    }
}
