using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManagerScript : MonoBehaviour {
    [SerializeField]
    
    public Text winText;

    public Transform[] spawnPoints = new Transform[8];
    public Text[] livesText = new Text[8];

    public GameObject player;
    public GameObject enemy;
    private GameObject menuController;

    public GameObject[] players ;
    public Vector3 bestSpawn = new Vector3(0, 0, 0);

    public int stock = 3;
    public int playersNumber;
    public int playersLeft;

    public Button backToMenuButton;

    public GameObject powerUp;
    public GameObject skins;
    
    

	// Use this for initialization
	void Start () {
        skins = GameObject.FindGameObjectWithTag("SkinHolder");
        menuController = GameObject.FindGameObjectWithTag("MenuController");
        stock = SkinsSingleton.instance.stocks;
        playersNumber = SkinsSingleton.instance.players;
        players = new GameObject[playersNumber];
        playersLeft = playersNumber;

        winText.text = "";
        backToMenuButton.enabled = false;
        backToMenuButton.GetComponent<Image>().enabled = false;
        backToMenuButton.GetComponentInChildren<Text>().enabled = false;
        players[0] = Instantiate(player, spawnPoints[0].position, Quaternion.identity);
        players[0].GetComponent<RotateToMouse>().battleManager = transform.gameObject;
        players[0].GetComponent<RotateToMouse>().lives = stock;

        for (int i = 1; i < playersNumber; i++)
        {
            players[i] = Instantiate(enemy, spawnPoints[i].position, Quaternion.identity);
            players[i].GetComponent<AIBattleScript>().battleManager = transform.gameObject;
            players[i].GetComponent<AIBattleScript>().lives = stock;
        }


	}
	
	// Update is called once per frame
	void Update () {
        bestSpawn = BestSpawnPoint();
        CheckIfDefeated();
        CheckPowerup();
        if (players[0] != null)
        {
            livesText[0].text = "Player 1 Lives: " + players[0].GetComponent<RotateToMouse>().lives;
        }
        for (int i = 1;i< playersNumber; i++)
        {
            if (players[i] != null)
            {
                livesText[i].text = "Bot " + (i) + " Lives: " + players[i].GetComponent<AIBattleScript>().lives;
            }
        }
        if (playersLeft == 1)
        {
            SetWinText();
        }
		
	}

    void CheckPowerup()
    {
        int randomNumber = Random.Range(0, 1000);
        if (randomNumber == 1)
        {
            Instantiate(powerUp, new Vector3(Random.Range(-20f,20f), 300, Random.Range(-20f,20f)), Quaternion.identity);
        }

    }

    void SetWinText()
    {
        playersLeft = 0;
        backToMenuButton.enabled = true;
        backToMenuButton.GetComponent<Image>().enabled = true;
        backToMenuButton.GetComponentInChildren<Text>().enabled = true;
        for (int i = 0; i< playersNumber; i++)
        {
            if (players[i] != null)
            {
                Destroy(players[i]);
                if(i == 0)
                {
                    winText.text = "Player Wins!";
                }
                else
                {
                    winText.text = "Bot " + i + " Wins!";
                }
            }
        }
    }

    void CheckIfDefeated()
    {
        if (players[0] != null)
        {
            if (players[0].GetComponent<RotateToMouse>().lives < 1)
            {
                playersLeft -= 1;
                Destroy(players[0]);
                players[0] = null;
                livesText[0].text = "PLAYER 1 DEFEATED";

            }
        }

        for (int i = 1; i < playersNumber; i++)
        {
            if (players[i] != null)
            {
                if (players[i].GetComponent<AIBattleScript>().lives < 1)
                {
                    playersLeft -= 1;
                    Destroy(players[i]);
                    players[i] = null;
                    livesText[i].text = "BOT " + i + " DEFEATED";

                }
            }
        }
    }

    Vector3 BestSpawnPoint()
    {
        float thisSpawnDistance = 0;
        float bestSpawnDistance = 0;
        Transform bestSpawn = transform; 

        foreach (Transform point in spawnPoints)
        {
            thisSpawnDistance = findNearestPlayerDistance(point);

            if (thisSpawnDistance > bestSpawnDistance)
            {
                bestSpawnDistance = thisSpawnDistance;
                bestSpawn = point;
            }


        }
            

        return bestSpawn.position;
    }

    float findNearestPlayerDistance(Transform spawnPoint)
    {

        float currentNearestDistance;
        

        
        currentNearestDistance = 100000f;

        for (int i = 0; i < playersNumber; i++)
        {
            if (players[i] != null)
            {
                if (FindDistance(spawnPoint.transform, players[i].transform) < currentNearestDistance)
                {
                   
                    currentNearestDistance = FindDistance(spawnPoint.transform, players[i].transform);
                }
            }
        }

        return currentNearestDistance;
    }

    float FindDistance(Transform a, Transform b)
    {
        float xDistance = Mathf.Abs(a.position.x - b.position.x);
        float zDistance = Mathf.Abs(a.position.z - b.position.z);
        xDistance = Mathf.Pow(xDistance, 2);
        zDistance = Mathf.Pow(zDistance, 2);
        float distance = Mathf.Sqrt(xDistance + zDistance);
        return distance;


    }

    
}
