using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBallScript : MonoBehaviour {

    public float power;
    private float powerBleed = 0.006f, maxSize = 0.6f, minSize = 0.2f, currentScale, maxPower = 0.11f;

	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {


        power -= powerBleed * Time.deltaTime;
        currentScale = (minSize + ((power/maxPower) * (maxSize - minSize)));
        transform.localScale = (new Vector3(currentScale,currentScale,currentScale));

        if (power <= 0)
        {
            Object.Destroy(transform.gameObject);
        }

	}

    void OnCollisionEnter(Collision col)
    {

        

        if (col.gameObject.tag == "Player")
        {
            Vector3 playerPosition = col.transform.position;
            Rigidbody playerRigidBody = col.gameObject.GetComponent<Rigidbody>();

            playerRigidBody.AddExplosionForce( (power/2f) * 15, transform.position, currentScale);
            Object.Destroy(transform.gameObject);
        }
    }
}
