using UnityEngine;
using UnityEngine.SceneManagement;

//Class used to transition between game scenes, place on camera so always present
public class sceneTransition : MonoBehaviour {

    //Load level select scene
    public void loadLevelSelect()
    {
        SceneManager.LoadScene("levelSelect");
    }

    //Load game scene
    public void loadGame()
    {
        SceneManager.LoadScene("game");
    }
}
