using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum SwipeDirection
{  
    horizontal,
    vertical,
    none
}

public class boardManager : MonoBehaviour {

    //Prefabs for red and blue tiles, each has sprite and number text
    public GameObject redTilePrefab;
    public GameObject blueTilePrefab;

    //Prefabs for grid's background and spaces to use when building the level's grid
    public GameObject gridBG;
    public GameObject gridSpace;

    public Text movesText;//UI text for moves counter

    private levelManager levManager;//Level manager used to build board for current level, get it in Start
    private level currentLevel;//The current level to reference for building board, accessed from level manager

    private const float minSwipeDist = 50f;//Minimum distance to detect a swipe
    private Vector2 swipeStartPosition;//Point on screen where player started a swipe
    private Vector2 swipeVector;//Vector from start position to where player's finger is now to represent swipe
    public SwipeDirection swipeDir;//Direction that current swipe is in

    private Vector3[,] gridPosition;//2D array of the coordinates for each space on the grid

    private List<GameObject> tiles;//List of all active tiles in level

    private int moves = 0;//Number of moves the player has made for this level
    private float borderWidth = 0.02f;//Width of borders between spaces on grid in percent of background size
    private Vector2 tileDist;//Vector holds x and y distance between grid spaces

    //Properties
    public List<GameObject> Tiles { get { return tiles; } }
    public Vector3[,] GridPosition { get { return gridPosition; } }
    public int BoardWidth { get { return currentLevel.width; } }
    public int BoardHeight { get { return currentLevel.height; } }
    public Vector2 TileDist { get { return tileDist; } }

    // Use this for initialization
    void Start () {
        tiles = new List<GameObject>();//Initialize tiles list

        swipeDir = SwipeDirection.none;//Set swipe direction to defaul

        levManager = GameObject.Find("LevelManager").GetComponent<levelManager>();//Get level manager from level manager gameobject in scene

        currentLevel = levManager.CurrentLevel;//Get current level from level manager, will reference this for building board

        buildGrid();//Sets up grid with appropriate spaces reflecting current level's width and height

        LoadTiles();//Loads tiles based on current level
    }

	// Update is called once per frame
	void Update () {
        getInput();//Get input from player each frame
    }

