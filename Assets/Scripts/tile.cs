using UnityEngine;
using System.Collections;

public enum tileType
{
    red,
    blue
}

public class tile : MonoBehaviour
{

    private boardManager BM;
    public int index;

    private Vector2 tileDist;

    public int number;
    public int xPos, yPos;
    public tileType type;

    private Vector3 desiredLocation;
    private Vector3 vecToDesired;
    private bool animating;
    private Vector3 animStartPosition;
    private float animStartTime;

    private bool partialSwiping;
    private const float minSwipeDist = 50f;

    private TextMesh numberText;
    private GameObject spriteObject;

    enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    SwipeDirection swipeDir;

    

    // Use this for initialization
    void Start()
    {
        swipeDir = SwipeDirection.None;

        spriteObject = transform.GetChild(0).gameObject;

        BM = GameObject.Find("BoardManagerGO").GetComponent<boardManager>();
        tileDist = new Vector2(BM.gridSpots[1, 0].transform.position.x - BM.gridSpots[0, 0].transform.position.x, BM.gridSpots[0, 1].transform.position.y - BM.gridSpots[0, 0].transform.position.y);

        GameObject gridSpace = GameObject.FindGameObjectWithTag("GridSpace");

        Vector3 gridSpaceWorldSize = gridSpace.GetComponent<MeshRenderer>().bounds.size;
        gridSpaceWorldSize.x *= spriteObject.transform.localScale.x / GetComponentInChildren<SpriteRenderer>().bounds.size.x;
        gridSpaceWorldSize.y *= spriteObject.transform.localScale.y / GetComponentInChildren<SpriteRenderer>().bounds.size.y;

        spriteObject.transform.localScale = new Vector3(gridSpaceWorldSize.x, gridSpaceWorldSize.y, 1f);

        transform.position = BM.gridSpots[xPos, yPos].transform.position;

        numberText = transform.GetComponentInChildren<TextMesh>();

        numberText.text = number.ToString();
        numberText.gameObject.transform.localScale = Vector3.one * spriteObject.transform.localScale.x;

        UpdateTile();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateTile();

    }

    public void PartialSlide(Vector2 swipeVector)
    {
        partialSwiping = true;

        Debug.Log(swipeVector);

        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y) && Mathf.Abs(swipeVector.x) > minSwipeDist && swipeDir != SwipeDirection.Up && swipeDir != SwipeDirection.Down)
        {
            if (swipeVector.x > 0)   
                swipeDir = SwipeDirection.Right;
            else
                swipeDir = SwipeDirection.Left; 
        }
        else if(Mathf.Abs(swipeVector.y) > minSwipeDist && swipeDir != SwipeDirection.Left && swipeDir != SwipeDirection.Right)
        {
            if (swipeVector.y > 0)
                swipeDir = SwipeDirection.Up;
            else
                swipeDir = SwipeDirection.Down;   
        }
        else
        {
            Debug.Log("Test");
            partialSwiping = false;
            transform.position = BM.gridSpots[xPos, yPos].transform.position;
            return;
        }

        swipeVector /= Screen.dpi;

        Vector3 newPos = BM.gridSpots[xPos,yPos].transform.position;

        if (swipeDir == SwipeDirection.Left || swipeDir == SwipeDirection.Right)
        {
            if ((swipeVector.x < 0 && xPos > 0) || (swipeVector.x > 0 && xPos < BM.BoardWidth - 1))
            {
                newPos.x += Mathf.Clamp(swipeVector.x, -(0.99f * tileDist.x), (0.99f * tileDist.x));
            }
        }
        else
        {
            if ((swipeVector.y < 0 && yPos > 0) || (swipeVector.y > 0 && yPos < BM.BoardHeight - 1))
            { 
                newPos.y += Mathf.Clamp(swipeVector.y, -(0.99f * tileDist.y), (0.99f * tileDist.y));
            }
        }

        transform.position = newPos;

    }

    public void MoveTile(int xChange, int yChange)
    {
        int newX = xPos + xChange;
        int newY = yPos + yChange;

        if (newX >= 0 && newX < BM.BoardWidth)
        {
            xPos = newX;
        }

        if (newY >= 0 && newY < BM.BoardHeight)
        {
            yPos = newY;
        }
    }

    public void MoveTile()
    {
        int newX = xPos;
        int newY = yPos;

        if(swipeDir == SwipeDirection.Right)
        {
            newX += 1;
        }
        else if(swipeDir == SwipeDirection.Left)
        {
            newX -= 1;
        }
        else if(swipeDir == SwipeDirection.Up)
        {
            newY += 1;
        }
        else
        {
            newY -= 1;
        }

        if (newX >= 0 && newX < BM.BoardWidth)
        {
            xPos = newX;
        }

        if (newY >= 0 && newY < BM.BoardHeight)
        {
            yPos = newY;
        }

        swipeDir = SwipeDirection.None;
        partialSwiping = false;

    }

    public void UpdateTile()
    {
        desiredLocation = BM.gridSpots[xPos, yPos].transform.position;

        if (transform.position != desiredLocation)
        {
            MoveToDesired();
        }
    }

    private void subtractNumbers()
    {
        tile otherTile;
        for (int i = 0; i < BM.Tiles.Count; i++)
        {
            otherTile = BM.Tiles[i].GetComponent<tile>();

            if (!(animating || otherTile.animating) && otherTile.index != index && otherTile.xPos == xPos && otherTile.yPos == yPos)
            {
                int newNumber = Mathf.Abs(number - otherTile.number);

                number = Mathf.Abs(newNumber);
                Destroy(BM.Tiles[i]);
                BM.Tiles.RemoveAt(i);

                break;
            }
        }

        if (number == 0)
        {
            BM.Tiles.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        else
        {
            numberText.text = number.ToString();
        }
    }

    private void MoveToDesired()
    {
        if (partialSwiping)
            return;        

        //If not animating already, set animating state to true and store start info
        if(!animating)
        {
            animating = true;
            animStartPosition = transform.position;
            animStartTime = Time.time;
        }
        
        //Update vector to desired
        vecToDesired = desiredLocation - animStartPosition;

        //Calculate move speed for this frame, smooth step makes it ease in and out
        float moveSpeed = Mathf.SmoothStep(0, vecToDesired.magnitude, (Time.time - animStartTime)/.3f);
        //Calculate future position based on move speed
        Vector3 futurePos = transform.position + (vecToDesired.normalized * moveSpeed);

        //If the future position will go past the desired location or is exactly on, set position to desired and end animation
        if (Vector3.Dot(desiredLocation - futurePos, vecToDesired) < 0 || futurePos == desiredLocation)
        {
            transform.position = desiredLocation;
            animating = false;

            subtractNumbers();
        }
        //Otherwise, change position as usual
        else
        {
            transform.position = futurePos;
        }

    }

}
