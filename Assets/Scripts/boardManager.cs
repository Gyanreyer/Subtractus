using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


//Enum for direction of swipe
public enum SwipeDirection
{  
    horizontal,//Longer on x than y axis
    vertical,//Longer on y than x axis
    none//Shorter than min swipe dist, not detected
}

public class boardManager : MonoBehaviour {

    //Enum for state game is currently in
    private enum GameState
    {
        playing,//Game is in progress, trying to solve puzzle
        won//Game is over, on win screen
    }

    //Prefabs for red, blue, and gray tiles, each has sprite and number text
    public GameObject redTilePrefab;
    public GameObject blueTilePrefab;
    public GameObject grayTilePrefab;


    //Prefabs for grid's background and spaces to use when building the level's grid
    public GameObject gridBG;
    public GameObject gridSpacePrefab;

    public GameObject fireworksPartSys;//Particle system for fireworks that shoots off when player wins

    //Sprites for star that is displayed when player wins, index 0 is silver star for if didn't beat par, index 1 is gold star if did beat it
    public Sprite[] stars;

    //Sound effects that are played during game play
    //0 - Touch sound effect when player begins a swipe
    //1 - Slide sound effect when player completes swipe and tiles slide into place
    //2 - Win chime sound when player beats level
    public AudioClip[] soundEffects;
    
    private GameObject winScreen;//Popup UI menu that appears when player beats game

    private levelManager levManager;//Level manager used to build board for current level, get it in Start
    private level currentLevel;//The current level to reference for building board, accessed from level manager

    private AudioSource soundPlayer;//Audio source component plays sound effects

    private const float minSwipeDist = 50f;//Minimum distance to detect a swipe
    private Vector2 swipeStartPosition;//Point on screen where player started a swipe
    private Vector2 swipeVector;//Vector from start position to where player's finger is now to represent swipe
    private SwipeDirection swipeDir;//Direction that current swipe is in

    private GameState gameState;//State game is currently in, either playing or win

    private Vector3[,] gridPosition;//2D array of the coordinates for each space on the grid

    private List<GameObject> tiles;//List of all active tiles in level

    private int grayTilesCount;//Number of gray tiles in puzzle, allows win check to ignore gray tiles in tile list when checking if all tiles removed

    private int moves = 0;//Number of moves the player has made for this level
    private float borderWidth = 0.02f;//Width of borders between spaces on grid in percent of background size
    private Vector2 tileDist;//Vector holds x and y distance between grid spaces
    private float worldScreenWidth;
    private float localScreenWidth;

    //Properties
    public List<GameObject> Tiles { get { return tiles; } }
    public Vector3[,] GridPosition { get { return gridPosition; } }
    public uint BoardWidth { get { return currentLevel.width; } }
    public uint BoardHeight { get { return currentLevel.height; } }
    public Vector2 TileDist { get { return tileDist; } }
    public SwipeDirection SwipeDir { get { return swipeDir; } }

    // Use this for initialization
    void Start () {
        winScreen = GameObject.Find("WinScreen");//Get win screen GO so its contents can be modified when player wins
        soundPlayer = GetComponent<AudioSource>();//Get audio source component so can play sound effects

        tiles = new List<GameObject>();//Initialize tiles list

        swipeDir = SwipeDirection.none;//Set swipe direction to defaul

        levManager = GameObject.Find("LevelManager").GetComponent<levelManager>();//Get level manager from level manager gameobject in scene
        currentLevel = levManager.CurrentLevel;//Get current level from level manager, will reference this for building board

        worldScreenWidth = Camera.main.orthographicSize * 2f * Screen.width / Screen.height;//Get width of screen in world units
                                                                                            //(orthographic size of cam is 1/2 of screen height, so multiply by 2 and then the ratio of width/height to get width)

        localScreenWidth = worldScreenWidth / gridBG.GetComponent<SpriteRenderer>().bounds.size.x;//Get local width of screen by dividing world width by width of background sprite

        buildGrid();//Sets up grid with appropriate spaces reflecting current level's width and height, then calls LoadTiles function to load in tiles

        gameState = GameState.playing;//Default state to playing
    }

	// Update is called once per frame
	void Update () {

        //If there are still non-gray tiles in play and the game hasn't registered that player has won yet, switch over game state and run winLevel() to set everything up
        if(tiles.Count <= grayTilesCount && gameState != GameState.won)
        {
            gameState = GameState.won;
            winLevel();
        }

        //Otherwise if the game is in a play state, check for input
        else if(gameState == GameState.playing)
            getInput();//Get input from player each frame while playing          
            
    }

    //Get input from player
    void getInput()
    {
#if UNITY_EDITOR

        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            swipeVector = new Vector2(Input.mousePosition.x - swipeStartPosition.x, Input.mousePosition.y - swipeStartPosition.y);//Update swipe vector from start position to where finger is now

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
                }

