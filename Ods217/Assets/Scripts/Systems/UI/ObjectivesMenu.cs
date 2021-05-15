using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A Menu Class
/// Used on the [ESC] item menu
/// Shows and updates your current objectives
/// </summary>
public class ObjectivesMenu : MonoBehaviour
{
	public static ObjectivesMenu instance;

	List<string> theObjectives;
	List<Text> theOutputs;

	// Use this for initialization
	void Awake()
	{
		// Ensure there's only one instance of this script
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);

		// Get the objectives from the game manager
		theObjectives = GameManager.Objectives;


		// Get all of the images in the description
		Text[] txtArr = GetComponentsInChildren<Text>();
		theOutputs = new List<Text>();
		for(int i = 0; i < 3; i++)
		{
			theOutputs.Add(txtArr[i]);
		}
	}


	void Update()
	{

	}

	public void UpdateObjectiveMenu()
	{
		// go through and update all the objectives
		theObjectives = GameManager.Objectives;
		for (int i = 0; i < theOutputs.Count; i++)
		{
			if(i < theObjectives.Count)
			{
				theOutputs[i].text = theObjectives[i];
			}
			else
			{
				theOutputs[i].text = "";
			}
		}
	}
}
