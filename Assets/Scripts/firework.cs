using UnityEngine;
using System.Collections;

public class firework : MonoBehaviour {


	// Use this for initialization
	void Awake () {
        Camera.main.GetComponent<sceneTransition>().shake();
        
        Destroy(gameObject,1f);
    }
}
