using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mobTCowardStatic : mobCowardUnit
{

    public bool mytriggered;
    [Range(1, 10)]
    public float myRange;
    [TextArea(1, 50)]
    public string myDialog;
    UsableIndicator myusableIndicator;

    bool canPickUp = true;

    public override void Start()
    {
        myusableIndicator = GetComponent<UsableIndicator>();
        base.Start();
    }


    public override void CheckDistance()
    {
        if (Triggered)
        {
            base.CheckDistance();

        }
        else
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

            }
            else
            {
                Vector3 dist = player.transform.position - transform.position;
                myusableIndicator.ind_Enabled = (dist.magnitude < myRange);

                if (Input.GetKeyDown(KeyCode.E) && dist.magnitude < myRange)
                {
                    CutsceneManager.instance.StartCutscene(myDialog);
                }
            }
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
            List<usableWeapon> Weapons = myZone.Weapons;

            // Look for a weapon we can pick ups
            usableWeapon desiredWeapon = null;
            for (int i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i].isBeingUsed)
                    continue;

                if (!Weapons[i].gameObject.activeInHierarchy)
                    continue;

                Vector3 dist = Weapons[i].transform.position - transform.position;
                if (dist.magnitude < 3)
                {
                    desiredWeapon = Weapons[i];
                    break;
                }
            }

            // If there is a weapon nearby we can pick up
            if (desiredWeapon != null)
            {

                Vector3 d = desiredWeapon.transform.position - transform.position;
                d = GlobalConstants.ZeroYComponent(d);

                // Move towards it
                myCC.ApplyForce(d);

                // If we're close enough pick it up
                if (d.magnitude < desiredWeapon.Range || d.magnitude < 1)
                {
                    desiredWeapon.PickedUp(this);
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
}
