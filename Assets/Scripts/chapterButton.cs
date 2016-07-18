using UnityEngine;
using UnityEngine.UI;

public class chapterButton : MonoBehaviour {

    public int chapterNumber;

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(delegate() { GameObject.Find("LevelManager").GetComponent<levelManager>().loadChapter(chapterNumber); });
	}
	
}
