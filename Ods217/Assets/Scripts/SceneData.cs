using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneData : MonoBehaviour {



    public ObjectPermanent[] Permanents;

 
	
	// Update is called once per frame
	void Update () {
		foreach(ObjectPermanent p in Permanents)
        {
            if(p.Object != null)
                p.Name = p.Object.name;
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
