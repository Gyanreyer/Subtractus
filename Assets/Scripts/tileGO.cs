using UnityEngine;
using System.Collections;


public class tileGO : MonoBehaviour
{

    private boardManager BM;
    public int index;

    public string type;
    public int number;
    public int xPos, yPos;

    private TextMesh numberText;


    // Use this for initialization
    void Start()
    {

        BM = GameObject.Find("BoardManagerGO").GetComponent<boardManager>();

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
        UpdateTile();
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

    void UpdateTile()
    {
        tileGO otherTile;
        for (int i = 0; i < BM.Tiles.Count; i++)
        {
            otherTile = BM.Tiles[i].GetComponent<tileGO>();

            if (otherTile.index != index && otherTile.xPos == xPos && otherTile.yPos == yPos)
            {
                Debug.Log("Destroying");

                number = Mathf.Abs(number - BM.Tiles[i].GetComponent<tileGO>().number);
                Destroy(BM.Tiles[i]);
                BM.Tiles.RemoveAt(i);
            }
        }

        transform.position = BM.gridSpots[xPos, yPos].transform.position;

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

}
