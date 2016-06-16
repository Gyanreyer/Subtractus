using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class levelManager : MonoBehaviour {

    private int levelNumber;

    public List<GameObject> levelButtons;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void loadLevel(int lvl)
    {
        levelNumber = lvl;
        SceneManager.LoadScene("game");

        GameObject.Find("BoardManagerGO").GetComponent<boardManager>().loadLevel(levelNumber);
    }
}
