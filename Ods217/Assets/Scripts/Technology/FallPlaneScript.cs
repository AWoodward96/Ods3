using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlaneScript : MonoBehaviour
{
	GameObject player;

	public int penalty;	// How much HP does the player lose for falling?

	static int numWeaponTosses = 0;
	static int maxWeaponTosses = 5;

	int currentBox = 0;	// Which spawn point is the player located at?

	List<Collider> spawnBoxes;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindGameObjectWithTag("Player");

		spawnBoxes = new List<Collider>();

		GetComponentsInChildren<Collider>(spawnBoxes);
		for(int i = 0; i < spawnBoxes.Count; i++)
		{
            spawnBoxes[i].isTrigger = true;

			if(spawnBoxes[i] == GetComponent<Collider>())
			{
				spawnBoxes.RemoveAt(i);
				break;
			}
		}
	} 

	void Update()
	{
		for(int i = 0; i < spawnBoxes.Count; i++)
		{
			if(spawnBoxes[i].bounds.Contains(player.transform.position))
			{
				currentBox = i;
				break;
			}
		}
	}
	 
    void Run(Collider other)
    {
        // Only needs to affect the player
        if (other.gameObject == player)
        {
            PlayerScript ps = player.GetComponent<PlayerScript>();

            player.GetComponentInChildren<ForceFieldScript>().RegisterHit(ps.myUnit.MaxEnergy);
            ps.myVisualizer.ShowMenu();

            if (ps.myUnit.CurrentHealth > 0)
            {
                //player.transform.position = safeSpawns[currentSpawn].transform.position;

                // If there's nowhere safe to spawn, RIP player
                if (spawnBoxes.Count == 0)
                {
                    ps.myUnit.CurrentHealth = 0;
                    return;
                }

                player.transform.position = spawnBoxes[currentBox].transform.position;
                ps.cc.Velocity *= .8f;
            }

            return;
        }

        // ... Unless it's a weapon
        else if (other.gameObject.name == "ThrowMe" || other.gameObject.name == "ThrowGun")
        {
            other.gameObject.transform.position = player.transform.position;
            other.gameObject.transform.position += new Vector3(0.0f, 50.0f, 0.0f);

            other.attachedRigidbody.velocity = new Vector3(0.0f, other.attachedRigidbody.velocity.y, 0.0f);
            numWeaponTosses++;

            if (numWeaponTosses >= maxWeaponTosses)
            {
                // Fun easter egg?
                CutsceneManager.instance.StartCutscene
                (
                    "HaltPlayer()\n" +
                    "LoadChar(Con1,Console)\n" +
                    "Say(Console,Hey!)\n" +
                    "Continue(Stop throwing your weapons off cliffs!)"
                );

                numWeaponTosses = 0;
            }

            return;
        }

        // Or an enemy unit
        IDamageable d = other.GetComponent<IDamageable>();
        if (d != null)
        {
            d.MyUnit.CurrentHealth = 0;
            d.gameObject.SetActive(false);

            return;
        }
    }

	void OnTriggerExit(Collider other)
	{
        Run(other);
	}

    void OnTriggerEnter(Collider other)
    {
        Run(other);
    }
}
