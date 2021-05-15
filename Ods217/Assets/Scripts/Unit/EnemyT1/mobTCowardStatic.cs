using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mobTCowardStatic : mobCowardUnit
{

    public bool mytriggered; 

    [TextArea(1, 50)]
    public string[] myDialog;
    public int dialogCount;

    UsableIndicator myusableIndicator;

    bool canPickUp = true;

    public override void Start()
    {
        myusableIndicator = GetComponentInChildren<UsableIndicator>();
        myusableIndicator.Preset = UsableIndicator.usableIndcPreset.Talk;

        if (myusableIndicator.talkText != null)
        {
            myusableIndicator.talkText.gameObject.SetActive(true);
            myusableIndicator.talkText.text = (dialogCount) + "/" + (myDialog.Length); 
        }


        base.Start();

    }

    public override void CheckDistance()
    {
        if (Triggered)
        {
            myusableIndicator.Preset = UsableIndicator.usableIndcPreset.Disarm;
            myusableIndicator.Output = DisarmDelegate;
            base.CheckDistance();

        }
        else
        {
            myusableIndicator.Preset = UsableIndicator.usableIndcPreset.Talk;
            myusableIndicator.Output = TalkDelegate;
        }
    }

    void TalkDelegate()
    {
        if (!CutsceneManager.InCutscene)
        { 
            CutsceneManager.instance.StartCutscene(myDialog[dialogCount]);
             
            if (myusableIndicator.talkText != null)
            {
                myusableIndicator.talkText.gameObject.SetActive(true);
                myusableIndicator.talkText.text = (dialogCount + 1) + "/" + (myDialog.Length);
            }

            dialogCount++;
            if (dialogCount >= myDialog.Length)
                dialogCount = 0;

        }
    }

    public override bool CheckAggro()
    {
        if (mytriggered)
            return base.CheckAggro();
        else
            return false;
    }

    public override bool Triggered
    {
        get
        {
            return mytriggered;
        }

        set
        {
            mytriggered = value;
        }
    }

    public override void AggroState()
    {
        if(canPickUp)
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
                if (dist.magnitude < 3)
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
            else if (SeePlayer())
            {
                // Look towards the player and shoot
                LastSeen = player.transform.position;
                myWeapon.FireWeapon(player.transform.position - transform.position);
            }
        }

    }

    IEnumerator pickupCRT()
    {
        yield return new WaitForSeconds(1);
        canPickUp = true;
    }

    public override void OnMelee(int _damage)
    {
        if(Triggered)
            base.OnMelee(_damage);
    }
}
