using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class EffectSystem : MonoBehaviour {

    public enum EffectType { Alert, Panic }
    public EffectType myType;
    public bool Fired;

    SpriteRenderer myRenderer;
    Animator myAnimator;
     
    Vector3 originPoint;
    float timeSince;

	// Use this for initialization
	void Start () {
        myRenderer = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();
	}

    private void Update()
    {
        myRenderer.enabled = Fired;
        myAnimator.SetFloat("State", (int)myType);

        if(Fired)
        {
            timeSince += Time.deltaTime;
           

            switch (myType)
            {
                case EffectType.Alert:
                    transform.position = Vector3.Lerp(transform.position, originPoint + (Vector3.up * 2), Time.deltaTime);
                    if (timeSince > 3)
                        Fired = false;
                    break;
                case EffectType.Panic:
                    // Jitter the panic back and forth 
                    transform.position = originPoint + new Vector3((1 - (Mathf.Sin(Time.time * 20) * 2))/30,0,0);
                    if (timeSince > 3)
                        Fired = false;
                    break;
            }
        }

    }

    public void Fire(EffectType _newType, Vector3 _newPosition)
    {
        transform.position = _newPosition;
        originPoint = _newPosition;
        myType = _newType;
        timeSince = 0;
        Fired = true;
    }
}
