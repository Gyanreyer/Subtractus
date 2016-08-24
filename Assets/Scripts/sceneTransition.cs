using UnityEngine;
using UnityEngine.SceneManagement;

//Class used to transition between game scenes, place on camera so always present
public class sceneTransition : MonoBehaviour {

    private Vector3 desiredPos;
    private Vector3 startMenuPos;
    private Vector3 chaptSelPos;
    private Vector3 levSelPos;

    private float camMoveSpeed = 25f;

    void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            startMenuPos = GameObject.Find("StartMenu").transform.position - new Vector3(0, 0, 10);
            chaptSelPos = GameObject.Find("ChapterMenu").transform.position - new Vector3(0, 0, 10);
            levSelPos = GameObject.Find("LevelMenu").transform.position - new Vector3(0, 0, 10);          
            
            levelManager levMan = GameObject.Find("LevelManager").GetComponent<levelManager>();

            if (levMan.ChapterLoaded)
            {
                levMan.setupLevelButtons();
                transform.position = levSelPos + new Vector3(8,0,0);
                desiredPos = levSelPos;
            }
            else
            {
                transform.position = startMenuPos;
                desiredPos = transform.position;
            }

            
        }
        else if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            transform.position = new Vector3(-8,0,-10);
            desiredPos = new Vector3(0,0,-10);
        }
        else
            desiredPos = transform.position;
    }

    void Update()
    {
        if(transform.position != desiredPos)
        {
            transform.position = Vector3.MoveTowards(transform.position,desiredPos,camMoveSpeed*Time.deltaTime);
        }
    }

    //Load level select scene
    private void loadLevelSelect()
    {
        SceneManager.LoadScene("levelSelect");
    }

    public void backToLevSel()
    {
        desiredPos = new Vector3(-7,0,-10);
        camMoveSpeed = 30f;

        Invoke("loadLevelSelect",.2f);
    }

    //Load game scene
    public void loadGame()
    {
        desiredPos = new Vector3(14,0,-10);
        camMoveSpeed = 30f;

        Invoke("openGameScene",.2f);
    }

    private void openGameScene()
    {
        SceneManager.LoadScene("game");
    }

    public void slideToLevelSelect()
    {
        desiredPos = levSelPos;
    }

    public void slideToChapterSelect()
    {
        desiredPos = chaptSelPos;
        camMoveSpeed = 25f;

        Invoke("destroyLevelButtons", .3f);

    }

    private void destroyLevelButtons()
    {
        foreach (GameObject button in GameObject.FindGameObjectsWithTag("LevSelButton"))
        {
            Destroy(button);
        }
    }

}
