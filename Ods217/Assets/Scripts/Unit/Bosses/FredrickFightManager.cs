using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FredrickFightManager : MonoBehaviour, IPermanent {

    public GameObject ExplosivePrefab;
    public GameObject NormalBoxPrefab;

    List<GameObject> ExplosiveBoxes = new List<GameObject>();
    List<GameObject> NormalBoxes = new List<GameObject>();
    public Vector3 StartPos; // No end position needed because these objects should just roll back into this manager;
    [Range(0,1)]
    public float ExplosiveRatio;
    public float Speed;

    public Vector3 setMove;
    public bool Enabled;

    float dTime = 0;

    BoxCollider myCol;
    ZoneScript z;

	public GameObject myBelt;

	int phase;

    public ZoneScript myZone
    {
        get
        {
            return z;
        }

        set
        {
            z = value;
        }
    }

    public bool Triggered
    {
        get
        {
            return Enabled;
        }

        set
        {
			if(Enabled != value)
			{
				phase++;
			}

			if(phase == 1)
			{
            	Enabled = value;
			}

			else if(phase == 2)
			{
				Enabled = value;

				for(int i = 0; i < ExplosiveBoxes.Count; i++)
				{
					ExplosiveBoxes[i].GetComponent<SimpleMove>().Move = Vector3.zero;
				}

				for(int i = 0; i < NormalBoxes.Count; i++)
				{
					NormalBoxes[i].GetComponent<SimpleMove>().Move = Vector3.zero;
				}

				myBelt.GetComponentInChildren<ConveyorBelt>().ToggleEnabled();
			}
        }
    }

    // Use this for initialization
    void Start () {
        myCol = GetComponent<BoxCollider>();
        myCol.isTrigger = true;

        for(int i =0; i < 8; i++)
        {
            ExplosiveBoxes.Add(Instantiate(ExplosivePrefab, transform.position, Quaternion.identity));
            ExplosiveBoxes[i].SetActive(false);
            ExplosiveBoxes[i].GetComponent<SimpleMove>().Move = setMove;
        }

        for (int i = 0; i < 8; i++)
        {
            NormalBoxes.Add(Instantiate(NormalBoxPrefab, transform.position, Quaternion.identity));
            NormalBoxes[i].SetActive(false);
            NormalBoxes[i].GetComponent<SimpleMove>().Move = setMove;
        }

		phase = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (!Enabled)
            return;

        dTime += Time.deltaTime;
        if(dTime > Speed)
        {
            float rnd = UnityEngine.Random.Range(0f, 1f);

            GameObject obj = getObjectThatCanBeUsed((rnd < ExplosiveRatio));
            if(obj != null)
            {
                obj.transform.position = StartPos;
                obj.SetActive(true); 
            }
            dTime = 0;
        }
		
	}

    GameObject getObjectThatCanBeUsed(bool _Explosive)
    { 
        if (_Explosive)
        {
            for(int i = 0; i < ExplosiveBoxes.Count; i++)
            {
                if (ExplosiveBoxes[i].activeSelf)
                    continue;
                else
                    return ExplosiveBoxes[i];
            }
        }


        for(int i =0; i < NormalBoxes.Count; i++)
        {
            if (NormalBoxes[i].activeSelf)
                continue;
            else
                return NormalBoxes[i];
        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(ExplosiveBoxes.Contains(other.gameObject))
        {
            ExplosiveBox e = other.GetComponent<ExplosiveBox>();
            e.Triggered = false;
            other.gameObject.SetActive(false);
        }

        if(NormalBoxes.Contains(other.gameObject))
        {
            ExplosiveBox e = other.GetComponent<ExplosiveBox>();
            e.Triggered = false;
            other.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(StartPos, new Vector3(1, 1, 1));
    }
}
