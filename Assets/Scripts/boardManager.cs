using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

public class boardManager : MonoBehaviour {

    //Mobile swiping stuff
    private bool swiping;
    public enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private const float minSwipeDist = 50f;

    private Vector2 swipeStartPosition;
    private float swipeStartTime;

    private Vector2 swipeVector;

    public int moves = 0;

    float borderWidth = 0.01f;//Width of borders between spaces on grid in percent of background size

    public GameObject redTilePrefab;
    public GameObject blueTilePrefab;

    public GameObject gridBG;
    public GameObject gridSpace;

    public GameObject[,] gridSpots;

    private List<GameObject> tiles;

    private level[] levels;
    private level currentLevel;

    public List<GameObject> Tiles { get { return tiles; } }
    public int BoardWidth { get { return currentLevel.width; } }
    public int BoardHeight { get { return currentLevel.height; } }

    // Use this for initialization
    void Start () {

        tiles = new List<GameObject>();

        string levelString = File.ReadAllText(Application.dataPath + "/Data/levels.json");//String contains all data text for levels
        levels = JsonUtility.FromJson<levelCollection>(levelString).levels;
        levelString = null; //Clear out string now that it's parsed

        loadLevel(0);


    }

	// Update is called once per frame
	void Update () {
        getInput();
    }

    void getInput()
    {
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            switch(touch.phase)
            {
                case TouchPhase.Began:
                    swipeStartPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    swipeVector = touch.position - swipeStartPosition;

                    if(swipeVector.sqrMagnitude > Mathf.Pow(minSwipeDist,2))
                    {
                        for (int i = 0; i < tiles.Count; i++)
                        {
                            tiles[i].GetComponent<tile>().PartialSlide(swipeVector);
                        }
                    }

                    break;

                case TouchPhase.Ended:
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        tiles[i].GetComponent<tile>().MoveTile();
                        tiles[i].GetComponent<tile>().UpdateTile();
                    }

                    break;

            }

        }
#endif

#if UNITY_EDITOR    
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Hit up");
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].GetComponent<tile>().MoveTile(0, 1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].GetComponent<tile>().MoveTile(0, -1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].GetComponent<tile>().MoveTile(-1, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].GetComponent<tile>().MoveTile(1, 0);
            }
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<tile>().UpdateTile();
        }
#endif

    }

    void loadLevel(int levelNumber)
    {
        currentLevel = levels[levelNumber];

        buildBoard(currentLevel.width, currentLevel.height);

        LoadTiles();

    }

    void buildBoard(int width, int height)
    {
        gridSpots = new GameObject[width, height];

        GameObject background = (GameObject)Instantiate(gridBG, Vector3.zero, gridBG.transform.rotation);

        float worldScreenWidth = Camera.main.orthographicSize * 2f * Screen.width / Screen.height;

        float localScreenWidth = worldScreenWidth / background.GetComponent<MeshRenderer>().bounds.size.x;

        background.transform.localScale = new Vector3(localScreenWidth * 0.9f, 1f, localScreenWidth);

        Vector3 bgSize = background.transform.localScale;

        gridSpace.transform.localScale = new Vector3((bgSize.x / (float)width) - borderWidth * bgSize.x, 1f, (bgSize.z / (float)height) - borderWidth * bgSize.z);

        Vector3 spaceWorldScale = gridSpace.GetComponent<MeshRenderer>().bounds.size;
        Vector3 worldBorder = background.GetComponent<MeshRenderer>().bounds.size * borderWidth;

        Vector3 startPos = background.GetComponent<MeshRenderer>().bounds.min + (spaceWorldScale + worldBorder) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gridSpots[x, y] = (GameObject)Instantiate(gridSpace, startPos + new Vector3((worldBorder.x + spaceWorldScale.x) * x, (worldBorder.y + spaceWorldScale.y) * y, -0.5f), gridSpace.transform.rotation);
            }
        }

        background.transform.localScale *= 1.03f;
    }

    void LoadTiles()
    {

        for (int i = 0; i < currentLevel.tiles.Length; i++)
        {
            GameObject newTile;

            if (currentLevel.tiles[i].type == "red")
            {
                newTile = redTilePrefab;

            }
            else
            {
                newTile = blueTilePrefab;
            }

            newTile.GetComponent<tile>().number = currentLevel.tiles[i].number;

            newTile.GetComponent<tile>().xPos = currentLevel.tiles[i].xPos;
            newTile.GetComponent<tile>().yPos = currentLevel.tiles[i].yPos;

            newTile.GetComponent<tile>().index = i;

            tiles.Add(Instantiate(newTile));
            

        }
    }
}
