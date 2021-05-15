using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manipulator : WeaponBase
{
    public override void FireWeapon(Vector3 _dir)
    {
		// Break out if we can't even shoot
		if (currentshootCD < weaponData.fireCD)
			return;

		if (myOwner.MyUnit.CurrentEnergy < weaponData.shotCost)
			return;

		if (!myEnergy)
			myEnergy = myOwner.gameObject.GetComponent<EnergyManager>();

		if (myEnergy.BrokenEnergy)
			return;

		if(myBullets.Count == 0)
		{
			maxBullets = 1;
			MakeBullets();
			currentBullet = 0;
			if(myBullets.Count == 0)
				return;
		}

		// Do something based on the type
		// For now we'll just shoot one bullet

		myBullets[0].gameObject.SetActive(true); 
		myBullets[0].myOwner = (myOwner);
		myBullets[0].myInfo = (weaponData);
		myBullets[0].transform.position = transform.position + (_dir.normalized/2);
		myBullets[0].Shoot(_dir);

		currentshootCD = 0;

		myEnergy.ExpendEnergy(weaponData.shotCost);


		//Play the sound
		if(myAudioSource != null)
		{
			myAudioSource.clip = ShootClip;
			myAudioSource.Play();
		}

		// Check for an animator
		Animator anim = RotateObject.GetComponentInChildren<Animator>();
		if (anim != null)
			anim.SetTrigger("Fire");

		if (myOwner != null)
			myOwner.myVisualizer.ShowMenu();

		if(weaponData.fireCD < 0)
		{
			// if our fire CD is less then 0 then it's a single use weapon
			ReleaseWeapon();
		}

		isFiring = true;

		return;
    }

	public override void UpdateBullets()
	{ 
		if (myBullets[0].Fired)
		{
			myBullets[0].UpdateBullet(); 
		}
		/*else
		{
			if (myBullets[0].gameObject.activeInHierarchy) 
				myBullets[0].gameObject.SetActive(false);  
		}*/
	}

	public override void MakeBullets()
	{
		// Ensure the bullets are properly stored
		if (myBullets.Count != 1)
		{
			// If they aren't, clear the list
			foreach (BulletBase o in myBullets)
			{
				Destroy(o.gameObject);
			}

			myBullets = new List<BulletBase>();

			// Then refill it
			GameObject newObj = (GameObject)Instantiate(weaponData.BulletObject);
			myBullets.Add(newObj.GetComponent<BulletBase>());
			newObj.SetActive(false);
		}
	}
}
