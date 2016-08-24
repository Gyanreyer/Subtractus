using UnityEngine;

public class tile : MonoBehaviour
{
    public enum tileType
    {
        red,
        blue
    }

    public enum tileState
    {
        idle,
        swiping,
        animating
    }

    private boardManager BM;//Board manager

    public int index;//Index of tile used to uniquely identify it

    public int number;//Number of tile
    public int xPos, yPos;//x and y coordinates of tile on grid
    public int futureX, futureY;//Future coords on grid based on current swipe, will just be equal to current coords if not swiping
    public tileType type;//Type of tile, red or blue

    //Stuff for animating to new position
    private Vector3 desiredLocation;//Location that tile wants to animate to
    private Vector3 startToDesired;//Vector from start off animation to desired
    private Vector3 animStartPosition;//Start position of animation
    private float animStartTime;//Time at start of animation

    tileState state;//State that tile is in

    private TextMesh numberText;//Text for tile's number

    //Set up tile to match data of a given tile from level, called in board manager when loading tiles
    public void setUpTile(levelTile levTile, int i)
    {
        //Store basic tile data from level tile
        number = levTile.number;
        xPos = levTile.xPos;
        yPos = levTile.yPos;
        index = i;

        //Set future coords to current
        futureX = xPos;
        futureY = yPos;
    }

    void Awake()
    {
        BM = GameObject.Find("BoardManager").GetComponent<boardManager>();//Get board manager for scene, handles input and loading/building board and tile
    }

    // Use this for initialization
    void Start()
    {
        state = tileState.idle;//Default state is idle

        GameObject spriteObject = transform.Find("Sprite").gameObject;//Get child with sprite for resizing object to fit grid

        transform.position = GetModifiedDesired();//Set position to appropriate area
        
        desiredLocation = transform.position;//Desired location is same as pos by default

        numberText = GetComponentInChildren<TextMesh>();//Get text component for number from child

        //Set up number text to display right number and scale correctly so it doesn't get stretched
        numberText.text = number.ToString();
        numberText.gameObject.transform.localScale *= spriteObject.transform.localScale.x;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        //If desired location is different and not in swiping state, we should animate to desired
        if (transform.position != desiredLocation && state != tileState.swiping)
        {
            AnimateToDesired();
        }
    }

    //Partially slide tile to indicate swipe direction
    public void PartialSlide(Vector2 swipeVector)
    {
        //If currently animating, ignore swipes until done
        if (state == tileState.animating)
            return;

        state = tileState.swiping;//Set state to swiping

        //Reset future coords, modify them as needed
        futureX = xPos;
        futureY = yPos;

        //If swipe vector is longer horiz than vert and swipe wasn't being swiped vertically earlier
        if (BM.SwipeDir == SwipeDirection.horizontal)
        {
            //Determine if swipe is right or left
            if (swipeVector.x > 0)
            {
                futureX += 1;
            }
            else
            {
                futureX -= 1;
            } 
        }
        //If swipe vector is vert and wasn't being swiped horiz earlier
        else if(BM.SwipeDir == SwipeDirection.vertical)
        {
            //Determine if swipe is up or down
            if (swipeVector.y > 0)
            {
                futureY += 1;
            }
            else
            {
                futureY -= 1;
            }
        }

        //Other tile to check against
        tile otherTile;

        //Check if pushing against a tile of a different type, don't move if so
        for (int i = 0; i < BM.Tiles.Count; i++)
        {
            otherTile = BM.Tiles[i].GetComponent<tile>();//Get other tile from list

            //If other tile is a different type and has same future coords that means we are pushing into it and shouldn't combine, return early and reset future coords
            if (otherTile.index != index && otherTile.type != type && otherTile.futureX == futureX && otherTile.futureY == futureY && otherTile.state == tileState.idle)
            {
                futureX = xPos;
                futureY = yPos;

                state = tileState.idle;

                return;
            }
        }

        swipeVector /= Screen.dpi;//Convert swipe vector from pixels to world units for position

        Vector3 newPos = GetModifiedDesired();//Set new position to position of grid tile is currently at, then modify from there

        //If future x coords are different and are within bounds of grid, modify x pos based on swipe vector
        if (futureX != xPos && futureX >= 0 && futureX < BM.BoardWidth)
        {
            newPos.x += Mathf.Clamp(swipeVector.x, -(0.85f * BM.TileDist.x), (0.85f * BM.TileDist.x));  
        }
        //If y coords are different and within bounds, modify y pos
        else if(futureY != yPos && futureY >= 0 && futureY < BM.BoardHeight)
        { 
            newPos.y += Mathf.Clamp(swipeVector.y, -(0.85f * BM.TileDist.y), (0.85f * BM.TileDist.y));       
        }
        //If neither, reset state and return
        else
        {
            futureX = xPos;
            futureY = yPos;

            state = tileState.idle;

            return;
        }

        transform.position = newPos;//Set position to newly calculated swipe position

        //If the swipe is less than 1/5 of distance to next, reset future coords so that on release tiles will slide back into place rather than moving
        if(Mathf.Abs(swipeVector.x) < BM.TileDist.x * 0.2f && Mathf.Abs(swipeVector.y) < BM.TileDist.y * 0.2f)
        {
            futureX = xPos;
            futureY = yPos;
        }
    }