                //Play pop sound effect when swipe is long enough to register
                if(swipeDir != SwipeDirection.none)
                    soundPlayer.PlayOneShot(soundEffects[0]);


            }

            //Loop through all tiles and partially slide them in direction of swipe vector
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].GetComponent<tile>().PartialSlide(swipeVector);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            swipeDir = SwipeDirection.none;//Reset swipe direction

            bool tilesMoved = false;//Whether any tiles were moved with this swipe, if yes then a move can be added to move counter

            //Loop through all tiles and try to move them
            for (int i = 0; i < tiles.Count; i++)
            {
                //MoveTile returns true if moved and false if not, set tilesMoved to true if any tiles moved
                if (tiles[i].GetComponent<tile>().MoveTile())
                {
                    tilesMoved = true;
                }
            }

            //If any tiles moved, add a move to move counter
            if (tilesMoved)
            {
                soundPlayer.PlayOneShot(soundEffects[1]);//Play slide sound effect
                addMove();
            }
        }
#endif

        //If using android, use touch controls
#if UNITY_ANDROID
        //If player isn't touching the screen, we don't need to do anything so just return early
        if (Input.touchCount <= 0)
            return;

        //If the player is touching the screen more than once, get the stored touch at index 0
        Touch touch = Input.GetTouch(0);

        //Switch statement based on phase that touch is in, could be started, moved, or ended
        switch (touch.phase)
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

                    soundPlayer.PlayOneShot(soundEffects[0]);//Play pop sound effect when swipe long enough, break will skip it otherwise
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

                soundPlayer.PlayOneShot(soundEffects[1]);//Play slide sound effect

                bool tilesMoved = false;//Whether any tiles were moved with this swipe, if yes then a move can be added to move counter

                //Loop through all tiles and try to move them
                for (int i = 0; i < tiles.Count; i++)
                {
                    //MoveTile returns true if moved and false if not, set tilesMoved to true if any tiles moved
                    if (tiles[i].GetComponent<tile>().MoveTile())
                    {
                        tilesMoved = true;
                    }
                }

                //If any tiles moved, add a move to move counter
                if (tilesMoved)
                    addMove();

                break;
        }
