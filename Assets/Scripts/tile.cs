using UnityEngine;
using System.Collections;


public class tile : MonoBehaviour
{

    private boardManager BM;
    public int index;

    private Vector2 tileDist;

    public string type;
    public int number;
    public int xPos, yPos;

    private Vector3 desiredLocation;
    private bool animating;
    private Vector3 animStartPosition;
    private float animStartTime;

    private TextMesh numberText;

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

        BM = GameObject.Find("BoardManagerGO").GetComponent<boardManager>();
        tileDist = new Vector2(BM.gridSpots[1, 0].transform.position.x - BM.gridSpots[0, 0].transform.position.x, BM.gridSpots[0, 1].transform.position.y - BM.gridSpots[0, 0].transform.position.y);

        GameObject gridSpace = GameObject.FindGameObjectWithTag("GridSpace");

        Vector3 gridSpaceWorldSize = gridSpace.GetComponent<MeshRenderer>().bounds.size;
        gridSpaceWorldSize.x *= transform.localScale.x / GetComponent<SpriteRenderer>().bounds.size.x;
        gridSpaceWorldSize.y *= transform.localScale.y / GetComponent<SpriteRenderer>().bounds.size.y;

        transform.localScale = new Vector3(gridSpaceWorldSize.x, gridSpaceWorldSize.y, 1f);

        numberText = transform.GetChild(0).GetComponent<TextMesh>();

        numberText.text = number.ToString();

        UpdateTile();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
    }

    public void PartialSlide(Vector2 swipeVector)
    {
        swipeVector /= Screen.dpi;

        if (swipeDir == SwipeDirection.None)
        {
            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            {
                if (swipeVector.x > 0)   
                    swipeDir = SwipeDirection.Right;
                else
                    swipeDir = SwipeDirection.Left; 
            }
            else
            {
                if (swipeVector.y > 0)
                    swipeDir = SwipeDirection.Up;
                else
                    swipeDir = SwipeDirection.Down;   
            }
        }

        Vector3 newPos = BM.gridSpots[xPos,yPos].transform.position;

        if (swipeDir == SwipeDirection.Left || swipeDir == SwipeDirection.Right)
        {
            if ((swipeVector.x < 0 && xPos > 0) || (swipeVector.x > 0 && xPos < BM.BoardWidth - 1))
            {
                newPos.x += Mathf.Clamp(swipeVector.x, -(0.99f * tileDist.x), (0.99f * tileDist.x));
                transform.position = newPos;

            }
        }
        else
        {
            if ((swipeVector.y < 0 && yPos > 0) || (swipeVector.y > 0 && yPos < BM.BoardHeight - 1))
            { 
                newPos.y += Mathf.Clamp(swipeVector.y, -(0.99f * tileDist.y), (0.99f * tileDist.y));
                transform.position = newPos;
            }
        }

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

    }

    public void UpdateTile()
    {
        tile otherTile;
        for (int i = 0; i < BM.Tiles.Count; i++)
        {
            otherTile = BM.Tiles[i].GetComponent<tile>();

            if (otherTile.index != index && otherTile.xPos == xPos && otherTile.yPos == yPos)
            {
                number = Mathf.Abs(number - BM.Tiles[i].GetComponent<tile>().number);
                Destroy(BM.Tiles[i]);
                BM.Tiles.RemoveAt(i);
            }
        }

        //transform.position = BM.gridSpots[xPos, yPos].transform.position;
        desiredLocation = BM.gridSpots[xPos, yPos].transform.position;

        if (number == 0)
        {
            BM.Tiles.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        else
        {
            numberText.text = number.ToString();
        }

        MoveToDesired();
    }

    private void MoveToDesired()
    {
        //Return early if already at desired point
        if (transform.position == desiredLocation) {
            animating = false;
            return;
        }

        //Otherwise continue on...
        //If not animating already, set animating state to true
        if(!animating)
        {
            animating = true;
            animStartPosition = transform.position;
            animStartTime = Time.time;
        }
        

        Vector3 distToDes = desiredLocation - animStartPosition;

        float moveSpeed = Mathf.SmoothStep(0, 2f * distToDes.magnitude, (Time.time - animStartTime)/2f);

        //Vector3 positionChange = Vector3.Lerp(animStartPosition, desiredLocation, moveSpeed * Time.deltaTime);

        transform.position += distToDes.normalized * moveSpeed;

        if((desiredLocation - transform.position).sqrMagnitude < 0.1f)
        {
            transform.position = desiredLocation;
            animating = false;
        }

    }

}
