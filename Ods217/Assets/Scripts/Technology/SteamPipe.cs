using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamPipe : MonoBehaviour
{
	public float cycleLength;	// How long is the period between steam bursts? In seconds.
	public float startTime = 0.0f;	// Where in the cycle do we start? Modifies globalTimer. Shouldn't be > cycleLength.
	public float ventLength = 1.0f;	// How long does the venting last for?

	public int damage;	// How much damage should the steam do?
	public float stunTime;	// When hit, how long should the player be stunned for?
	
	bool didDamage;	// Has the steam hit anything this cycle?

	static bool updatedTimer = false;	// Have we updated the timer this frame?
	static float globalTimer = 0.0f;	// Global timer, because WaitForSeconds makes everything fall out of sync
	float localTimer = 0.0f;

	public Vector3 direction;	// Where does the steam go, and how far?

	ParticleSystem steamGFX;	// The steam itself
	CapsuleCollider hitcapsule;	// The thing that makes the steam hurt
	AudioSource myAudio;		// The thing that makes the steam sound

	public bool DEBUG = false;

	void Start()
	{
		/*steamGFX = GetComponent<LineRenderer>();
		steamGFX.positionCount = 2;
		steamGFX.SetPosition(0, transform.position);
		steamGFX.SetPosition(1, transform.position + transform.rotation * direction);
		steamGFX.enabled = false;*/

		steamGFX = GetComponent<ParticleSystem>();

		ParticleSystem.MainModule main = steamGFX.main;
		main.startLifetime = direction.magnitude / 4.0f;

		ParticleSystem.ShapeModule shape = steamGFX.shape;
		shape.rotation = new Vector3(0.0f, Vector3.Angle(transform.right, transform.rotation * direction.normalized) + 90.0f, 0.0f);

		steamGFX.Stop();

		hitcapsule = GetComponent<CapsuleCollider>();
		hitcapsule.radius = 0.5f;
		hitcapsule.height = direction.magnitude;
		hitcapsule.direction = 0;
		hitcapsule.center = direction / 2.0f;

		myAudio = GetComponent<AudioSource>();
		myAudio.Stop();

		if(startTime > cycleLength)
		{
			startTime = cycleLength;
			Debug.Log("startTime for " + name + " was set to be greater than its cycleLength. Clamped to make sense.");
		}

		didDamage = false;
	}

	void Update()
	{
		// To make sure only one instance affects the timer
		if(!updatedTimer)
		{
			globalTimer += Time.deltaTime;
			updatedTimer = true;
		}
		localTimer = (globalTimer + startTime) % (cycleLength + ventLength);

		// Check the timer!
		if(localTimer < cycleLength)
		{
			if(steamGFX.isEmitting)
			{
				steamGFX.Stop();
				myAudio.Stop();
			}
			didDamage = false;
		}
		else if(!steamGFX.isEmitting)
		{
			steamGFX.Play();
			myAudio.Play();
		}
	}

	void LateUpdate()
	{
		updatedTimer = false;
	}

	// On collision with the zap
	void OnTriggerStay(Collider other)
	{
		if(!steamGFX.isEmitting || didDamage)
		{
			return;
		}

		// If the other collider can be shocked, shock it!
		IDamageable dmg = other.GetComponent<IDamageable>();
		if(dmg != null)
		{
			dmg.OnHit(damage);
			didDamage = true;
		}

		// If it's a player, stun it!
		PlayerScript player = other.GetComponent<PlayerScript>();
		if(player != null)
		{
			player.cc.ApplyForce(-transform.forward * 50.0f);
			player.Stun(stunTime, true);
		}
	}

	void OnDrawGizmos()
	{
		if(!DEBUG)
		{
			return;
		}

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 0.5f);

		Gizmos.DrawSphere(transform.position + transform.rotation * direction, 0.5f);
	}
}
