using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneData : MonoBehaviour {



	public List<ObjectPermanent> Permanents;

	void Awake()
	{
		LoadList();
	}
	
	// Update is called once per frame
	void Update () {
		foreach(ObjectPermanent p in Permanents)
        {
            if(p.Object != null)
                p.Name = p.Object.name;
        }
	}

	public void LoadList()
	{
		Permanents.Clear();

		// Populate the Permanents list
		GameObject[] myList = FindObjectsOfType<GameObject>();
		for(int i = 0; i < myList.Length; i++)
		{
			// If we can get the IPermanent script, then the object is an IPermanent (Thanks, Alex)
			if(myList[i].GetComponent<IPermanent>() != null)
			{
				Permanents.Add(new ObjectPermanent());
				Permanents[Permanents.Count - 1].Object = myList[i];
			}
		}
	}
}

[System.Serializable]
public class ObjectPermanent
{
    public GameObject Object;
    public string Name;
    public bool Destroyed;
    
}
