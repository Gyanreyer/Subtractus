using UnityEngine;
using UnityEngine.SceneManagement;

public class levelManager : MonoBehaviour {

    private int levelNumber;//Index for current level to load

    private level[] levels;//Array of levels loaded from file

    //Property to return current level that should be loaded
    public level CurrentLevel
    {
        get { return levels[levelNumber]; }
    }

    // Use this for initialization
    void Awake () {
        DontDestroyOnLoad(gameObject);//Don't destroy this gameobject between scenes, it will be used between scenes for loading levels

        string levelString = Resources.Load<TextAsset>("levels").text;//Load in json file for levels as a string

        levels = JsonUtility.FromJson<levelCollection>(levelString).levels;//Use Unity's json parser to parse in text from file
                                                                           //Convert into serializable levelCollection class and get the resulting array of levels
        levelString = null; //Clear out string now that it's parsed
    }
	
    //Method sets level number and loads game scene, called when level button is selected
    public void loadLevel(int lvl)
    {
        levelNumber = lvl;
        SceneManager.LoadScene("game");
    }
}
