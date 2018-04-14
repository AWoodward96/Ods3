using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script that is always present in the scene
/// Used to move players around, handle the inventory, loading and saving player data, the works
/// </summary>
public class GameManager : MonoBehaviour {

    public enum SpecialMoveType { Sprint, Roll };
    public static SpecialMoveType MyType;

    public static GameManager instance; // So there's only one
    public static int ScrapCount; // How much money the player has

    public static List<Item> Inventory; // What's in you're inventory
    public static int HealthKits; // How many health kits you have [Deprecated]
 
    public int scrapcount;

    public   enum GameDifficulty { Easy, Normal, Hard }
    public static GameDifficulty curDifficulty;


    public static MetaData GameData;

    public Upgrades UpgradeData;

    public SceneData currentSceneData;

    [Header("Sounds")]
    public AudioClip SaveSound;
    AudioSource src;

    // Use this for initialization
	void Awake () {
        // There can only be one
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        Inventory = new List<Item>();
        // Make sure that the communicator item is in the inventory
        Inventory.Add((Resources.Load("Prefabs/Items/Communicator") as GameObject).GetComponent<Item>());


        if (GameData.FileName == null)
        {
            MetaData newData = new MetaData();
            newData.FileName = "BetaTest2.ods";
            newData.Username = "Author";
            GameData = newData;
            //WriteSaveFile(newData.FileName);
        }

        //SceneManager.sceneLoaded += LevelLoaded;
        ScrapCount += 200;
        HealthKits = 0;

        Physics.gravity = new Vector3(0, -GlobalConstants.Gravity * 4,0);

        Cursor.SetCursor(Resources.Load("Sprites/UI/CursorTexture") as Texture2D, new Vector2(8, 8), CursorMode.Auto);

        src = GetComponent<AudioSource>(); 
	}
	
	// Update is called once per frame
	void Update () {
        scrapcount = ScrapCount;

        // Some keystrokes to work with saving and loading files
        if (Input.GetKeyDown(KeyCode.Alpha8))
            WriteSaveFile("BetaTest2.ods");

        if (Input.GetKeyDown(KeyCode.Alpha9))
			LoadSaveFile("BetaTest2.ods");
       
		// Temp fix to see if player data transfers to a new room
		/*if(Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			PreservePlayer();
			SceneManager.LoadScene("Level_Data/lvl_Spire/Spire");
			SceneManager.sceneLoaded += RestorePlayer;
		}*/
    }

    public void LoadScene(string _Name)
    {
        SceneManager.LoadScene(_Name);
    }
		
	// For saving the player and the current scene at a save station.
    bool WriteSaveFile(string _filePath)
    {
        string actualPath = Application.dataPath + '\\' + _filePath;
        if (File.Exists(actualPath))
            File.Delete(actualPath);

        // Make sure you've got the scene data
        if (currentSceneData == null)
        {
            currentSceneData = FindObjectOfType<SceneData>();
        }

        using (StreamWriter writer = new StreamWriter(actualPath))
        {
            // First line should be the name of the user
            writer.WriteLine(GameData.Username);
            writer.WriteLine("--------------Plz don't edit this. You might break the game and then your saved file is lost.--------------");
            writer.WriteLine(SceneManager.GetActiveScene().name);
            writer.WriteLine(scrapcount);
            writer.WriteLine(GameObject.FindGameObjectWithTag("Player").transform.position);

			// And then Ed jumped in and added:
			// HP (Player.GetComponent<PlayerScript>().MyUnit.CurrentHealth)
			PlayerScript Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
			writer.WriteLine(Player.MyUnit.CurrentHealth);

			// Weapons (Player.GetComponent<PlayerScript>().PrimaryWeapon, SecondaryWeapon, ActiveWeapon
			if(Player.PrimaryWeapon != null)
			{
				if(Player.PrimaryWeapon.name.IndexOf(' ') != -1)
				{
					writer.WriteLine(Player.PrimaryWeapon.name.Substring(0, Player.PrimaryWeapon.name.IndexOf(' ')));
				}
				else
				{
					writer.WriteLine(Player.PrimaryWeapon.name);
				}
			}
			else
			{
				writer.WriteLine("null");	
			}
			if(Player.SecondaryWeapon != null)
			{
				if(Player.SecondaryWeapon.name.IndexOf(' ') != -1)
				{
					writer.WriteLine(Player.SecondaryWeapon.name.Substring(0, Player.SecondaryWeapon.name.IndexOf(' ')));
				}
				else
				{
					writer.WriteLine(Player.SecondaryWeapon.name);
				}
			}
			else
			{
				writer.WriteLine("null");
			}
			if(Player.ActiveWeapon != null)
			{
				if(Player.ActiveWeapon.name.IndexOf(' ') != -1)
				{
					writer.WriteLine(Player.ActiveWeapon.name.Substring(0, Player.ActiveWeapon.name.IndexOf(' ')));
				}
				else
				{
					writer.WriteLine(Player.ActiveWeapon.name);
				}
			}
			else
			{
				writer.WriteLine("null");
			}
				
			// First, write down which permanents are active!
			writer.WriteLine("$PHASE");
			for(int i = 0; i < currentSceneData.Permanents.Count; i++)
			{
				writer.WriteLine(currentSceneData.Permanents[i].Object.activeInHierarchy);
			}

			// Next, write which ones are triggered!
			writer.WriteLine("$PHASE");
			for(int i = 0; i < currentSceneData.Permanents.Count; i++)
			{
				writer.WriteLine(currentSceneData.Permanents[i].Object.GetComponent<IPermanent>().Triggered);
			}
				
			// Which doors locked (currentSceneData.Permanents[].GetComponent<SlidingDoor>().State)
			writer.WriteLine("$PHASE");
			SlidingDoor currentDoor;
			for(int i = 0; i < currentSceneData.Permanents.Count; i++)
			{
				currentDoor = currentSceneData.Permanents[i].Object.GetComponent<SlidingDoor>();
				if(currentDoor != null)
				{
					writer.WriteLine(currentDoor.Locked);
				}
			}

			// Where are the elevators
			writer.WriteLine("$PHASE");
			StandardElevator currentElevator;
			for(int i = 0; i < currentSceneData.Permanents.Count; i++)
			{
				currentElevator = currentSceneData.Permanents[i].Object.GetComponent<StandardElevator>();
				if(currentElevator != null)
				{
					writer.WriteLine(currentElevator.FloorIndex);
				}
			}

			writer.WriteLine("$PHASE");

			// Remaining enemies (currentSceneData.Permanents[]) :: Commented out because the active permanent check makes this obsolete ::
			/*writer.WriteLine("$PHASE");
			for(int i = 0; i < currentSceneData.Permanents.Count; i++)
			{
				if(currentSceneData.Permanents[i].Object.GetComponent<AIStandardUnit>() != null)
				{
					// I feel like this is hacky; replace this with a check if their HP is 0, or something like that
					writer.WriteLine(currentSceneData.Permanents[i].Object.activeInHierarchy);
				}
			}*/

            writer.WriteLine(System.DateTime.Now);

            if(src != null)
            {
                src.clip = SaveSound;
                src.Play();
            } 
			return true;
        }
    }

