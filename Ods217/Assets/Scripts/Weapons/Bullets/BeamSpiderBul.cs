using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamSpiderBul : BulletBase {

    public override void OnTriggerEnter(Collider other)
    {
        SpiderBroT1 bro1 = other.GetComponent<SpiderBroT1>();
        if (bro1 != null)
            return;


        base.OnTriggerEnter(other);
    }
}
