using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class SceneData : MonoBehaviour {



	public List<ObjectPermanent> Permanents;
	public List<GameObject> Savables;

	void Awake()
	{
		if(!SceneManager.GetActiveScene().isLoaded)
		{
			SceneManager.sceneLoaded += eventWrapper;
		}
		else
		{
			LoadList();
		}
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
		Savables.Clear();

		List<GameObject> myList = new List<GameObject>();

		SceneManager.GetActiveScene().GetRootGameObjects(myList);

		for(int i = 0; i < myList.Count; i++)
		{
			for(int j = 0; j < myList[i].transform.childCount; j++)
			{
				myList.Add(myList[i].transform.GetChild(j).gameObject);
			}
		}

		// Populate the Permanents list
		for(int i = 0; i < myList.Count; i++)
		{
			// If we can get the IPermanent script, then the object is an IPermanent (Thanks, Alex)
			if(myList[i].GetComponent<IPermanent>() != null)
			{
				Permanents.Add(new ObjectPermanent());
				Permanents[Permanents.Count - 1].Object = myList[i];
			}
				
			if(myList[i].GetComponent<ISavable>() != null)
			{
				Savables.Add(myList[i]);

				// We need to make sure that all ISavables in this object have the same ID!
				ISavable[] temp = myList[i].GetComponents<ISavable>();
				for(int j = 0; j < temp.Length; j++)
				{
					// If we've already set the SaveIDs, we don't need to be here
					if(temp[j].SaveIDSet)
					{
						break;
					}

					if(temp[j].SaveID >= 0)
					{
						for(int k = 0; k < temp.Length; k++)
						{
							temp[k].SaveID = temp[j].SaveID;
							temp[k].SaveIDSet = true;
						}
						break;
					}
				}
			}
		}

		// Super cool anonymous function syntax!!!
		// TODO: Having to do this *might* increase load time by a bit, when it comes time for optimization.
		// If there's a way to make comparing instanceIDs guaranteed, do that!
		Savables.Sort
		(
			delegate(GameObject x, GameObject y)
			{
				// Compare letters, if they're the same, compare the next letter until we're out of letters for one of the names
				for(int i = 0; i < x.name.Length && i < y.name.Length; i++)
				{
					if(x.name.ToUpper()[i] < y.name.ToUpper()[i])
					{
						return -1;
					}
					else if(x.name.ToUpper()[i] > y.name.ToUpper()[i])
					{
						return 1;
					}
				}

				return 0;
			}
		);
	}

	void eventWrapper(Scene scene, LoadSceneMode mode)
	{
		LoadList();
		SceneManager.sceneLoaded -= eventWrapper;
	}
}

[System.Serializable]
public class ObjectPermanent
{
    public GameObject Object;
    public string Name;
    public bool Destroyed;
    
}
