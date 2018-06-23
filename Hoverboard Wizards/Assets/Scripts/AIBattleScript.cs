using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AIBattleScript : MonoBehaviour {
    
    public GameObject battleManager;
    private bool isFiring = false;
    private GameObject currentTarget;
    private Rigidbody thisRigid;
    private float newTargetTimer = 0f;

    int obstacleMask, shieldMask;
    LineRenderer energyBolt;
    public float power = .05f, maxPower = 0.1f, rechargeRate = 0.1f;
    public Transform boltTip, boltEnd;
    float beamWidth;
    public GameObject magicBubble;
    public int lives = 100;

    private int boardNum, characterNum;
    private GameObject skins, board, character;

    private GameObject shield;
    private float shieldCooldown = 15;
    private float currentShieldCooldown = 0;

    private GameObject steel;
    private float steelCooldown = 15;
    private float currentSteelCooldown = 0;

    private GameObject hole;
    private float holeCooldown = 15;
    private float currentHoleCooldown = 0;

    private Transform ballSpawn;
    public GameObject ball;

    private enum powerups { none, shield, steel, hole, ball };
    private powerups powerup = powerups.none;

    private bool powerUsed = false;
    private float powerupUseTimer = 15;
    // Use this for initialization
    void Start () {
        thisRigid = GetComponent<Rigidbody>();
        obstacleMask = LayerMask.GetMask("Obstacle");
        shieldMask = LayerMask.GetMask("Shield");
        energyBolt = GetComponent<LineRenderer>();
        energyBolt.positionCount = 0;

        boardNum = Random.Range(0, 3);
        characterNum = Random.Range(0, 3);

        board = Instantiate(SkinsSingleton.instance.hoverBoards[boardNum], this.transform.position - new Vector3(0, 0.5f, 0), Quaternion.identity, this.transform);
        character = Instantiate(SkinsSingleton.instance.characters[characterNum], this.transform.position, Quaternion.Euler(-90,0,0), this.transform);

        shield = transform.Find("Shield").gameObject;
        steel = transform.Find("Steel").gameObject;
        hole = transform.Find("Hole").gameObject;

        shield.GetComponent<BoxCollider>().enabled = false;
        shield.GetComponent<MeshRenderer>().enabled = false;

        steel.GetComponent<CapsuleCollider>().enabled = false;
        steel.GetComponent<MeshRenderer>().enabled = false;

        hole.GetComponent<SphereCollider>().enabled = false;
        hole.GetComponent<MeshRenderer>().enabled = false;

        ballSpawn = transform.Find("GiantBubbleSpawn").gameObject.transform;

        

    }
	
	// Update is called once per frame
	void Update () {

        ManagePowerups();
        
        beamWidth = power * 20;

        Transform goForPowerup = FindPowerup();


        if (powerup == powerups.steel && powerUsed)
        {
            fireAtNearest();
        }
        else if (ShouldISave())
        {
            EdgeSave();
        }
        else if (powerup == powerups.shield && powerUsed)
        {
            BehaviourShield();
        }

        else if (goForPowerup != null)
        {
            
            GoToPowerup(goForPowerup);
        }
        else
        {
            fireAtNearest();
        }
        ManagePower();

        powerupUseTimer -= Time.deltaTime;

        if (powerupUseTimer < 0)
        {
            UsePowerup();
            powerupUseTimer = 15;
            
        }

        if (transform.position.y < -30)
        {
            Respawn();
        }

    }

    void Respawn()
    {
        BattleManagerScript battleScript = battleManager.GetComponent<BattleManagerScript>();
        lives -= 1;
        thisRigid.velocity = new Vector3(0, 0, 0) ;
        transform.position = battleScript.bestSpawn;
    }

    bool ShouldISave()
    {
        if(Vector3.Distance(transform.position,new Vector3(0,transform.position.y,0)) > 7f)
        {
            return true;
        }

        return false;
    }

    void EdgeSave()
    {
        Vector3 middle = new Vector3(0, 0, 0);
        Vector3 middleDirection = middle - transform.position;

        // Ensure the vector is entirely along the floor plane.
        middleDirection.y = 0f;

        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation(middleDirection * -1);
        

        // Set the player's rotation to this new rotation.
        thisRigid.MoveRotation(Quaternion.Lerp(transform.rotation, newRotation, 10f * Time.deltaTime));

        float angle = 10f;
        if (Vector3.Angle(transform.forward, middleDirection * -1) < angle)
        {


            isFiring = true;
        }
        else
        {
            isFiring = false;
        }

    }

    void fireAtNearest()
    {
        newTargetTimer -= Time.deltaTime;
        if (battleManager)
        {
            if (newTargetTimer < 0 || currentTarget == null)
            {
                currentTarget = findNearestPlayerPosition();
                newTargetTimer = Random.Range(3f, 10f);

            }
            else
            {

                Vector3 nearestDirection = currentTarget.transform.position - transform.position;

                // Ensure the vector is entirely along the floor plane.
                nearestDirection.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                Quaternion newRotation = Quaternion.LookRotation(nearestDirection);

                // Set the player's rotation to this new rotation.
                thisRigid.MoveRotation(Quaternion.Lerp(transform.rotation, newRotation, 5f * Time.deltaTime));

                float angle = 4f;
                if (Vector3.Angle(transform.forward, currentTarget.transform.position - transform.position) < angle)
                {


                    isFiring = true;
                }
                else
                {
                    isFiring = false;
                }



            }
        }
    }

    void FixedUpdate()
    {
        if (isFiring)
        {
            energyBolt.positionCount = 2;
            Fire();
        }
        else
        {
            energyBolt.positionCount = 0;
            
        }
    }
    
    GameObject findNearestPlayerPosition()
    {
        GameObject currentNearest;
        float currentNearestDistance;
        BattleManagerScript battleScript = battleManager.GetComponent<BattleManagerScript>();

        currentNearest = battleScript.players[0];
        currentNearestDistance = 100000f;

        for (int i = 0; i < battleScript.playersNumber; i++)
        {
            if (battleScript.players[i] != null) { 
                if (FindDistance(transform, battleScript.players[i].transform) < currentNearestDistance && battleScript.players[i].transform != transform)
                {
                    currentNearest = battleScript.players[i];
                    currentNearestDistance = FindDistance(transform, battleScript.players[i].transform);
                }
            }
        }

        return currentNearest;
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

    void ManagePower()
    {
        if (power < maxPower)
        {
            power += rechargeRate * Time.deltaTime;
        }
        if (power < 0)
        {
            power = 0;
        }
    }

    void Fire()
    {
        energyBolt.positionCount = 2;

        RaycastHit hit, hit2;

        energyBolt.SetPosition(0, boltTip.position);
        if (Physics.Linecast(boltTip.position, boltEnd.position, out hit, shieldMask))
        {
            energyBolt.SetPosition(1, hit.point);
            energyBolt.positionCount = 3;

            if (Physics.Linecast(hit.point, Vector3.Reflect(boltEnd.position - boltTip.position, hit.normal), out hit2, obstacleMask) || Physics.Linecast(hit.point, Vector3.Reflect(boltEnd.position - boltTip.position, hit.normal), out hit2, shieldMask))
            {
                energyBolt.SetPosition(2, hit2.point);
                if (hit2.transform.tag == "Player" || hit2.transform.tag == "Powerup")
                {
                    Vector3 direction = hit2.transform.position - transform.position;

                    hit2.transform.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(direction) * power);

                }
            }
            else
            {
                energyBolt.SetPosition(2, (transform.position + (Vector3.Reflect(boltEnd.position - boltTip.position, hit.normal) * 50f)));
            }

        }
        else if (Physics.Linecast(boltTip.position, boltEnd.position, out hit, obstacleMask))
        {
            energyBolt.SetPosition(1, hit.point);

            if (hit.transform.tag == "Player" || hit.transform.tag == "Powerup")
            {
                Vector3 direction = hit.transform.position - transform.position;

                hit.transform.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(direction) * power);

            }
        }
        else
        {
            energyBolt.SetPosition(1, boltEnd.position);
        }




        energyBolt.startWidth = beamWidth;
        energyBolt.endWidth = beamWidth;
        thisRigid.AddRelativeForce(Vector3.forward * -1 * power);
        power -= power * Time.deltaTime;
        CheckIfSpawnBubble();
    }

    void CheckIfSpawnBubble()
    {
        float spawnChance = Random.Range(0f, 1f) + (power * 1.2f);


        if (spawnChance > 0.99f)
        {

            GameObject bubble = Instantiate(magicBubble, boltTip.position, Quaternion.identity);
            bubble.GetComponent<MagicBallScript>().power = power;
            bubble.GetComponent<Rigidbody>().velocity = Random.onUnitSphere * Random.Range(1f, 10f);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "BlackHole" && col.gameObject.GetComponentInParent<Transform>() != this.transform)
        {
            thisRigid.AddForce((col.transform.position - transform.position).normalized * Mathf.Clamp((100000f / Vector3.Distance(transform.position, col.transform.position)), 0, 1) * Time.smoothDeltaTime);
        }
    }

    void ManageShield()
    {
        currentShieldCooldown -= Time.deltaTime;
        if (currentShieldCooldown < 0 && shield.GetComponent<BoxCollider>().enabled == true)
        {
            shield.GetComponent<BoxCollider>().enabled = false;
            shield.GetComponent<MeshRenderer>().enabled = false;
            powerup = powerups.none;
            powerUsed = false;
        }

        if (currentShieldCooldown > 0 && shield.GetComponent<BoxCollider>().enabled == false)
        {
            shield.GetComponent<BoxCollider>().enabled = true;
            shield.GetComponent<MeshRenderer>().enabled = true;
        }



    }

    void ManageSteel()
    {
        currentSteelCooldown -= Time.deltaTime;
        if (currentSteelCooldown < 0 && steel.GetComponent<CapsuleCollider>().enabled == true)
        {
            steel.GetComponent<CapsuleCollider>().enabled = false;
            steel.GetComponent<MeshRenderer>().enabled = false;
            powerup = powerups.none;
            thisRigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            powerUsed = false;
        }

        if (currentSteelCooldown > 0 && steel.GetComponent<CapsuleCollider>().enabled == false)
        {
            steel.GetComponent<CapsuleCollider>().enabled = true;
            steel.GetComponent<MeshRenderer>().enabled = true;
            thisRigid.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }



    }

    void ManageHole()
    {
        currentHoleCooldown -= Time.deltaTime;
        if (currentHoleCooldown < 0 && hole.GetComponent<SphereCollider>().enabled == true)
        {
            hole.GetComponent<SphereCollider>().enabled = false;
            hole.GetComponent<MeshRenderer>().enabled = false;
            powerup = powerups.none;
            powerUsed = false;

        }

        if (currentHoleCooldown > 0 && hole.GetComponent<SphereCollider>().enabled == false)
        {
            hole.GetComponent<SphereCollider>().enabled = true;
            hole.GetComponent<MeshRenderer>().enabled = true;

        }



    }

    void ManagePowerups()
    {
        ManageShield();
        ManageSteel();
        ManageHole();
    }

    void UsePowerup()
    {
        if(powerup != powerups.none && !powerUsed)
        {
            switch (powerup)
            {
                case powerups.none: break;
                case powerups.shield: currentShieldCooldown = shieldCooldown;  powerUsed = true; break;
                case powerups.steel: currentSteelCooldown = steelCooldown;  powerUsed = true; break;
                case powerups.hole: currentHoleCooldown = holeCooldown;  powerUsed = true; break;
                case powerups.ball: Instantiate(ball, ballSpawn.position, ballSpawn.rotation); powerup = powerups.none; powerUsed = false; break;
                default: break;
            }
        }
    }

    void BehaviourShield()
    {
        newTargetTimer -= Time.deltaTime;
        if (battleManager)
        {
            if (newTargetTimer < 0 || currentTarget == null)
            {
                currentTarget = findNearestPlayerPosition();
                newTargetTimer = Random.Range(3f, 10f);

            }
            else
            {

                Vector3 nearestDirection = currentTarget.transform.position - transform.position;

                // Ensure the vector is entirely along the floor plane.
                nearestDirection.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                Quaternion newRotation = Quaternion.LookRotation(nearestDirection);

                // Set the player's rotation to this new rotation.
                thisRigid.MoveRotation(Quaternion.Lerp(transform.rotation, newRotation, 5f * Time.deltaTime));
            }
        }
    }

    Transform FindPowerup()
    {
        GameObject[] powerups = GameObject.FindGameObjectsWithTag("Powerup");
        
        for(int x = 0; x < powerups.Length; x++)
        {
            
            if (FindDistance(transform, powerups[x].transform) < 15f )
            {
                
                return powerups[x].transform;
                
            }
        }
        
        return null;
    }

    void GoToPowerup(Transform powerup)
    {

        
        Vector3 nearestDirection = powerup.position - transform.position;

        // Ensure the vector is entirely along the floor plane.
        nearestDirection.y = 0f;

        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation(nearestDirection);

        Vector3 rot = newRotation.eulerAngles;
        rot = new Vector3(rot.x, rot.y + 180, rot.z);
        newRotation = Quaternion.Euler(rot);

        // Set the player's rotation to this new rotation.
        thisRigid.MoveRotation(Quaternion.Lerp(transform.rotation, newRotation, 5f * Time.deltaTime));

        float angle = 4f;
        
       
        if ((Vector3.Angle(transform.forward, powerup.position - transform.position)) - 180 < angle)
        {


            isFiring = true;
        }
        else
        {
            isFiring = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Powerup")
        {

            if (powerup == powerups.none)
            {
                powerUsed = false;
                int pickupNumber = col.gameObject.GetComponent<PowerupScript>().powerupNumber;

                switch (pickupNumber)
                {
                    case 1: powerup = powerups.shield; break;
                    case 2: powerup = powerups.steel; break;
                    case 3: powerup = powerups.hole; break;
                    case 4: powerup = powerups.ball; break;
                    default: powerup = powerups.shield; break;

                }

            }
            Destroy(col.gameObject);
        }

    }
}
