using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script that is always present in the scene
/// Used to move players around, handle the inventory, loading and saving player data, the works
/// </summary>
public class GameManager : MonoBehaviour {	// Pseudo-ISavable

    public enum SpecialMoveType { Sprint, Roll };
    public static SpecialMoveType MyType;

    public static GameManager instance; // So there's only one
    public static int ScrapCount; // How much money the player has


    public static List<Item> Inventory; // What's in your inventory
	public const int maxInventorySize = 8;	// How big can your inventory be?

	public enum TimeOfDay { Morning, Evening };
	public static TimeOfDay currentTimeOfDay;	// Used for keeping track of the following two bools

	public bool ateMorning;	// Did Slas eat this morning?
	public bool ateEvening;	// Did slas eat last evening?
	public int starveTimer;	// How long has slas gone without eating?

	public Consumable affectingFood;	// What food effect is currently on Slas?

	public static List<Consumable> FoodStorage;	// What food is stored in the fridge?
	public const int foodStorageSize = 8;	// How much can be stored in there?

	public static List<string> Objectives;	// What do you currently need to be doing?
    public static int HealthKits; // How many health kits you have [Deprecated]

	public static Dictionary<string, bool> Events;	// How far through the game has the player made it?

	public List<Upgrade> ownedUpgrades;		// Which upgrades do you own?
	public List<Upgrade> equippedUpgrades;		// Which are currently in use?
 
    public int scrapcount;

    public   enum GameDifficulty { Easy, Normal, Hard }
    public static GameDifficulty curDifficulty;

    public static Dictionary<string, string> World;
	public string currentLocation = "null";

    public static MetaData GameData;

    public Upgrades UpgradeData;

    public SceneData currentSceneData;

	bool hardLoad = false;	// Are we loading from the persistent data path? Used almost exclusively for LevelLoaded

    [Header("Sounds")]
    public AudioClip SaveSound;
    AudioSource src;

    // Use this for initialization
	void Awake() {
        // There can only be one
        if (instance == null)
            instance = this;
        else if (instance != this)
		{
            Destroy(this.gameObject);
			return;
		}

        DontDestroyOnLoad(this.gameObject);

        Inventory = new List<Item>();
        // Make sure that the communicator item is in the inventory
        Inventory.Add((Resources.Load("Prefabs/Items/Communicator") as GameObject).GetComponent<Item>());

		FoodStorage = new List<Consumable>();

		Objectives = new List<string>();  
		Events = new Dictionary<string, bool>();

		// Put everything in the Events dictionary here!
		Events.Add("OUTPOST UNLOCKED", true);
		Events.Add("PIRATE SHIP UNLOCKED", false);
		Events.Add("THE SKYBORN SPIRIT UNLOCKED", false);
		Events.Add("LEVEL 3 UNLOCKED", false);
		Events.Add("LEVEL 4 UNLOCKED", false);

        Events.Add("ARMED FOR LEVEL 1", false);

		Events.Add("USED BED HOLDING FOOD", false);

		ownedUpgrades = new List<Upgrade>();
		equippedUpgrades = new List<Upgrade>();

        if (GameData.FileName == null)
        {
            MetaData newData = new MetaData();
            newData.FileName = "Save1";
            newData.Username = "Author";
            GameData = newData;
            //WriteSaveFile(newData.FileName);
        }

        // Set up the world links
        World = new Dictionary<string, string>();
        World.Add("SPIRE", "SpireC0-1");
        World.Add("OUTPOST", "OutpostC0-3");
        World.Add("PIRATE", "PirateC0-5");

		UpdateLocation(SceneManager.GetActiveScene().name);

        //SceneManager.sceneLoaded += LevelLoaded;
        //ScrapCount += 100;
        HealthKits = 0;

        Physics.gravity = new Vector3(0, -GlobalConstants.Gravity * 4,0);

        Cursor.SetCursor(Resources.Load("Sprites/UI/CursorTexture") as Texture2D, new Vector2(8, 8), CursorMode.Auto);

        src = GetComponent<AudioSource>();

		currentTimeOfDay = TimeOfDay.Morning;

		ateMorning = false;
		ateEvening = false;
		starveTimer = 0;

		// Clear the cache if there's anything left in it somehow, and set the cache up to be cleared at quit!
		ClearCache();
		Application.quitting += ClearCache;
	}
	