#endif

    }




    //Builds grid based on width and height of current level
    private void buildGrid()
    {
        currentLevel = levManager.CurrentLevel;//Get level to load from level manager

        GameObject background = Instantiate(gridBG);//Instantiate background for grid

        background.transform.localScale = new Vector3(localScreenWidth * 0.9f, localScreenWidth, 1f);//Scale background down so that it only takes up 90% of screen width

        Vector3 bgSize = background.transform.localScale;//Store scale of background for easier access

        GameObject gridSpace = Instantiate(gridSpacePrefab);

        //Set scale of grid space prefab to appropriate portion of grid, also factoring in border width
        gridSpace.transform.localScale = new Vector3((bgSize.x / (float)currentLevel.width) - borderWidth * bgSize.x, (bgSize.y / (float)currentLevel.height) - borderWidth * bgSize.y, 1f);
        
        Vector3 spaceWorldScale = gridSpace.GetComponent<SpriteRenderer>().bounds.size;//Store size of grid space in world units
        Vector3 worldBorder = background.GetComponent<SpriteRenderer>().bounds.size * borderWidth;//Store size of border in world units by multiplying background's size by border width percentage

        //Calculate start position from where grid spaces will spawn, this is in lower left corner and will have grid coordinates (0,0)
        Vector3 startPos = background.GetComponent<SpriteRenderer>().bounds.min + new Vector3(spaceWorldScale.x, 0, -0.5f) + (worldBorder / 2);

        background.transform.localScale *= 1.04f;//Increase size of background so that its outer borders are slightly larger

        gridPosition = new Vector3[currentLevel.width, currentLevel.height];//Initialize grid position array to dimensions of grid
        GameObject newSpace;//Temporarily store grid spaces as they are spawned for each position on grid

        tileDist = new Vector2(worldBorder.x + spaceWorldScale.x, worldBorder.y + spaceWorldScale.y);

        gridSpace.transform.position = startPos;

        //Loop through all grid coordinates and spawn a new space at each one
        for (int y = 0; y < currentLevel.height; y++)
        {
            //Move gridSpace back to start of row
            gridSpace.transform.position = new Vector3(startPos.x,gridSpace.transform.position.y,-0.5f);

            for (int x = 0; x < currentLevel.width; x++)
            {
                //Instantiate a copy of gridSpace at its current position
                newSpace = Instantiate(gridSpace);
                newSpace.transform.parent = background.transform;//Make space a child of background

                gridPosition[x, y] = newSpace.transform.position;//Store position for grid space at these coordinates

                gridSpace.transform.position += new Vector3(tileDist.x,0,0);//Move gridSpace along to next x pos on grid
            }

            gridSpace.transform.position += new Vector3(0,tileDist.y,0);//Increment y pos to next row on grid
        }

        LoadTiles(gridSpace);//Load tiles to place on board

        Destroy(gridSpace);//Destroy gridSpace object now that done with it
    }

    //Loads tiles from current level and places them on grid
    private void LoadTiles(GameObject gridSpace)
    {
        swipeDir = SwipeDirection.none;

        //Loop through currently existing leftover tiles and destroy their gameobjects
        for (int i = 0; i < tiles.Count; i++)
        {
            Destroy(tiles[i]);
        }

        tiles.Clear();//Clear tiles list

        GameObject newTile;
        tile tileComponent;

        Vector3 gridSpaceWorldSize = gridSpace.GetComponent<SpriteRenderer>().bounds.size;

        SpriteRenderer tileRend = redTilePrefab.GetComponentInChildren<SpriteRenderer>();

        Vector3 localTileSize = new Vector3(gridSpaceWorldSize.x * redTilePrefab.transform.localScale.x / tileRend.bounds.size.x, gridSpaceWorldSize.y * redTilePrefab.transform.localScale.y / tileRend.bounds.size.y, 1f) * 755f/720f;

        grayTilesCount = 0;

        //Loop through array of tiles stored in current level
        for (int i = 0; i < currentLevel.tiles.Length; i++)
        {
            //Set gameobject to appropriate type/color
            switch (currentLevel.tiles[i].type)
            {
                case "red":
                    newTile = Instantiate(redTilePrefab);
                    break;
                case "blue":
                    newTile = Instantiate(blueTilePrefab);
                    break;
                default:
                    grayTilesCount++;
                    newTile = Instantiate(grayTilePrefab);
                    break;

            }
          

            tileComponent = newTile.GetComponent<tile>();

            tileComponent.setUpTile(currentLevel.tiles[i],i);

            newTile.transform.localScale = localTileSize;            

            tiles.Add(newTile);//Spawn tile and add it to list, it will sort its size and position out on awake based on given x and y coords
        }

        gameState = GameState.playing;
    }

    //Add a move to move counter and change UI text to reflect this
    public void addMove()
    {
        moves++;

        GameObject.Find("MovesText").GetComponent<Text>().text = "Moves: " + moves;
    }

    //Clear all tiles and load them again, called when reset button is hit
    public void resetTiles()
    {
        Camera.main.GetComponent<sceneTransition>().slideToPoint(0, 0);

        //Reset moves counter to 0
        moves = -1;
        addMove();

        LoadTiles(GameObject.FindGameObjectWithTag("GridSpace"));//Reload tiles for current level    
        
    }

    private void moveCameraDown()
    {
        Camera.main.GetComponent<sceneTransition>().slideToPoint(0, winScreen.transform.position.y);
    }

    //Runs when player has cleared tiles in level
    private void winLevel()
    {
        soundPlayer.PlayOneShot(soundEffects[2]);

        Invoke("shootFireworks", .25f);
        Invoke("shootFireworks", .4f);
        Invoke("shootFireworks", .55f);

        winScreen.transform.Find("FinalMovesText").GetComponent<Text>().text = "Moves: " + moves;
        winScreen.transform.Find("ParText").GetComponent<Text>().text = "Par: " + currentLevel.par;

        if(levManager.CurrentHighscore == 0 || moves < levManager.CurrentHighscore)
        {
            levManager.CurrentHighscore = (uint)moves;
            levManager.saveLevelInfo();           
        }

        Image starImage = winScreen.transform.Find("StarImage").GetComponent<Image>();

        if (levManager.CurrentLevel.par >= moves)
        {
            starImage.sprite = stars[1];
        }
        else
        {
            starImage.sprite = stars[0];
        }

        Invoke("moveCameraDown",1f);

        if(levManager.LastLevel)
        {
            GameObject.Find("NextButton").GetComponent<Button>().interactable = false;

            Color disabledColor = new Color(.6f, .6f, .6f, .6f);

            GameObject.Find("NextText").GetComponent<Text>().color = disabledColor;
            GameObject.Find("NextIcon").GetComponent<Image>().color = disabledColor;
            
        }

    }

    private void shootFireworks()
    {
        Instantiate(fireworksPartSys, new Vector3(Random.Range(-3,3), Random.Range(-4, 4), -6), Quaternion.identity);
    }

    public void loadNextLevel()
    {
        levManager.loadNextLevel();
        currentLevel = levManager.CurrentLevel;

        Destroy(GameObject.FindGameObjectWithTag("Grid"));

        //Reset moves counter to 0
        moves = -1;
        addMove();

        buildGrid();

        Camera.main.GetComponent<sceneTransition>().slideToPoint(0, 0);
    }
}
