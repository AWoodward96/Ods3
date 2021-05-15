using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mobTCowardUnit : mobCowardUnit {

    [Space(20)]
    [Header("mobTCowardUnit")]
    public bool isTriggered;
    [Range(1,10)]
    public float myRange;
    [TextArea(1, 50)]
    public string myDialog;
    UsableIndicator usableIndicator;

    bool canPickUp = true;

    public override void Start()
    {
        usableIndicator = GetComponentInChildren<UsableIndicator>();
        usableIndicator.Preset = UsableIndicator.usableIndcPreset.Talk;
        usableIndicator.Output = TalkDelegate;
        base.Start();
    }


    public override void CheckDistance()
    { 
        if(Triggered)
        {
            usableIndicator.Preset = UsableIndicator.usableIndcPreset.Disarm;
            usableIndicator.Output = DisarmDelegate;
            base.CheckDistance();

        }else
        {
            usableIndicator.Preset = UsableIndicator.usableIndcPreset.Talk;
            usableIndicator.Output = TalkDelegate;
        } 
    }

    void TalkDelegate()
    { 
        if(myDialog != "")  
            CutsceneManager.instance.StartCutscene(myDialog);
    }

    public override bool CheckAggro()
    {
        if (isTriggered)
            return base.CheckAggro();
        else
            return false;
    }

    public override bool Triggered
    {
        get
        {
            return isTriggered;
        }

        set
        {
            isTriggered = value;
        }
    }

    public override void AggroState()
    {
        if (canPickUp)
        {
            List<WeaponBase> Weapons = myZone.Weapons;

            // Look for a weapon we can pick ups
            WeaponBase desiredWeapon = null;
            for (int i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i].heldData.PickedUp)
                    continue;

                if (!Weapons[i].gameObject.activeInHierarchy)
                    continue;

                Vector3 dist = Weapons[i].ThrownObject.transform.position - transform.position;
 
                if (dist.magnitude < 4)
                {
                    desiredWeapon = Weapons[i];
                    break;
                }
            }

            // If there is a weapon nearby we can pick up
            if (desiredWeapon != null)
            {
                Vector3 d = desiredWeapon.ThrownObject.transform.position - transform.position;
                d = GlobalConstants.ZeroYComponent(d);

                // Move towards it
                myCC.ApplyForce(d);

                // If we're close enough pick it up
                if (d.magnitude < desiredWeapon.heldData.ind_MyIndicator.Range || d.magnitude < 1)
                {
                    desiredWeapon.heldData.PickUp(this);
                    canPickUp = false;
                    StartCoroutine(pickupCRT());
                }

            }
            else
                base.AggroState();
            }

    }

    IEnumerator pickupCRT()
    {
        yield return new WaitForSeconds(1);
        canPickUp = true;
    }
}
