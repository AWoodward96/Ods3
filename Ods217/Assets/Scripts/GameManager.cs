using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {


    public static GameManager instance; 
    public static int ScrapCount;

    public static List<Item> Inventory;
    public static int HealthKits;
 
    public int scrapcount;


    public static MetaData GameData;

    // Use this for initialization
	void Start () {
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
            WriteSaveFile(newData.FileName);
        }

        SceneManager.sceneLoaded += LevelLoaded;

        HealthKits = 3;

        Cursor.SetCursor(Resources.Load("Objects/UI/CursorTexture") as Texture2D, new Vector2(8, 8), CursorMode.Auto);

	}
	
	// Update is called once per frame
	void Update () {
        scrapcount = ScrapCount;


        if (Input.GetKeyDown(KeyCode.Alpha8))
            WriteSaveFile("Beta");

        if (Input.GetKeyDown(KeyCode.Alpha9))
            LoadSaveFile("Beta");
       
    }


    void WriteSaveFile(string _filePath)
    {
        string actualPath = Application.dataPath + '\\' + _filePath;
        if (File.Exists(actualPath))
            File.Delete(actualPath);

        using(StreamWriter writer = new StreamWriter(actualPath))
        {
            // First line should be the name of the user
            writer.WriteLine(GameData.Username);
            writer.WriteLine("--------------Plz don't edit this. You might break the game and then you're saved file is lost.--------------");
            writer.WriteLine(SceneManager.GetActiveScene().name);
            writer.WriteLine(scrapcount);
            writer.WriteLine(GameObject.FindGameObjectWithTag("Player").transform.position);
            writer.WriteLine(System.DateTime.Now);
        }
    }

    void LoadSaveFile(string _filePath)
    {
        string actualPath = Application.dataPath + '\\' + _filePath;
        if (!File.Exists(actualPath))
            return;
 
        using (StreamReader reader = new StreamReader(actualPath))
        {
            string val;
            int linenum = 0;
            while((val = reader.ReadLine()) != null)
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
                }
                linenum++;
            }

        }

        LoadLevelBasedOnMetaData();
    }

    void LoadLevelBasedOnMetaData()
    {
        SceneManager.LoadScene(GameData.LevelName);
    }

    void LevelLoaded(Scene _scene, LoadSceneMode _loadMode)
    {
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        Player.transform.position = GameData.SavedPlayerPosition;
    }
 

    public void LoadLastSaveFile()
    {
        LoadSaveFile(GameData.FileName);
    }
}

[System.Serializable]
public struct MetaData
{
    public string FileName;
    public string Username;
    public string LevelName;
    public int scrapCount;
    public Vector3 SavedPlayerPosition;
}
