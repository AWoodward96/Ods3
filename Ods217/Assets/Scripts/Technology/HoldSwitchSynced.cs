using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldSwitchSynced : HoldSwitch {

    
    public static float globalValue;
    bool flippin = false;

    protected override void FixedUpdate()
    {

        if (heldThisFrame)
            globalValue = myValue;
        else
            myValue = globalValue;

        if ((myValue == 1 || myValue == 0) && !flippin)
        {
            flippin = true;
            StartCoroutine(flipInvert(myValue == 1));
        }

		base.FixedUpdate();
    }

    IEnumerator flipInvert(bool _v)
    {
        yield return new WaitForSeconds(.66f);
        flippin = false;
        Invert = _v;
    }


}
