using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldSwitch : MonoBehaviour
{
	public bool regresses;	// Does the object go backwards when not being interacted with?
    public bool regressLock = false; // Will it regress when myValue = 1?
	public float regressionLength;	// Time it takes to regress from 100% to 0%, in seconds

	public UsableIndicator ind_Interactable;
	GameObject Player;
    
    [Space(20)]
    [Header("Value Data")]
	public float myValue;	// The value currently held in the switch
    public float Speed = 1;
    public bool Invert;
	protected bool heldThisFrame;	// Was the switch held this frame?
    bool locked = false;

    Animator myAnim;

	// Use this for initialization
	void Start()
	{
		ind_Interactable = GetComponentInChildren<UsableIndicator>();
		ind_Interactable.Output = OnInteract;

        Animator[] anims = GetComponentsInChildren<Animator>();
        foreach(Animator a in anims)
        {
            if (a.name.Contains("Screen"))
                myAnim = a;
        } 

		heldThisFrame = false;
	}

	// Update is called once per frame
	protected virtual void FixedUpdate()
	{
		heldThisFrame = ind_Interactable.lasered || ind_Interactable.held;

		// Adjust the value based on whether or not the switch was held this frame
		if(heldThisFrame)
		{
			myValue = Mathf.MoveTowards(myValue, (Invert) ? 0 : 1, Time.fixedDeltaTime * Speed);

            if (regressLock && myValue >= ((Invert) ? 0 : 1))
                locked = true;
		}
		else if(regresses)
		{
			if(!(regressLock && locked))
			{
				myValue = Mathf.MoveTowards(myValue, (Invert) ? 1 : 0, (Time.fixedDeltaTime / regressionLength));
			}
		}

        HandleAnim();
	}

    protected virtual void HandleAnim()
    {
        if (myAnim == null)
            return;

        myAnim.SetFloat("State", myValue);
        myAnim.SetBool("Locked", locked);
 

    }

		
	// EIndicator delegate
	void OnInteract()
	{
		/*heldThisFrame = true;*/
	}
}