    //Move the tile on the grid based on keyboard input, DELETE THIS EVENTUALLY
    public bool MoveTile(int xChange, int yChange)
    {
        futureX = xPos + xChange;
        futureY = yPos + yChange;

        tile otherTile;

        for (int i = 0; i < BM.Tiles.Count; i++)
        {
            otherTile = BM.Tiles[i].GetComponent<tile>();

            if ((otherTile.futureX == futureX && otherTile.futureY == futureY && otherTile.type != type))
            {
                futureX = xPos;
                futureY = yPos;
                return false;
            }
        }

        state = tileState.idle;

        return MoveTile();

    }

    //Move the tile on the grid based on its future coords from swiping, returns whether the tile moved
    public bool MoveTile()
    {
        state = tileState.idle;//Reset state to idle

        //Whether tile should move
        bool moving = (futureX != xPos || futureY != yPos);

        //If should move, set coords to new ones
        if (moving)
        {
            xPos = futureX;
            yPos = futureY;
        }    

        //Set new desired location to animate to based on new coords
        desiredLocation = GetModifiedDesired();     

        return moving;//True if tile moved, false if didn't, BM can use to determine if a move should be added to counter
    }

    //Check if at same position as other tile and subtract their numbers
    private void subtractNumbers()
    {
        tile otherTile;//Other tile to check against

        //Loop through all active tiles
        for (int i = 0; i < BM.Tiles.Count; i++)
        {
            otherTile = BM.Tiles[i].GetComponent<tile>();

            //If other tile has some coords...
            if (otherTile.index != index && otherTile.xPos == xPos && otherTile.yPos == yPos)
            {
                number = Mathf.Abs(number - otherTile.number);//Set new number to result of subtraction from this tile's number and other number

                //Destroy the other tile and remove from list
                Destroy(BM.Tiles[i]);
                BM.Tiles.RemoveAt(i);

                break;//We found the tile so no need to continue, break loop here
            }
        }

        //If this tile's number is 0 destroy and remove it
        if (number == 0)
        {
            BM.Tiles.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        else//Otherwise, update the number text
        {
            numberText.text = number.ToString();
        }
    }

    //Animate to the desired position
    private void AnimateToDesired()
    {  
        //If not animating already, set state to animating and store start info
        if(state != tileState.animating)
        {
            state = tileState.animating;

            animStartPosition = transform.position;
            animStartTime = Time.time;
   
            startToDesired = desiredLocation - animStartPosition;//Vector from start pos to desired for reference during anim
        }

        //Calculate move speed for this frame, smooth step makes it ease in and out
        float moveSpeed = Mathf.SmoothStep(0, startToDesired.magnitude, (Time.time - animStartTime)/0.5f);
        //Calculate future position based on move speed
        Vector3 futurePos = transform.position + (startToDesired.normalized * moveSpeed);

        //If the future position will go past the desired location or is exactly on, end animation
        if (Vector3.Dot(desiredLocation - futurePos, startToDesired) < 0 || futurePos == desiredLocation)
        {
            transform.position = desiredLocation;//Snap position to desired

            state = tileState.idle;//Change state from animating

            subtractNumbers();//Subtract numbers with other tile if necessary
        }
        //Otherwise, change position as usual
        else
        {
            transform.position = futurePos;
        }

    }

    //Returns desired grid position with modified z for appropriate draw order
    private Vector3 GetModifiedDesired()
    {
        return BM.GridPosition[xPos, yPos] - new Vector3(0, 0, 1 + xPos + yPos);
    }

}
