using UnityEngine;
using UnityEngine.SceneManagement;

//Class used to transition between game scenes, place on camera so always present
public class sceneTransition : MonoBehaviour {

    private Vector3 desiredPos;
    public GameObject levText;

    private Vector3 chapterViewPosition;

    void Start()
    {
        desiredPos = transform.position;
        chapterViewPosition = Vector3.zero;
    }

    void Update()
    {
        if(transform.position != desiredPos)
        {
            transform.position = Vector3.MoveTowards(transform.position,desiredPos,20f*Time.deltaTime);
        }
    }

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

    public void slideRight()
    {
        GameObject[] levButtons = GameObject.FindGameObjectsWithTag("LevSelButton");
        if (levButtons.Length >= 2)
        {
            float pos1 = levButtons[0].transform.position.x;
            float pos2 = levButtons[1].transform.position.x;
            desiredPos = new Vector3(0.5f * (pos1 + pos2), 0, -10);

            chapterViewPosition = desiredPos;

            GameObject lt = Instantiate(levText);

            lt.transform.SetParent(GameObject.Find("Canvas").transform);
            lt.transform.position = new Vector3(desiredPos.x,GameObject.Find("ChapterText").transform.position.y,0);
            lt.transform.localScale = Vector3.one;
            lt.name = levText.name;
        }
    }

    public void slideLeft()
    {
        desiredPos = new Vector3(0,0,-10);

        GameObject[] levButtons = GameObject.FindGameObjectsWithTag("LevSelButton");
        for(int i = 0; i < levButtons.Length; i++)
        {
            Destroy(levButtons[i]);
        }

        Destroy(GameObject.Find("LevelsText"));
        Destroy(GameObject.FindGameObjectWithTag("BackButton"));
    }

    void OnLevelWasLoaded(int index)
    {
        levelManager levMan = GameObject.Find("LevelManager").GetComponent<levelManager>();

        if (index == 1 && levMan.ChapterLoaded)
        {
            levMan.setupLevelButtons();

            transform.position = chapterViewPosition;
            desiredPos = transform.position;
        }
    }

}
