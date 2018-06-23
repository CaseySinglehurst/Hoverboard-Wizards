using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsSingleton : MonoBehaviour {

    public static SkinsSingleton instance = null;

    public GameObject[] hoverBoards = new GameObject[3];
    public GameObject[] characters = new GameObject[3];

    public GameObject playerBoard, playerCharacter;

    public int stocks, players;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }
        // Use this for initialization
        void Start () {
        playerBoard = hoverBoards[0];
        playerCharacter = characters[0];
	}
	
	
}
