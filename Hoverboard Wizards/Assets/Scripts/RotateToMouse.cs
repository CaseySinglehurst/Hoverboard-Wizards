using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateToMouse : MonoBehaviour
{

    int floorMask, obstacleMask, shieldMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    float camRayLength = 100f;          // The length of the ray from the camera into the scene.
    public Rigidbody playerRigidbody;          // Reference to the player's rigidbody.

    float beamWidth;

    public float power = .05f, maxPower = 0.1f, rechargeRate = 0.1f;

    private LineRenderer energyBolt;
    public Transform boltTip, boltEnd;

    public ParticleSystem part1Wand, part2Wand;

    public GameObject magicBubble;

    private GameObject energyBarObject;
    public Sprite shieldUI, shieldBG, steelUI, steelBG, holeUI, holeBG;
    public Image energyBar, PowerupTimerUI, PowerupTimerBG;

    public int lives = 100;

    public GameObject battleManager;

    private enum powerups {none, shield, steel, hole, ball };
    private powerups powerup = powerups.none;

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

    public bool powerUsed = false;

    private GameObject skins,menuController, character, board;


    void Start()
    {
        // Create a layer mask for the floor layer.
        floorMask = LayerMask.GetMask("Floor");
        obstacleMask = LayerMask.GetMask("Obstacle");
        shieldMask = LayerMask.GetMask("Shield");

        part1Wand.Stop();
        part2Wand.Stop();

        // Set up references.
        playerRigidbody = GetComponent<Rigidbody>();

        energyBolt = GetComponent<LineRenderer>();
        energyBolt.positionCount = 0;

        energyBarObject = GameObject.FindGameObjectWithTag("EnergyBar");
        energyBar = energyBarObject.GetComponent<Image>();

        PowerupTimerUI = GameObject.FindGameObjectWithTag("PowerupTimer").GetComponent<Image>();
        PowerupTimerBG = GameObject.FindGameObjectWithTag("PowerupTimerBG").GetComponent<Image>();

        PowerupTimerUI.enabled = false;
        PowerupTimerBG.enabled = false;

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

        skins = GameObject.FindGameObjectWithTag("SkinHolder");
        board = Instantiate(SkinsSingleton.instance.playerBoard,this.transform.position - new Vector3(0,0.5f,0),Quaternion.identity,this.transform);
        character = Instantiate(SkinsSingleton.instance.playerCharacter, this.transform.position, Quaternion.Euler(-90,0,0), this.transform);

        //debugging

    }

    void Update()
    {
        ManageUI();
        Turning();
        ManagePower();
        ManagePowerups();
        ManageWandParticles();
        

        beamWidth = power * 20;

        if (transform.position.y < -30)
        {
            Respawn();
        }



    }

    void ManageWandParticles()
    {
        if (powerup != powerups.none && part1Wand.isStopped)
        {


            part1Wand.Play();
            part2Wand.Play();
        }
        if (powerup == powerups.none && currentShieldCooldown <0 && currentSteelCooldown < 0 && currentHoleCooldown < 0 && !part1Wand.isStopped)
        {
            part1Wand.Stop();
            part2Wand.Stop();
        }
    }

    void ManagePowerups()
    {
        ManageShield();
        ManageSteel();
        ManageHole();
    }

    void FixedUpdate()
    {
        if (Input.GetButton("Fire1"))
        {
            
            FireBolt();
        }
        else
        {
            energyBolt.positionCount = 0;
           
        }

        if (Input.GetButton("Fire2") && !powerUsed )
        {
            switch (powerup)
            {
                case powerups.none: break;
                case powerups.shield: currentShieldCooldown = shieldCooldown; PowerupTimerUI.sprite = shieldUI; PowerupTimerBG.sprite = shieldBG; PowerupTimerUI.enabled = true; PowerupTimerBG.enabled = true; powerUsed = true;  break;
                case powerups.steel: currentSteelCooldown = steelCooldown; PowerupTimerUI.sprite = steelUI; PowerupTimerBG.sprite = steelBG; PowerupTimerUI.enabled = true; PowerupTimerBG.enabled = true; powerUsed = true; break;
                case powerups.hole: currentHoleCooldown = holeCooldown; PowerupTimerUI.sprite = holeUI; PowerupTimerBG.sprite = holeBG; PowerupTimerUI.enabled = true; PowerupTimerBG.enabled = true; powerUsed = true; break;
                case powerups.ball: Instantiate(ball, ballSpawn.position, ballSpawn.rotation); powerup = powerups.none; powerUsed = false; break;
                default: break;
            }
            
        }
    }

    void Respawn()
    {
        BattleManagerScript battleScript = battleManager.GetComponent<BattleManagerScript>();
        lives -= 1;
        playerRigidbody.velocity = new Vector3(0, 0, 0);
        transform.position = battleScript.bestSpawn;
    }

    void ManageUI()
    {
        energyBar.fillAmount = power / maxPower;

        switch (powerup)
        {
            case powerups.none:if (PowerupTimerUI.enabled) { PowerupTimerUI.enabled = false; PowerupTimerBG.enabled = false; } break;
            case powerups.shield:  PowerupTimerUI.fillAmount = currentShieldCooldown/shieldCooldown; break;
            case powerups.steel:   PowerupTimerUI.fillAmount = currentSteelCooldown / steelCooldown; break;
            case powerups.hole:     PowerupTimerUI.fillAmount = currentHoleCooldown / holeCooldown; break;
            case powerups.ball: break;
            default: break;
        }


    }

    void Turning()
    {
        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            // Create a vector from the player to the point on the floor the raycast from the mouse hit.
            Vector3 playerToMouse = floorHit.point - transform.position;

            // Ensure the vector is entirely along the floor plane.
            playerToMouse.y = 0f;

            // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
            
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

            // Set the player's rotation to this new rotation.
            transform.rotation = (newRotation);

            Debug.DrawLine(transform.position, floorHit.point);
        }
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

    void FireBolt()
    {
        energyBolt.positionCount = 2;

        RaycastHit hit, hit2;

        energyBolt.SetPosition(0, boltTip.position);
        if (Physics.Linecast(boltTip.position, boltEnd.position, out hit, shieldMask))
        {
            energyBolt.SetPosition(1, hit.point);
            energyBolt.positionCount = 3;

            if (Physics.Linecast(hit.point, Vector3.Reflect( boltEnd.position- boltTip.position,hit.normal), out hit2, obstacleMask) || Physics.Linecast(hit.point, Vector3.Reflect(boltEnd.position - boltTip.position, hit.normal), out hit2, shieldMask))
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
        playerRigidbody.AddRelativeForce(Vector3.forward * -1 * power);
        power -= power * Time.deltaTime;
        CheckIfSpawnBubble();



    }

    void CheckIfSpawnBubble()
    {
        float spawnChance = Random.Range(0f, 1f) + (power*1.2f);


        if (spawnChance > 0.99f)
        {

            GameObject bubble = Instantiate(magicBubble, boltTip.position, Quaternion.identity);
            bubble.GetComponent<MagicBallScript>().power = power;
            bubble.GetComponent<Rigidbody>().velocity = Random.onUnitSphere * Random.Range(1f, 10f);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Powerup")
        {

            if (powerup == powerups.none){
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

    void OnTriggerStay(Collider col)
    {
        
            if (col.gameObject.tag == "BlackHole" && col.gameObject.GetComponentInParent<Transform>() != this.transform)
            {
                playerRigidbody.AddForce((col.transform.position - transform.position).normalized * Mathf.Clamp((100000f / Vector3.Distance(transform.position, col.transform.position)),0,1)   * Time.smoothDeltaTime);
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

        if (currentShieldCooldown > 0 && shield.GetComponent<BoxCollider>().enabled == false )
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
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            powerUsed = false;
        }

        if (currentSteelCooldown > 0 && steel.GetComponent<CapsuleCollider>().enabled == false)
        {
            steel.GetComponent<CapsuleCollider>().enabled = true;
            steel.GetComponent<MeshRenderer>().enabled = true;
            playerRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
}
