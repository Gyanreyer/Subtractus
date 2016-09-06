using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class levelManager : MonoBehaviour {

    public GameObject levelButtonPrefab;//Prefab for level select buttons to generate when loading chapter

    private int chapterNumber;//Index for current chapter
    private int levelNumber;//Index for current level to load

    private chapter currentChapter;//Current loaded chapter

    private string chapterFilePathStub;//Base string for file path to chapters

    //Property to return current level that should be loaded
    public level CurrentLevel { get { return currentChapter.levels[levelNumber]; }  }

    //Return high score for current level or set new score
    public uint CurrentHighscore
    {
        get { return CurrentLevel.best; }
        set
        {
            currentChapter.levels[levelNumber].best = value;
        }
    }

    //Whether a chapter is currently loaded or not
    public bool ChapterLoaded
    {
        get { return currentChapter != null; }
    }

    public bool LastLevel { get { return !((levelNumber + 1) < currentChapter.levels.Length); } }

    // Use this for initialization
    void Awake () {
        //If this is a duplicate of the original persistant LevelManager then destroy this GO
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);//Don't destroy this gameobject between scenes, it will be used between scenes for loading levels    

        //Get data path to chapters, can just add "[number].json" to end of this stub to get a chapter
        chapterFilePathStub = Application.persistentDataPath + "/chapter";

        currentChapter = null;//Current chapter is null by default until one is loaded
    }
	
    //Loads given chapter from file for future level loading
    public void loadChapter(int chapter)
    {
        chapterNumber = chapter;//Store current chapter number for future use
        string chapterFilePath = chapterFilePathStub + chapter + ".json";//Get full file path to this chapter

        Debug.Log("Loaded: " + chapterFilePath);

        //ALL THIS FILE CHECKING STUFF SHOULD GO FOR FINAL RELEASE, ABSOLUTELY NOT NECESSARY BUT KINDA MAKES ANY TWEAKS DURING DEV EASIER
        //If chapter save file doesn't exist at persistent path, load the default from Resources and store its file at correct path
        if (!File.Exists(chapterFilePath))
        {
            File.WriteAllText(chapterFilePath, Resources.Load<TextAsset>("chapter" + chapter).text);
            currentChapter = JsonUtility.FromJson<chapter>(File.ReadAllText(chapterFilePath));//Use Unity's json parser to parse in text from file
                                                                                              //Convert into serializable levelCollection class and get the resulting array of levels
                                                                                              //Get saves from file and store them in a list   
        }
        else//Check if level file is consistent with how levels are supposed to be, will make updates easier but maintain save info
        {
            currentChapter = JsonUtility.FromJson<chapter>(File.ReadAllText(chapterFilePath));//Load in save file

            level[] levelsCheck = JsonUtility.FromJson<chapter>(Resources.Load<TextAsset>("chapter" + chapter).text).levels;//Load in official default file to check against

            bool changeFound = false;//Whether a change has been found between save file and default
            
            for(int i = 0; i < currentChapter.levels.Length; i++)
            {
                

                //If a disparity is found, set stored level to match default official one
                if (levelsCheck[i].tiles.Length != currentChapter.levels[i].tiles.Length || levelsCheck[i].par != currentChapter.levels[i].par)
                {                                       
                    currentChapter.levels[i] = levelsCheck[i];
                    changeFound = true;               
                }
                else
                {
                    for (int j = 0; j < levelsCheck[i].tiles.Length; j++)
                    {
                        //Debug.Log(JsonUtility.ToJson(currentChapter.levels[i].tiles[j]));
                        if(JsonUtility.ToJson(currentChapter.levels[i].tiles[j]) != JsonUtility.ToJson(levelsCheck[i].tiles[j]))
                        {
                            currentChapter.levels[i] = levelsCheck[i];
                            break;
                        }

                        if(j >= levelsCheck[i].tiles.Length)
                        {
                            levelsCheck[i].best = currentChapter.levels[i].best;
                        }
                    }                    
                }

            }       
            
            //If there's less levels in file than should be, reset to newer levels (this will retain high scores)
            if(currentChapter.levels.Length != levelsCheck.Length)
            {
                currentChapter.levels = levelsCheck;
                changeFound = true;
            }

            //If change was found, overwrite old save file with updated one
            if(changeFound)
                File.WriteAllText(chapterFilePathStub + chapterNumber + ".json", JsonUtility.ToJson(currentChapter));
        }

        setupLevelButtons();//Set up level buttons for chapter now that it's loaded

        Camera.main.GetComponent<sceneTransition>().slideToLevelSelect();//Make slide transition to level select buttons
    }

    //Set up level select buttons to open each level from loaded chapter
    public void setupLevelButtons()
    {      
        Vector3 buttonPosition = new Vector3(-1.7f, 1.5f, 0);//Initial position for button, top left

        GameObject buttonParent = GameObject.Find("LevelButtons");//Parent object to place all level buttons under

        bool unlocked = true;

        //Loop through all levels in loaded chapter
        for (int i = 0; i < currentChapter.levels.Length; i++)
        {
            //Instantiate new level button
            GameObject currentLevelButton = Instantiate(levelButtonPrefab);

            //Index for level to link button to
            int levIndex = i;

            //Add listener for button to load appropriate level when clicked
            currentLevelButton.GetComponent<Button>().onClick.AddListener(delegate () { loadLevel(levIndex); });
            
            //Set button as child under buttons parent object
            currentLevelButton.transform.SetParent(buttonParent.transform, false);

            //Modify x position depending on if this is the 1st, 2nd, or 3rd button in this row
            buttonPosition.x = -1.7f + ((i % 3) * 1.7f);
            currentLevelButton.GetComponent<RectTransform>().localPosition = buttonPosition;

            //Modify y position if need to start new row
            if ((i+1) % 3 == 0)
            {
                buttonPosition.y -= 1.7f;
            }

            //Set button text to reflect level number
            currentLevelButton.GetComponentInChildren<TextMesh>().text = (i + 1).ToString();

            //If last level was unlocked then check this one, otherwise there's no point since it's locked
            if (unlocked)
            {
                //Current best score for this level
                uint best = currentChapter.levels[i].best;

                //If last level was completed, unlock this level
                if (i == 0 || currentChapter.levels[i - 1].best != 0)
                {
                    currentLevelButton.GetComponent<Button>().interactable = true;
                    currentLevelButton.GetComponentInChildren<TextMesh>().color = Color.white;

                    //If best score beat par on this level, display star
                    if (best > 0 && best <= currentChapter.levels[i].par)
                    {                   
                        currentLevelButton.transform.GetChild(1).gameObject.SetActive(true);//NOTE: ADD CHECK MARK TO INDICATE IF DIDN'T BEAT PAR
                    }
                }
                else//If this level is locked then stop checking against locked levels
                {
                    unlocked = false;
                }
            }
        }

        //Set title text for level select menus
        GameObject.Find("LevelSelectTitle").GetComponent<Text>().text = "Chapter " + chapterNumber;

        //Set up stuff for scrolling level navigation
        buttonParent.GetComponent<buttonScroller>().setupNewChapter();
    }
    

    //Method sets level number and loads game scene, called when level button is selected
    public void loadLevel(int lvl)
    {
        levelNumber = lvl;

        Camera.main.GetComponent<sceneTransition>().loadGame();
    }

    //Increment level index for loading next level, if this is called then already in game scene
    public void loadNextLevel()
    {
        levelNumber++;
    }

    //Overwrite chapter file with new data
    public void saveLevelInfo()
    {
        File.WriteAllText(chapterFilePathStub + chapterNumber + ".json", JsonUtility.ToJson(currentChapter));
    }
}
