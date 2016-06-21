using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class levelManager : MonoBehaviour {

    private int levelNumber;//Index for current level to load

    private level[] levels;//Array of levels loaded from file
    private List<save> saves;//Array of saves loaded from file

    private string levelFilePath;
    private string saveFilePath;

    //Property to return current level that should be loaded
    public level CurrentLevel { get { return levels[levelNumber]; }
    }
    public int CurrentHighscore
    {
        get { return saves[levelNumber].highscore; }
        set
        {
            saves[levelNumber].highscore = value;
            
            if(saves[levelNumber].highscore <= levels[levelNumber].par)
                saves[levelNumber].beatPar = true;
        }
    }
    public bool BeatCurrentPar { get { return saves[levelNumber].beatPar; } }

    // Use this for initialization
    void Awake () {
        DontDestroyOnLoad(gameObject);//Don't destroy this gameobject between scenes, it will be used between scenes for loading levels

        levelFilePath = Application.persistentDataPath + "/levels.json";
        saveFilePath = Application.persistentDataPath + "/saveData.json";

        if (!File.Exists(levelFilePath) || File.ReadAllText(levelFilePath) != Resources.Load<TextAsset>("levels").text)
        {
            File.WriteAllText(levelFilePath, Resources.Load<TextAsset>("levels").text);
        }

        if(!File.Exists(saveFilePath))
        {
            File.WriteAllText(saveFilePath, Resources.Load<TextAsset>("saveData").text);
        }

        levels = JsonUtility.FromJson<levelCollection>(File.ReadAllText(levelFilePath)).levels;//Use Unity's json parser to parse in text from file
                                                                                               //Convert into serializable levelCollection class and get the resulting array of levels
        //Get saves from file and store them in a list
        saves = new List<save>(JsonUtility.FromJson<saveCollection>(File.ReadAllText(saveFilePath)).saves);


    }
	
    //Method sets level number and loads game scene, called when level button is selected
    public void loadLevel(int lvl)
    {
        levelNumber = lvl;

        checkSaveInfo();

        SceneManager.LoadScene("game");
    }

    public void loadNextLevel()
    {
        levelNumber++;

        checkSaveInfo();
    }

    private void checkSaveInfo()
    {
        if (levelNumber >= saves.Count)
        {
            for (int i = saves.Count; i < levelNumber; i++)
            {
                saves.Add(new save());
            }
        }
    }


    public void saveLevelInfo()
    {
        saveCollection saveColl = new saveCollection(saves.ToArray());

        File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveColl));
    }
}
