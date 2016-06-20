using UnityEngine;
using UnityEngine.UI;

//Script for level selection buttons
public class levelSelectButton : MonoBehaviour {

    public int levelNumber;//Level number that it will load

	// Use this for initialization
	void Start () {
        //Set on click event to call level manager's loadLevel method with this button's level index
        GetComponent<Button>().onClick.AddListener(delegate() { GameObject.Find("LevelManager").GetComponent<levelManager>().loadLevel(levelNumber-1); });

        GetComponentInChildren<TextMesh>().text = levelNumber.ToString();//Set text to reflect level number
	}
	
}
