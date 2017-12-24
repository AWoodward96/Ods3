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
        usableIndicator = GetComponent<UsableIndicator>();
        base.Start();
    }


    public override void CheckDistance()
    { 
        if(Triggered)
        {
            base.CheckDistance();

        }else
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

            }
            else
            {
                Vector3 dist = player.transform.position - transform.position;
                usableIndicator.ind_Enabled = (dist.magnitude < myRange);

                if (Input.GetKeyDown(KeyCode.E) && dist.magnitude < myRange)
                {
                    CutsceneManager.instance.StartCutscene(myDialog);
                }
            }
        } 
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
                if (d.magnitude < desiredWeapon.heldData.Range || d.magnitude < 1)
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