	// Update is called once per frame
	void Update() {
        scrapcount = ScrapCount;

        // Some keystrokes to work with saving and loading files
        if (Input.GetKeyDown(KeyCode.Alpha8))
			WriteSaveFile();

        if (Input.GetKeyDown(KeyCode.Alpha9))
			LoadSaveFile(true);
       
	
        if(Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadScene("Title");
        }

        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha1))
            SceneManager.LoadScene("TutorialC0-0");


        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha2))
            SceneManager.LoadScene("SpireC0-1");

        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha3))
            SceneManager.LoadScene("OutpostC0-3");

        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha4))
            SceneManager.LoadScene("PirateC0-5");

        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha5))
            SceneManager.LoadScene("OutpostC0-6");

        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha6))
            SceneManager.LoadScene("SpireC0-8");
    }

	// Overriden LoadScene that will load the scene based on its save data
    public void LoadScene(string _Name)
    {
		GameData.LevelName = _Name;

		// Update current location
		UpdateLocation(_Name);

		// There is no reason that this function should require a hard load
		hardLoad = false;

		LoadLevelBasedOnMetaData();
    }
		
	// For saving the player and the current scene at a save station.
    public bool WriteSaveFile()
    {
		// Firstly, before we do anything, make sure the current stage is cached!
		WriteCache();

		// TODO: Replace this variable with an actual way to track the player's save slot when it's time!
		int fileSlot = 1;

		string actualPath = Application.persistentDataPath + "\\Saves\\Save" + fileSlot + '\\';
		if(!Directory.Exists(actualPath))
		{
			Directory.CreateDirectory(actualPath);
		}

		// Copy the contents of the cache directory over to the persistent directory!
		string[] files = Directory.GetFiles(Application.temporaryCachePath + "\\Save\\");
		for(int i = 0; i < files.Length; i++)
		{
			FileInfo current = new FileInfo(files[i]);
			current.CopyTo(actualPath + current.Name, true);
		}

        if(src != null)
        {
            src.clip = SaveSound;
            src.Play();
        } 
		return true;
    }

	// Preserves the player for transfer between scenes.
	public void PreservePlayer()
	{
		PlayerScript Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

		GameData.PlayerHP = Player.myUnit.CurrentHealth;

		if(Player.PrimaryWeapon != null)
		{
			GameData.PlayerWeapon1 = Player.PrimaryWeapon.name;
		}
		else
		{
			GameData.PlayerWeapon1 = "null";	
		}
		if(Player.SecondaryWeapon != null)
		{
			GameData.PlayerWeapon2 = Player.SecondaryWeapon.name;
		}
		else
		{
			GameData.PlayerWeapon2 = "null";
		}
		if(Player.ActiveWeapon != null)
		{
			GameData.PlayerWeaponActive = Player.ActiveWeapon.name;
		}
		else
		{
			GameData.PlayerWeaponActive = "null";
		}

		Debug.Log("Player data preserved.");
	}

	// Restores the player upon their entry into a new scene.
	// To use this function, do SceneManager.sceneLoaded += RestorePlayer; after loading a new scene.
	// It will only work properly if PreservePlayer() is done first.
	public void RestorePlayer(Scene _scene, LoadSceneMode _loadMode)
	{
		GameObject Player = GameObject.FindGameObjectWithTag("Player");
		PlayerScript ps = Player.GetComponent<PlayerScript>();

		ps.myUnit.CurrentHealth = GameData.PlayerHP;

		// If the player starts out with a weapon on opening the scene, delete it...
		if(ps.PrimaryWeapon != null)
		{
			Destroy(ps.PrimaryWeapon.gameObject);
			ps.PrimaryWeapon = null;
		}
		if(GameData.PlayerWeapon1 != "null")
		{
			// ... Then replace it with the weapon we saved (if any).
			ps.PrimaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + GameData.PlayerWeapon1), Player.transform) as GameObject).GetComponent<WeaponBase>();

			// Hacky failsafing because sometimes the weapon gets named with a (clone) suffix, which throws a monkey wrench in my plans
			ps.PrimaryWeapon.gameObject.name = GameData.PlayerWeapon1;
		}

		// Do the same for secondary and active weapons
		if(ps.SecondaryWeapon != null)
		{
			Destroy(ps.SecondaryWeapon.gameObject);
			ps.SecondaryWeapon = null;
		}
		if(GameData.PlayerWeapon2 != "null")
		{
			ps.SecondaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + GameData.PlayerWeapon2), Player.transform) as GameObject).GetComponent<WeaponBase>();

			ps.SecondaryWeapon.gameObject.name = GameData.PlayerWeapon2;
		}

		if(GameData.PlayerWeaponActive != "null")
		{
			if(GameData.PlayerWeaponActive == ps.PrimaryWeapon.name)
			{
				ps.ActiveWeapon = ps.PrimaryWeapon;
			}
			else
			{
				ps.ActiveWeapon = ps.SecondaryWeapon;
			}
		}

		Debug.Log("Player data restored.");

		SceneManager.sceneLoaded -= RestorePlayer;
	}

	// Loading save data upon starting the game.
	public void LoadSaveFile(bool _hardLoad)
    {
		hardLoad = _hardLoad;

		string actualPath = "";

		// TODO: Replace this variable with an actual way to track the player's save slot when it's time!
		int fileSlot = 1;

		// Are we loading from the persistent file, or from the cache?
		if(hardLoad)
		{
			actualPath = Application.persistentDataPath + "\\Saves\\Save" + fileSlot + '\\';
		}
		else
		{
			actualPath = Application.temporaryCachePath + "\\Save\\";
		}

		if(!Directory.Exists(actualPath))
		{
			Directory.CreateDirectory(actualPath);
		}

		// If master.dat doesn't exist, that makes it *highly* unlikely that anything else in the cache folder does either.
		// Therefore, we should just hard load.
        if (!File.Exists(actualPath + "MASTER.dat"))
		{
			if(!hardLoad)
			{
				Debug.Log("MASTER.dat does not exist in the cache, so it cannot be loaded. Attempting to Hard Load.");

				hardLoad = true;
				actualPath = Application.persistentDataPath + "\\Saves\\Save" + fileSlot + '\\';

				if(!File.Exists(actualPath + "MASTER.dat"))
				{
					Debug.Log("Hard Load failed. Defaulting to resetting the scene.");
					SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					return;
				}
			}
			else
			{
				Debug.Log("MASTER.dat does not exist in the save data, so it cannot be loaded. Defaulting to resetting the scene.");
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				return;
			}
		}

		StreamReader reader = new StreamReader(actualPath + "MASTER.dat");
		List<string> data = new List<string>();
		while(!reader.EndOfStream)
		{
			data.Add(reader.ReadLine());
		}

		reader.Close();

		GameData.LevelName = data[2].Trim();

		string[] gmData = new string[data.Count - 3];
		for(int i = 3; i < data.Count; i++)
		{
			gmData[i - 3] += data[i];
		}

		LoadGameManager(gmData);
		
        LoadLevelBasedOnMetaData();
    }

    void LoadLevelBasedOnMetaData()
    {
        SceneManager.LoadScene(GameData.LevelName);
		SceneManager.sceneLoaded += LevelLoaded;
    }

    void LevelLoaded(Scene _scene, LoadSceneMode _loadMode)
    {
		Camera.main.GetComponent<CamScript>().FadeIn(1.0f);

		// TODO: Replace this variable with an actual way to track the player's save slot when it's time!
		int fileSlot = 1;

		string actualPath = "";

		// Are we loading from the persistent file, or from the cache?
		if(hardLoad)
		{
			actualPath = Application.persistentDataPath + "\\Saves\\Save" + fileSlot + '\\';
		}
		else
		{
			actualPath = Application.temporaryCachePath + "\\Save\\";
		}

		if(!Directory.Exists(actualPath))
		{
			Directory.CreateDirectory(actualPath);
		}

		if (!File.Exists(actualPath + currentLocation + ".dat"))
		{
			if(!hardLoad)
			{
				Debug.Log(currentLocation + ".dat does not exist in the cache, so it cannot be loaded. Attempting to Hard Load.");

				hardLoad = true;
				actualPath = Application.persistentDataPath + "\\Saves\\Save" + fileSlot + '\\';

				if(!File.Exists(actualPath + currentLocation + ".dat"))
				{
					Debug.Log("Hard Load failed. Defaulting to default scene.");
					SceneManager.sceneLoaded -= LevelLoaded;
					return;
				}
			}
			else
			{
				Debug.Log(currentLocation + ".dat does not exist in the save data, so it cannot be loaded.");
				SceneManager.sceneLoaded -= LevelLoaded;
				return;
			}
		}

		// Make sure you've got the scene data
		if (currentSceneData == null)
		{
			currentSceneData = FindObjectOfType<SceneData>();
			
			// If you can't get the current scene data, throw an error!
			if(currentSceneData == null)
			{
				Debug.LogError("No scene data found in the scene! Place an object with the SceneData script attached before running!");
				SceneManager.sceneLoaded -= LevelLoaded;
				return;
			}
		}

		currentSceneData.LoadList();

		StreamReader reader = new StreamReader(actualPath + currentLocation + ".dat");

		// Get the file header
		string[] header;
		header = reader.ReadToEnd().Split(new string[] { "$HEADERFILE\r\n" }, System.StringSplitOptions.None);
		header[0] = header[0].Trim();

		// Get the meat of the data itself
		string[] data = header[1].Split(new string[] { "$END\r\n" }, System.StringSplitOptions.None);
		reader.Close();

		// Now, load the data into each object!
		for(int i = 0; i < currentSceneData.Savables.Count && i < data.Length; i++)
		{
			// Get the id of the current object
			string[] objHeader = data[i].Split(new string[] { "$HEADEROBJECT\r\n" }, System.StringSplitOptions.None);

			// If for any reason objHeader's length is less than 2, that means that we're not currently analyzing an object because it didn't write its ID. We're done here.
			if(objHeader.Length < 2)
			{
				break;
			}

			int id = int.Parse(objHeader[0].Trim());
			data[i] = objHeader[1];

			GameObject mySavable = currentSceneData.Savables[i];

			// If the plot has progressed since last time we were here we need to account for that in loading the data
			if(_scene.name != header[0])
			{
				// If the current data has an ID of -1, ignore it.
				if(id == -1)
				{
					continue;
				}

				// Otherwise, check to see if by pure chance the objects we're comparing have the same id
				// If not, we need to find the object with the same id
				else if(id != mySavable.GetComponent<ISavable>().SaveID)
				{
					mySavable = currentSceneData.Savables.Find
					(
						delegate(GameObject obj)
						{
							return obj.GetComponent<ISavable>().SaveID == id;
						}
					);
				}
			}

			// If mySavable is null, that means we couldn't find a match. Assume the object doesn't exist in this iteration of the scene and continue
			if(mySavable == null)
			{
				continue;
			}

			ISavable[] temp = mySavable.GetComponents<ISavable>();

			for(int j = 0; j < temp.Length; j++)
			{
				string[] scrData = data[i].Split(new string[] { "$SCRIPT\r\n" }, System.StringSplitOptions.None);

				temp[j].Load(scrData[j].Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries));
			}
		}

		// And now that everything is loaded, load the player's upgrades!
		for(int i = 0; i < equippedUpgrades.Count; i++)
		{
			equippedUpgrades[i].Apply();
		}

		// TODO: This is dumb.
		// And do some shenanigans to apply the food's effect, if there is one
		if(affectingFood != null)
		{
			Consumable tempFood = affectingFood;
			affectingFood = null;
			tempFood.ApplyEffect();
		}

		SceneManager.sceneLoaded -= LevelLoaded;
    }

	// Returns a string containing a properly formatted master.dat file
	string GetMasterData()
	{
		StringWriter writer = new StringWriter();

		// First line should be the name of the user
		writer.WriteLine(GameData.Username);
		writer.WriteLine("--------------Plz don't edit this. You might break the game and then your saved file is lost.--------------");
		writer.WriteLine(SceneManager.GetActiveScene().name.Trim());
		writer.Write(SaveGameManager());

		return writer.ToString();
	}

	public void WriteCache()
	{
		string actualPath = Application.temporaryCachePath + "\\Save\\";
		if(!Directory.Exists(actualPath))
		{
			Directory.CreateDirectory(actualPath);
		}

		// Regardless of everything else, we're saving the master.dat file
		if (File.Exists(actualPath + "MASTER.dat"))
		{
			File.Delete(actualPath + "MASTER.dat");
		}

		StreamWriter writer = new StreamWriter(actualPath + "MASTER.dat");
		writer.Write(GetMasterData());
		writer.Close();

		if(!World.ContainsKey(currentLocation))
		{
			Debug.Log("Our current location isn't in the World dictionary. Scene state will not be cached.");
			return;
		}

		// Make sure you've got the scene data
		if (currentSceneData == null)
		{
			currentSceneData = FindObjectOfType<SceneData>();

			// If you can't get the current scene data, throw an error!
			if(currentSceneData == null)
			{
				Debug.LogError("No scene data found in the scene! Place an object with the SceneData script attached before running!");
				return ;
			}
		}

		string currentScene = SceneManager.GetActiveScene().name.Trim();

		if (File.Exists(actualPath + currentLocation + ".dat"))
		{
			File.Delete(actualPath + currentLocation + ".dat");
		}

		writer = new StreamWriter(actualPath + currentLocation + ".dat");

		// First thing written should be the name of the scene, so the save system can keep track of whether the plot's progressed
		writer.WriteLine(currentScene);
		writer.WriteLine("$HEADERFILE");

		// Now, save the data from each object!
		for(int i = 0; i < currentSceneData.Savables.Count; i++)
		{
			writer.WriteLine(currentSceneData.Savables[i].GetComponent<ISavable>().SaveID);
			writer.WriteLine("$HEADEROBJECT");

			ISavable[] temp = currentSceneData.Savables[i].GetComponents<ISavable>();

			for(int j = 0; j < temp.Length; j++)
			{
				writer.Write(temp[j].Save());

				// Signifies the end of this script's data
				writer.WriteLine("$SCRIPT");
			}

			// Signifies the end of this object's data
			// "$" is to signify that this line is for the parser, and isn't data
			writer.WriteLine("$END");
		}

		writer.WriteLine(System.DateTime.Now);

		writer.Close();

		/*if(src != null)
		{
			src.clip = SaveSound;
			src.Play();
		} */

		return;
	}

	// Clear the save cache. Called on quit, and startup
	void ClearCache()
	{
		// If the temporary cache directory doesn't exist, there's nothing to clear!
		if(!Directory.Exists(Application.temporaryCachePath + "\\Save\\"))
		{
			return;
		}

		string[] files = Directory.GetFiles(Application.temporaryCachePath + "\\Save\\");
		for(int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
	}

	public void EquipUpgrade(Upgrade myUpgrade)
	{
		for(int i = 0; i < ownedUpgrades.Count; i++)
		{
			if(ownedUpgrades[i] == myUpgrade)
			{
				equippedUpgrades.Add(ownedUpgrades[i]);
				equippedUpgrades[equippedUpgrades.Count - 1].Apply();
			}
		}
	}

	public void ChangeTimeOfDay(string newTime)
	{
		if(newTime.ToUpper() == "MORNING")
		{
			// Spoil any food that Slas had on him
			Item spoiledTemplate = (Resources.Load("Prefabs/Items/SpoiledFood") as GameObject).GetComponent<Item>();
			for(int i = 0; i < Inventory.Count; i++)
			{
				if(Inventory[i].GetComponent<Consumable>() != null)
				{
					// If the player hasn't tried going to bed with food on them before, warn them first!
					if(Events.ContainsKey("USED BED HOLDING FOOD") && !Events["USED BED HOLDING FOOD"])
					{
						CutsceneManager.instance.ResetCutscene();
						CutsceneManager.instance.StartCutscene(
							"Start()\n" +
							"Say(Slas,Wait.,Concerned)\n" +
							"Continue(I should put this food away before I go to bed or it might spoil.,Concerned)\n" +
							"End()"
						);

						Events["USED BED HOLDING FOOD"] = true;

						return;
					}
					else
					{
						Inventory[i] = spoiledTemplate;
					}
				}
			}

			currentTimeOfDay = TimeOfDay.Morning;
			Debug.Log("It's now morning");
			ateMorning = false;

			if(!ateEvening)
			{
				starveTimer++;

				if(affectingFood != null)
				{
					affectingFood.WearOff();
				}
			}
		}
		else if(newTime.ToUpper() == "EVENING")
		{
			currentTimeOfDay = TimeOfDay.Evening;
			Debug.Log("It's now evening");
			ateEvening = false;

			if(!ateMorning)
			{
				starveTimer++;

				if(affectingFood != null)
				{
					affectingFood.WearOff();
				}
			}
		}
		else
		{
			Debug.Log("Invalid time of day");
		}
	}

	public void UpdateLocation(string _Name)
	{
		// If our new scene exists in the World dictionary, update our current location!
		if(World.ContainsValue(_Name))
		{
			foreach(string s in World.Keys)
			{
				if(World[s] == _Name)
				{
					currentLocation = s.ToUpper();
					Debug.Log("Location updated.");
					break;
				}
			}
		}
		else
		{
			Debug.Log("World dictionary doesn't contain '" + _Name + "'. Location Update Failed.");
		}
	}

	public string SaveGameManager()
	{
		StringWriter data = new StringWriter();

		// BEGIN CONSTANTS //

		// Scrap
		data.WriteLine(ScrapCount);

		// Save Time of Day
		data.WriteLine((int)currentTimeOfDay);

		// Save when we've eaten
		data.WriteLine(ateMorning);
		data.WriteLine(ateEvening);
		data.WriteLine(starveTimer);

		// Save the food item that's currently affecting us
		if(affectingFood != null)
		{
			data.WriteLine(affectingFood.gameObject.name);
		}
		else
		{
			data.WriteLine("null");
		}

		data.WriteLine(currentLocation);

		// END CONSTANTS //

		// BEGIN LOOPS //

		// Inventory
		// We don't have to write a start line because this is the first loop, so we can assume it starts after our last constant
		for(int i = 0; i < Inventory.Count; i++)
		{
			data.WriteLine(Inventory[i].name);
		}

		// Objectives
		// TODO: We need a better objectives system, preferably a class with an ID and/or a name attached
		data.WriteLine("$ObjectivesStart");
		for(int i = 0; i < Objectives.Count; i++)
		{
			data.WriteLine(Objectives[i]);
		}

		// Owned Upgrades
		data.WriteLine("$OwnedUpgradesStart");
		for(int i = 0; i < ownedUpgrades.Count; i++)
		{
			data.WriteLine(ownedUpgrades[i].gameObject.name);
		}

		// Equipped Upgrades
		data.WriteLine("$EquippedUpgradesStart");
		for(int i = 0; i < equippedUpgrades.Count; i++)
		{
			data.WriteLine(equippedUpgrades[i].gameObject.name);
		} 

		// Events
		data.WriteLine("$EventsStart");
		string[] myKeys = new string[Events.Count];
		Events.Keys.CopyTo(myKeys, 0);

		bool[] myValues = new bool[Events.Count];
		Events.Values.CopyTo(myValues, 0);

		for(int i = 0; i < myKeys.Length; i++)
		{
			data.WriteLine(myKeys[i] + ", " + myValues[i].ToString());
		}


        // Save world
        data.WriteLine("$WorldDataStart");
        myKeys = new string[World.Count];
        World.Keys.CopyTo(myKeys, 0);

        string[] strMyValues = new string[World.Count];
        World.Values.CopyTo(strMyValues, 0);
        for(int i = 0; i < myKeys.Length; i ++)
        {
            data.WriteLine(myKeys[i] + ", " + strMyValues[i]);
        }

		// Save Stored Food
		data.WriteLine("$FoodStorageStart");
		for(int i = 0; i < FoodStorage.Count; i++)
		{
			data.WriteLine(FoodStorage[i].gameObject.name);
		}

		// END LOOPS //

        return data.ToString();
	}

	public void LoadGameManager(string[] data)
	{
		// Clear everything
		int phase = 0;

		Inventory.Clear();
		Objectives.Clear();
		ownedUpgrades.Clear();
		equippedUpgrades.Clear();
		Events.Clear();
        World.Clear();
		FoodStorage.Clear();

		affectingFood = null;

		// BEGIN CONSTANTS //

		ScrapCount = int.Parse(data[0]);
		currentTimeOfDay = (TimeOfDay)int.Parse(data[1].Trim());
			
		ateMorning = bool.Parse(data[2]);
		ateEvening = bool.Parse(data[3]);
		starveTimer = int.Parse(data[4]);

		if(data[5].ToUpper() != "NULL")
		{
			(Resources.Load("Prefabs/Items/" + data[5]) as GameObject).GetComponent<Consumable>().ApplyEffect();
		}

		currentLocation = data[6].Trim();

		// END CONSTANTS //

		phase++;

		// BEGIN LOOPS //
		for(int i = 7; i < data.Length; i++)
		{
            // Inventory
            if (phase == 1)
            {
                if (data[i] == "$ObjectivesStart")
                {
                    phase++;
                }
                else
                {
                    Inventory.Add((Resources.Load("Prefabs/Items/" + data[i]) as GameObject).GetComponent<Item>());
                }
            }

            // Objectives
            else if (phase == 2)
            {
                if (data[i] == "$OwnedUpgradesStart")
                {
                    phase++;
                }
                else
                {
                    Objectives.Add(data[i]);
                }
            }

            // Owned Upgrades
            else if (phase == 3)
            {
                if (data[i] == "$EquippedUpgradesStart")
                {
                    phase++;
                }
                else
                {
                    ownedUpgrades.Add((Resources.Load("Prefabs/Upgrades/" + data[i]) as GameObject).GetComponent<Upgrade>());
                }
            }

            // Equipped Upgrades
            else if (phase == 4)
            {
                if (data[i] == "$EventsStart")
                {
                    phase++;
                }
                else
                {
                    equippedUpgrades.Add((Resources.Load("Prefabs/Upgrades/" + data[i]) as GameObject).GetComponent<Upgrade>());
                }
            }

            // Events
            else if (phase == 5)
            {
                if (data[i] == "$WorldDataStart")
                {
                    phase++;
                }
                else
                { 
                    string[] myString = data[i].Split(new string[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries);
                    Events.Add(myString[0], bool.Parse(myString[1]));
                }
			}

			// World
            else if (phase == 6)
            {
				if(data[i] == "$FoodStorageStart")
				{
					phase++;
				}
				else
				{
                	string[] myString = data[i].Split(new string[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries);
                	World.Add(myString[0], myString[1]);
				}
            }

			// Food Storage
			else if(phase == 7)
			{
				FoodStorage.Add((Resources.Load("Prefabs/Items/" + data[i]) as GameObject).GetComponent<Consumable>());
			}
		}

		// END LOOPS //
	}
}

// A struct to hold relavent information to save to a text file
[System.Serializable]
public struct MetaData
{
    public string FileName;
    public string Username;
    public string LevelName;
    public int scrapCount;

    public Vector3 SavedPlayerPosition;
	public int PlayerHP;
	public string PlayerWeapon1;
	public string PlayerWeapon2;
	public string PlayerWeaponActive;

	public List<bool> permsActive;
	public List<bool> permsTriggered;

	public List<bool> doorsLocked;
	public List<int> elevatorIndices;
}

[System.Serializable]
public class Upgrades
{
    [Header("Weapon")]
    public bool[] FireRate = new bool[3];
    public bool[] ReloadSpeed = new bool[3];
    public bool[] ClipSize = new bool[3];

    public enum bulletUpgradeType { None, Explosive, Piercing, Bouncy };
    public bulletUpgradeType UpgradeType;



    [Header("Shield")]
    public bool HasShield;
    public bool[] ShieldHealth = new bool[3];
    public bool[] ShieldRegen = new bool[3];
}