    //Get input from player
    void getInput()
    {
        //If using android, use touch controls
#if UNITY_ANDROID
        //If player isn't touching the screen, we don't need to do anything so just return early
        if (Input.touchCount <= 0)
            return;

        //If the player is touching the screen more than once, get the stored touch at index 0
        Touch touch = Input.GetTouch(0);

        //Switch statement based on phase that touch is in, could be started, moved, or ended
        switch(touch.phase)
        {
            //If touch just began, store the position of the touch as the start position of swipe for future calculations
            case TouchPhase.Began:
                swipeStartPosition = touch.position;
                break;

            //If touch has moved from start point, update swipe vector and partially slide tiles in appropriate direction
            case TouchPhase.Moved:
                swipeVector = touch.position - swipeStartPosition;//Update swipe vector from start position to where finger is now

                if (swipeDir == SwipeDirection.none)
                {
                    //If x axis of swipe vector is longer than minimum length and longer than y, set swipe direction to horiz
                    if (Mathf.Abs(swipeVector.x) > minSwipeDist && Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
                    {
                        swipeDir = SwipeDirection.horizontal;
                    }
                    //Otherwise if y axis is longer than min length set swipe direction to vert
                    else if (Mathf.Abs(swipeVector.y) > minSwipeDist)
                    {
                        swipeDir = SwipeDirection.vertical;
                    }
                    else//Otherwise just set swipe direction to default and break early
                    {
                        swipeDir = SwipeDirection.none;
                        break;
                    }
                }

                //Loop through all tiles and partially slide them in direction of swipe vector
                for (int i = 0; i < tiles.Count; i++)
                {
                    tiles[i].GetComponent<tile>().PartialSlide(swipeVector);
                }

                break;
            
            //If touch has ended, move the tiles appropriately based on swipe made
            case TouchPhase.Ended:
                swipeDir = SwipeDirection.none;//Reset swipe direction

                bool tilesMoved = false;//Whether any tiles were moved with this swipe, if yes then a move can be added to move counter

                //Loop through all tiles and try to move them
                for (int i = 0; i < tiles.Count; i++)
                {
                    //MoveTile returns true if moved and false if not, set tilesMoved to true if any tiles moved
                    if(tiles[i].GetComponent<tile>().MoveTile())
                    {
                        tilesMoved = true;
                    }
                }

                //If any tiles moved, add a move to move counter
                if(tilesMoved)
                    addMove();

                break;
        }

        
#endif
        //If in editor just use keyboard controls, TAKE THIS OUT IN FINAL VERSION
#if UNITY_EDITOR    
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if(tiles[i].GetComponent<tile>().MoveTile(0, 1))
                {
                    moved = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if(tiles[i].GetComponent<tile>().MoveTile(0, -1))
                {
                    moved = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if(tiles[i].GetComponent<tile>().MoveTile(-1, 0))
                {
                    moved = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if(tiles[i].GetComponent<tile>().MoveTile(1, 0))
                {
                    moved = true;
                }
            }
        }
        else
            return;

        if(moved)
            addMove();
#endif

    }

    //Builds grid based on width and height of current level
    private void buildGrid()
    {
        gridSpace.transform.localScale = Vector3.one;//Ensure that grid space prefab is full default size so that it can be resized correctly        

        GameObject background = (GameObject)Instantiate(gridBG, Vector3.zero, gridBG.transform.rotation);//Instantiate background for grid

        float worldScreenWidth = Camera.main.orthographicSize * 2f * Screen.width / Screen.height;//Get width of screen in world units
                                //(orthographic size of cam is 1/2 of screen height, so multiply by 2 and then the ratio of width/height to get width)

        float localScreenWidth = worldScreenWidth / background.GetComponent<SpriteRenderer>().bounds.size.x;//Get local width of screen by dividing world width by width of background sprite

        background.transform.localScale = new Vector3(localScreenWidth * 0.9f, localScreenWidth, 1f);//Scale background down so that it only takes up 90% of screen width

        Vector3 bgSize = background.transform.localScale;//Store scale of background for easier access

        //Set scale of grid space prefab to appropriate portion of grid, also factoring in border width
        gridSpace.transform.localScale = new Vector3((bgSize.x / (float)currentLevel.width) - borderWidth * bgSize.x, (bgSize.y / (float)currentLevel.height) - borderWidth * bgSize.y, 1f);
        
        Vector3 spaceWorldScale = gridSpace.GetComponent<SpriteRenderer>().bounds.size;//Store size of grid space in world units
        Vector3 worldBorder = background.GetComponent<SpriteRenderer>().bounds.size * borderWidth;//Store size of border in world units by multiplying background's size by border width percentage

        //Calculate start position from where grid spaces will spawn, this is in lower left corner and will have grid coordinates (0,0)
        Vector3 startPos = background.GetComponent<SpriteRenderer>().bounds.min + new Vector3(spaceWorldScale.x, 0, 0) + worldBorder / 2;

        background.transform.localScale *= 1.04f;//Increase size of background so that its outer borders are slightly larger


        gridPosition = new Vector3[currentLevel.width, currentLevel.height];//Initialize grid position array to dimensions of grid
        GameObject newSpace;//Temporarily store grid spaces as they are spawned for each position on grid

        //Loop through all grid coordinates and spawn a new space at each one
        for (int y = 0; y < currentLevel.height; y++)
        {
            for (int x = 0; x < currentLevel.width; x++)
            {
                //Instantiate at appropriate position based on coordinates
                newSpace = (GameObject)Instantiate(gridSpace, startPos + new Vector3((worldBorder.x + spaceWorldScale.x) * x, (worldBorder.y + spaceWorldScale.y) * y, -0.5f), Quaternion.identity);
                newSpace.transform.parent = background.transform;

                gridPosition[x, y] = newSpace.transform.position;//Store position for grid space at these coordinates
            }
        }   

        tileDist = new Vector2(gridPosition[1, 0].x - gridPosition[0, 0].x, gridPosition[0, 1].y - gridPosition[0, 0].y);

    }

    //Loads tiles from current level and places them on grid
    private void LoadTiles()
    {
        GameObject newTile;

        //Loop through array of tiles stored in current level
        for (int i = 0; i < currentLevel.tiles.Length; i++)
        {
            //If the current tile's type is red, set its gameobject to the red tile prefab, otherwise set it to blue
            if (currentLevel.tiles[i].type == "red")
            {
                newTile = redTilePrefab;
            }
            else
            {
                newTile = blueTilePrefab;
            }

            //Set values for tile based on current level info
            newTile.GetComponent<tile>().number = currentLevel.tiles[i].number;
            newTile.GetComponent<tile>().xPos = currentLevel.tiles[i].xPos;
            newTile.GetComponent<tile>().yPos = currentLevel.tiles[i].yPos;

            newTile.GetComponent<tile>().index = i;//Each tile has unique index to avoid comparisons with itself when checking tiles in tile list

            tiles.Add(Instantiate(newTile));//Spawn tile and add it to list, it will sort its position out on awake based on given x and y coords
        }
    }

    //Add a move to move counter and change UI text to reflect this
    public void addMove()
    {
        moves++;

        movesText.text = "Moves: " + moves;
    }

    //Clear all tiles and load them again, called when reset button is hit
    public void resetTiles()
    {
        //Loop through tiles and destroy their gameobjects
        for(int i = 0; i < tiles.Count; i++)
        {
            Destroy(tiles[i]);
        }

        tiles.Clear();//Clear tiles list

        //Reset moves counter to 0
        moves = -1;
        addMove();

        LoadTiles();//Reload tiles for current level
    }
}
