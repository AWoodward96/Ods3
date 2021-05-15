using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutStatic1 : mobIdleStatic
{ 
    bool specOne, specTwo, specThree, specFour, specFive, specSix = false;

    [TextArea(3, 50)]
    public string specOneDialog;

    [TextArea(3, 50)]
    public string specTwoDialog;

    [TextArea(3, 50)]
    public string specThreeDialog;

    [TextArea(3, 50)]
    public string specFourDialog;

    [TextArea(3, 50)]
    public string specFiveDialog;

    [TextArea(3, 50)]
    public string specSixDialog;


    public override void OnHit(int _damage)
    {
        if (myZone != ZoneScript.ActiveZone) // Don't let them take damage if you're not in their scene
            return;


        // Firstly show the health bar (Remove this when we have the on-screen healthbar)
        myVisualizer.ShowMenu();
        if (myForceField != null)
        {
            if (MyUnit.CurrentEnergy > 0)
                myForceField.RegisterHit(_damage);
            else
            {
                UnitData.CurrentHealth -= _damage;
            }
        }
 
        if (UnitData.CurrentEnergy < 400 && !specOne)
        {
            specOne = true;
            CutsceneManager.instance.StartCutscene(specOneDialog);
        }

        if (UnitData.CurrentEnergy < 200 && !specTwo)
        {
            specTwo = true;
            CutsceneManager.instance.StartCutscene(specTwoDialog);
        }

        if (UnitData.CurrentEnergy < 1 && !specThree)
        {
            specThree = true;
            CutsceneManager.instance.StartCutscene(specThreeDialog);
        }

        if (UnitData.CurrentHealth < 500 && !specFour)
        {
            specFour = true;
            CutsceneManager.instance.StartCutscene(specFourDialog);
        }

        if (UnitData.CurrentHealth < 250 && !specFive)
        {
            specFive = true;
            CutsceneManager.instance.StartCutscene(specFiveDialog);
        }

        if (UnitData.CurrentHealth < 1 && !specSix)
        {
            // troll the player
            specSix = true;
            CutsceneManager.instance.StartCutscene(specSixDialog);
            StartCoroutine(bye());
        }
    } 
    
    IEnumerator bye()
    {
        yield return new WaitForSeconds(2); 
        Application.Quit();
    }
}
