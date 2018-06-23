using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantBallScript : MonoBehaviour {

    private float power;

	// Use this for initialization
	void Start () {
        power = 1.2f;

	}
	
	// Update is called once per frame
	void Update () {


        transform.position += transform.forward * Time.deltaTime;

        

	}

    void OnCollisionEnter(Collision col)
    {

        

        if (col.gameObject.tag == "Player")
        {
            Vector3 playerPosition = col.transform.position;
            Rigidbody playerRigidBody = col.gameObject.GetComponent<Rigidbody>();

            playerRigidBody.AddExplosionForce( (power/2f) * 15, new Vector3(transform.position.x, transform.position.y - 2, transform.position.z), 20f);
            Object.Destroy(transform.gameObject);
        }
    }
}
