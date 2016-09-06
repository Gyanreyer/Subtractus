using UnityEngine;
using UnityEngine.SceneManagement;

//Class used to transition between game scenes, place on camera so always present
public class sceneTransition : MonoBehaviour {

    private Vector3 desiredPos;

    private Vector3 startMenuPos;
    private Vector3 chaptSelPos;
    private Vector3 levSelPos;
    private Vector3 settingsMenuPos;

    private float animTime;
    public float animDuration = 2f;

    private float shakeAmount = .07f;
    private float shakeDecrease = 5f;
    private float shakeTime;

    void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
           // startMenuPos = GameObject.Find("StartMenu").transform.position - new Vector3(0, 0, 10);
            chaptSelPos = GameObject.Find("ChapterMenu").transform.position - new Vector3(0, 0, 10);
            levSelPos = GameObject.Find("LevelMenu").transform.position - new Vector3(0, 0, 10);
            settingsMenuPos = GameObject.Find("SettingsMenu").transform.position - new Vector3(0,0,10);    
            
            levelManager levMan = GameObject.Find("LevelManager").GetComponent<levelManager>();

            if (levMan.ChapterLoaded)
            {
                levMan.setupLevelButtons();
                transform.position = levSelPos + new Vector3(8,0,0);
                desiredPos = levSelPos;
            }
            else
            {
                //transform.position = startMenuPos;
                desiredPos = transform.position;
            }

            

            
        }
        else if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            desiredPos = new Vector3(0,0,-10);
        }
        else
            desiredPos = transform.position;
    }

    void Update()
    {
        if(shakeTime > 0)
        {
            transform.position = desiredPos + (Vector3)Random.insideUnitCircle * shakeAmount;

            shakeTime -= Time.deltaTime * shakeDecrease;
        }

        else if(transform.position != desiredPos)
        {
            animTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position,desiredPos,animTime/animDuration);
        }
        else
        {
            animTime = 0;
        }
    }

    public void shake()
    {
        shakeTime = .7f;
    }

    //Load level select scene
    private void loadLevelSelect()
    {
        SceneManager.LoadScene("menus");
    }
    private void openGameScene()
    {
        SceneManager.LoadScene("game");
    }

    public void backToLevSel()
    {
        slideToPoint(-7,transform.position.y);

        Invoke("loadLevelSelect",.3f);        
    }    

    //Load game scene
    public void loadGame()
    {
        slideToPoint(14,0);

        Invoke("openGameScene",.3f);
    }

    public void slideToLevelSelect()
    {
        slideToPoint(levSelPos);
    }

    public void slideToChapterSelect()
    {
        slideToPoint(chaptSelPos);
        Invoke("destroyLevelButtons", .4f);
    }

    public void slideToSettings()
    {
        slideToPoint(settingsMenuPos);
    }

    public void slideToPoint(float x, float y)
    { 
        if (desiredPos.x != x || desiredPos.y != y)
            desiredPos = new Vector3(x, y, -10);
        
        GetComponent<AudioSource>().Play();
        animTime = 0;
                 
    }

    public void slideToPoint(Vector3 point)
    {
        slideToPoint(point.x,point.y);
    }

    private void destroyLevelButtons()
    {
        foreach (GameObject button in GameObject.FindGameObjectsWithTag("LevSelButton"))
        {
            Destroy(button);
        }
    }

    public void toggleSound()
    {

    }
}
