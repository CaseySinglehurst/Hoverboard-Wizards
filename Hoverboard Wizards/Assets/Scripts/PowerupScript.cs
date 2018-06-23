using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupScript : MonoBehaviour {

    private float timer;
    private bool timerStarted = false;
    private Image timerImage;
    private CapsuleCollider collider;
    public int powerupNumber;

    
    

    // Use this for initialization
    void Start () {
        timer = 1;
        timerImage = GetComponentInChildren<Image>();
        timerImage.fillAmount = timer;
        collider = GetComponent<CapsuleCollider>();

        powerupNumber = Random.Range((int)1, (int)5);
        
	}
	
	// Update is called once per frame
	void Update () {

        if(transform.position.y < 50)
        {
            timerStarted = true;
        }

        if (timerStarted)
        {
            timer -= Time.deltaTime / 10;
        }
        timerImage.fillAmount = timer;

        if(timer < 0)
        {
            
            collider.isTrigger = true;
        }

        if (transform.position.y < -30)
        {
            Destroy(transform.gameObject);
        }
    }
}
