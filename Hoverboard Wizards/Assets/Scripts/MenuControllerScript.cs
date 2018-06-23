using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuControllerScript : MonoBehaviour {

    public InputField stockInput, playerInput;
    public Text errorMessage;
    public int stocks, players;
    private bool worked = false;
    public GameObject star, circle, square, baseCharacter,magicCharacter,gemCharacter, baseBoard, magicBoard, gemBoard;
    private string level;
    [HideInInspector]
    public int playerBoard, playerCharacter;

    public float inUsePosition, notInUsePosition;
    private float mainMenuTargetX, characterMenuTargetX, playMenuTargetX;
    private float cameraTargetX;
    public RectTransform mainMenu, characterMenu, playMenu;

    private float menuSpeed = 5f;

    void Start()
    {
        SquareSelected();
        stocks = 555;
        players = 555;
        GoToMainMenu();
    }

    void Update()
    {
        mainMenu.position = (new Vector3(Mathf.Lerp(mainMenu.transform.position.x, mainMenuTargetX + (Screen.width/2), menuSpeed * Time.deltaTime), Screen.height/2, 0f));
        characterMenu.position = (new Vector3( Mathf.Lerp(characterMenu.transform.position.x, characterMenuTargetX + (Screen.width / 2), menuSpeed * Time.deltaTime) , Screen.height / 2, 0f));
        playMenu.position = (new Vector3(Mathf.Lerp(playMenu.transform.position.x, playMenuTargetX + (Screen.width / 2), menuSpeed * Time.deltaTime), Screen.height / 2, 0f));
    }
    
    void TryToStart () {
        


        try
        {
            stocks = int.Parse(stockInput.text);
            players = int.Parse(playerInput.text);
            worked = true;
        }
        catch
        {
            worked = false;
            errorMessage.text = "wrong text format";
            
        }

        if (worked) {
            if (players <9 && players >1)
            {
                SkinsSingleton.instance.stocks = stocks;
                SkinsSingleton.instance.players = players;
                SceneManager.LoadScene(level);
            }
            else
            {
                errorMessage.text = "Players is: " + players + ". Players must be between 2 and 8!";
                this.enabled = false;
            }
        }
    }
	
    void SquareSelected()
    {
        square.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        circle.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        star.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        level = "SquareLevel";
    }
    void CircleSelected()
    {
        square.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        circle.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        star.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        level = "CircleLevel";
    }
    void StarSelected()
    {
        square.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        circle.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        star.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        
        level = "StarLevel";
    }
    
    public void GoToCharacterSelectMenu()
    {
        mainMenuTargetX = notInUsePosition;
        characterMenuTargetX = inUsePosition;
        playMenuTargetX = notInUsePosition;
    }
    public void GoToMainMenu()
    {
        mainMenuTargetX = inUsePosition;
        characterMenuTargetX = notInUsePosition;
        playMenuTargetX = notInUsePosition;
    }
    public void GoToPlayMenu()
    {
        mainMenuTargetX = notInUsePosition;
        characterMenuTargetX = notInUsePosition;
        playMenuTargetX = inUsePosition;
    }
}
