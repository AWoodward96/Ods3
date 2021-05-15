using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interface that should be attached to any object that needs to be loaded and/or saved
public interface ISavable
{
	// This method should return a string containing all variables in a format parsable by Load.
	string Save();

	// Given a parsed array of strings containing data, this method should assign the data to the object accordingly.
	void Load(string[] _data);

	// ID to track the object between scene iterations (between SpireC0-1 and SpireC0-8, for example).
	// Will not track across locations. I.E: Data from SpireC0-1 with ID 0 will not get transferred to an object in OutpostC0-3 with ID 0.
	// -1 is the default; It means the object *WILL NOT* be tracked between scene iterations, though will still be saved for this iteration only. Used for things like one-shot cutscenes.
	// Anything >= 0 means that the parser will look for an object in the new scene with a matching ID, and assign the saved data to it.
	// Having multiple objects in one scene with the same ID (>= 0) will cause unintended effects! The parser wouldn't know which object to give the data to!
	// If an object doesn't find something with a matching ID in the new scene, its data won't be applied to anything, and *WILL* be erased on save
	// (If this would cause an issue, I can retool the system, just let me know)
	int SaveID
	{
		get;
		set;
	}

	// Since every ISavable script on an object will have a SaveID, they will all be synced when loaded by sceneData.
	// This exists to check if they've been synced, so that other ISavable scripts on the same object don't go through syncing again
	bool SaveIDSet
	{
		get;
		set;
	}
}