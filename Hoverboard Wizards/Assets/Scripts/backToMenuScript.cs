using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class backToMenuScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Destroy(GameObject.FindGameObjectWithTag("MenuController"));
        SceneManager.LoadScene("TitleScreen");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
