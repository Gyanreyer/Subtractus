using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Collections;

public class levelManager : MonoBehaviour {

    public GameObject levelButtonPrefab;
    public GameObject backButtonPrefab;

    private int chapterNumber;//Index for current chapter
    private int levelNumber;//Index for current level to load

    private chapter currentChapter;

    private string chapterFilePathStub;

    //Property to return current level that should be loaded
    public level CurrentLevel { get { return currentChapter.levels[levelNumber]; }  }

    public uint CurrentHighscore
    {
        get { return CurrentLevel.best; }
        set
        {
            currentChapter.levels[levelNumber].best = value;
        }
    }
    public bool BeatCurrentPar
    {
        get { return (CurrentHighscore > 0 && CurrentHighscore <= CurrentLevel.par); }
    }
    public bool ChapterLoaded
    {
        get { return currentChapter != null; }
    }

    // Use this for initialization
    void Awake () {
        DontDestroyOnLoad(gameObject);//Don't destroy this gameobject between scenes, it will be used between scenes for loading levels

        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }        

        chapterFilePathStub = Application.persistentDataPath + "/chapter";

        currentChapter = null;
    }
	
    //Loads given chapter from file for future level loading
    public void loadChapter(int chapter)
    {
        string chapterFilePath = chapterFilePathStub + chapter + ".json";

        if (!File.Exists(chapterFilePath))
        {
            File.WriteAllText(chapterFilePath, Resources.Load<TextAsset>("chapter" + chapter).text);
            currentChapter = JsonUtility.FromJson<chapter>(File.ReadAllText(chapterFilePath));//Use Unity's json parser to parse in text from file
                                                                                              //Convert into serializable levelCollection class and get the resulting array of levels
                                                                                              //Get saves from file and store them in a list   
        }
        else//Check if level file is consistent with how levels are supposed to be, will make updates easier but maintain save info
        {
            currentChapter = JsonUtility.FromJson<chapter>(File.ReadAllText(chapterFilePath));

            level[] levelsCheck = JsonUtility.FromJson<chapter>(Resources.Load<TextAsset>("chapter" + chapter).text).levels;

            bool changeFound = false;

            for(int i = 0; i < currentChapter.levels.Length; i++)
            {
                if(currentChapter.levels[i].tiles != levelsCheck[i].tiles)
                {
                    currentChapter.levels[i].tiles = levelsCheck[i].tiles;
                    changeFound = true;
                }
                if(currentChapter.levels[i].par != levelsCheck[i].par)
                {
                    currentChapter.levels[i].par = levelsCheck[i].par;
                    changeFound = true;
                }
                if(currentChapter.levels[i].width != levelsCheck[i].width || currentChapter.levels[i].height != levelsCheck[i].height)
                {
                    currentChapter.levels[i].width = levelsCheck[i].width;
                    currentChapter.levels[i].height = levelsCheck[i].height;
                    changeFound = true;
                }

                if(changeFound)
                    levelsCheck[i] = currentChapter.levels[i];
            }       
            
            if(currentChapter.levels.Length < levelsCheck.Length)
            {
                currentChapter.levels = levelsCheck;
                changeFound = true;
            }

            if(changeFound)
                File.WriteAllText(chapterFilePathStub + currentChapter + ".json", JsonUtility.ToJson(currentChapter));
        }

        setupLevelButtons();

        Camera.main.GetComponent<sceneTransition>().slideRight();
    }

    public void setupLevelButtons()
    {      
        Vector3 buttonPosition = new Vector3(80, 120, 0);

        for (int i = 1; i <= currentChapter.levels.Length; i++)
        {
            GameObject currentLevelButton = Instantiate(levelButtonPrefab);

            int levIndex = i - 1;

            currentLevelButton.GetComponent<Button>().onClick.AddListener(delegate () { loadLevel(levIndex); });

            currentLevelButton.transform.SetParent(GameObject.Find("Canvas").transform, false);

            buttonPosition.x *= -1;

            currentLevelButton.GetComponent<RectTransform>().localPosition = buttonPosition + new Vector3(2 * Screen.width, 0, 0);

            if (i % 2 == 0)
            {
                buttonPosition.y -= 150;
            }

            currentLevelButton.GetComponentInChildren<TextMesh>().text = i.ToString();//Set text to reflect level number
        }

        GameObject backButton = Instantiate(backButtonPrefab);
        backButton.transform.SetParent(GameObject.Find("Canvas").transform, false);

        backButton.GetComponent<RectTransform>().localPosition = new Vector3(Screen.width * 2 - 140, 240, 0);

        backButton.GetComponent<Button>().onClick.AddListener(delegate () { Camera.main.GetComponent<sceneTransition>().slideLeft(); });

        backButton.name = backButtonPrefab.name;  
    }
    

    //Method sets level number and loads game scene, called when level button is selected
    public void loadLevel(int lvl)
    {
        levelNumber = lvl;
        Debug.Log(levelNumber);
        Debug.Log(currentChapter.levels[lvl]);

        SceneManager.LoadScene("game");
    }

    //Increment level index for loading next level, if this is called then already in game scene
    public void loadNextLevel()
    {
        levelNumber++;
    }

    //Overwrite chapter file with new data
    public void saveLevelInfo()
    {
        File.WriteAllText(chapterFilePathStub + currentChapter + ".json", JsonUtility.ToJson(currentChapter));
    }
}