	// Preserves the player for transfer between scenes.
	void PreservePlayer()
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
	void RestorePlayer(Scene _scene, LoadSceneMode _loadMode)
	{
		GameObject Player = GameObject.FindGameObjectWithTag("Player");
		PlayerScript ps = Player.GetComponent<PlayerScript>();

		ps.myUnit.CurrentHealth = GameData.PlayerHP;

		// If the player starts out with a weapon on opening the scene, delete it...
		if(ps.PrimaryWeapon != null)
		{
			Destroy(ps.PrimaryWeapon.gameObject);
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
			Destroy(ps.PrimaryWeapon.gameObject);
		}
		if(GameData.PlayerWeapon2 != "null")
		{
			ps.SecondaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + GameData.PlayerWeapon2), Player.transform) as GameObject).GetComponent<WeaponBase>();

			ps.SecondaryWeapon.gameObject.name = GameData.PlayerWeapon2;
		}

		if(ps.ActiveWeapon != null)
		{
			Destroy(ps.PrimaryWeapon.gameObject);
		}
		if(GameData.PlayerWeaponActive != "null")
		{
			ps.ActiveWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + GameData.PlayerWeaponActive), Player.transform) as GameObject).GetComponent<WeaponBase>();

			ps.ActiveWeapon.gameObject.name = GameData.PlayerWeaponActive;
		}

		Debug.Log("Player data restored.");

		SceneManager.sceneLoaded -= RestorePlayer;
	}

	// Loading save data upon starting the game.
    void LoadSaveFile(string _filePath)
    {
        string actualPath = Application.dataPath + '\\' + _filePath;
        if (!File.Exists(actualPath))
            return;

		// Make sure you've got the scene data
		if (currentSceneData == null)
		{
			currentSceneData = FindObjectOfType<SceneData>();
		}
			
		GameData.permsActive = new List<bool>();
		GameData.permsTriggered = new List<bool>();

		GameData.doorsLocked = new List<bool>();
		GameData.elevatorIndices = new List<int>();
 
        using (StreamReader reader = new StreamReader(actualPath))
        {
            string val;
            int linenum = 0;
			int phase = 0;
            while((val = reader.ReadLine()) != null)
            {
				// Player info
				if(linenum <= 8)
				{
	                switch (linenum)
	                {
	                    case 0:
	                        GameData.Username = val;
	                        break;
	                    case 2:
	                        GameData.LevelName = val;
	                        break;
	                    case 3:
	                        int.TryParse(val, out GameData.scrapCount);
	                        ScrapCount = GameData.scrapCount;
	                        break;
	                    case 4:
	                        GameData.SavedPlayerPosition = GlobalConstants.StringToVector3(val);
	                        break;

						// Add the other things to read here, Ed!
						// Player HP
						case 5:
							int.TryParse(val, out GameData.PlayerHP);
							break;

						// Player's Primary Weapon
						case 6:
							GameData.PlayerWeapon1 = val;
							break;
						
						// Player's Secondary Weapon
						case 7:
							GameData.PlayerWeapon2 = val;
							break;

						// Player's Active Weapon
						case 8:
							GameData.PlayerWeaponActive = val;
							break;
							
	                }
				}
				// Level info
				else
				{
					if(val == "$PHASE")
					{
						phase++;
					}

					// Permanents Active
					else if(phase == 1)
					{
						GameData.permsActive.Add(val.Trim().ToUpper() == "TRUE");
					}

					// Permanents Triggered
					else if(phase == 2)
					{
						GameData.permsTriggered.Add(val.Trim().ToUpper() == "TRUE");
					}

					// Doors Locked
					else if(phase == 3)
					{
						GameData.doorsLocked.Add(val.Trim().ToUpper() == "TRUE");
					}

					// Elevator indices
					else if(phase == 4)
					{
						GameData.elevatorIndices.Add(int.Parse(val.Trim()));
					}
				}

                linenum++;
            }

        }

        LoadLevelBasedOnMetaData();
    }

    void LoadLevelBasedOnMetaData()
    {
        SceneManager.LoadScene(GameData.LevelName);
		SceneManager.sceneLoaded += LevelLoaded;
    }

    void LevelLoaded(Scene _scene, LoadSceneMode _loadMode)
    {
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
		PlayerScript ps = Player.GetComponent<PlayerScript>();
        Player.transform.position = GameData.SavedPlayerPosition;
		ps.MyUnit.CurrentHealth = GameData.PlayerHP;

		// The following 3 loads are ugly as sin. There HAS to be a neater way to do this, right?

		// If the player starts with either weapon, delete them and replace with ours
		if(ps.PrimaryWeapon != null)
		{
			Destroy(ps.PrimaryWeapon.gameObject);
			ps.PrimaryWeapon = null;
		}
		if(ps.SecondaryWeapon != null)
		{
			Destroy(ps.SecondaryWeapon.gameObject);
			ps.SecondaryWeapon = null;
		}

		ps.PrimaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + GameData.PlayerWeapon1), Player.transform) as GameObject).GetComponent<WeaponBase>();
		ps.SecondaryWeapon = (Instantiate(Resources.Load("Prefabs/Weapon/" + GameData.PlayerWeapon2), Player.transform) as GameObject).GetComponent<WeaponBase>();

		// Make sure the names are correct
		ps.PrimaryWeapon.gameObject.name = GameData.PlayerWeapon1;
		ps.SecondaryWeapon.gameObject.name = GameData.PlayerWeapon2;

        currentSceneData = FindObjectOfType<SceneData>();

		if(currentSceneData)
		{
			Debug.Log("Scene Data loaded.");
		}
		else
		{
			Debug.Log("Could not locate Scene Data Object.");
		}

		currentSceneData.LoadList();

		// Set the permanents' values
		SlidingDoor currentDoor;
		int doorCount = 0;

		StandardElevator currentElevator;
		int elevatorCount = 0;

		for(int i = 0; i < currentSceneData.Permanents.Count; i++)
		{
			if(!GameData.permsActive[i])
			{
				currentSceneData.Permanents[i].Object.SetActive(false);
				continue;
			}

			currentSceneData.Permanents[i].Object.SetActive(true);

			if(!currentSceneData.Permanents[i].Object.GetComponent<IPermanent>().Triggered == GameData.permsTriggered[i])
			{
				currentSceneData.Permanents[i].Object.GetComponent<IPermanent>().Triggered = GameData.permsTriggered[i];
			}

			// Doors
			currentDoor = currentSceneData.Permanents[i].Object.GetComponent<SlidingDoor>();
			if(currentDoor != null)
			{
				currentDoor.Locked = GameData.doorsLocked[doorCount];
				doorCount++;
				continue;
			}

			// Elevators
			currentElevator = currentSceneData.Permanents[i].Object.GetComponent<StandardElevator>();
			if(currentElevator != null)
			{
				currentElevator.FloorIndex = GameData.elevatorIndices[elevatorCount];
				currentElevator.transform.position = currentElevator.ArrivalPoints[currentElevator.FloorIndex].transform.position;
				elevatorCount++;
				continue;
			}
		}

		SceneManager.sceneLoaded -= LevelLoaded;
    }

    public void LoadLastSaveFile()
    {
        LoadSaveFile(GameData.FileName);
    }

	// I feel like I shouldn't do this
	// But I also don't know if porting the save function over to the InteractToSave script would be a good idea, so...
	public bool WriteToCurrentSave()
	{
		// NOTE: Replace "BetaTest2.ods" with whatever the current save file's name is later!
		return WriteSaveFile("BetaTest2.ods");
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
