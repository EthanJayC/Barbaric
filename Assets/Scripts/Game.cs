using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    //Sounds
    public AudioClip DashNoise;
    public List<AudioClip> DeathRattle;
    public List<AudioClip> WeaponSwings;
    public AudioMixerSnapshot PauseMusic;
    public AudioMixerSnapshot IngameMusic;

    //Prefabs
    public GameObject EnemyPrefab;
    public GameObject Player;
    public GameObject SkullPrefab;
    public GameObject AttackPrefab;

    //Enemy
    float Spawn;
    public float EnemySpeed;
    public float SpawnReset;
    public float SkullCooldownPickup;
    public List<GameObject> SpawnLocations;

    //generic UI stuff
    int KillCount;
    private float DamageCooldownTimer;
    public float DamageCooldownReset;
    public int HitPoints;
    public int SkullCount;
    public GameObject MusicUI;
    public Transform HP;
    public Transform SkullText;
    public Transform DashText;
    public Transform KillText;

    //in-game menu UI stuff
    public bool Paused;
    public bool InShop;
    public GameObject PauseUI;
    public GameObject ShopUI;

    //Misc
    private GameObject Music;
    public GameObject HealButton;
    public GameObject AttackBuffButton;
    public GameObject DashBuffButton;
    public bool DmgBuff;

    //Movement
    private Rigidbody rb;
    public float VelocitySpeed;
    Vector3 TargetPosition;

    //Dash
    private float Cooldown;
    public float DashReset;
    public float DashDistance;
    public float SmoothingSpeed;
    public bool IsDashing;

    private void Start()
    {
        DamageCooldownTimer = 0;
        Time.timeScale = 1f;
        Music = GameObject.Find("Music");
        HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.collider.tag == "ChasingPlayer") && DamageCooldownTimer == 0)
        {
            DamageCooldownTimer += DamageCooldownReset;
            HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
            HitPoints--;
            if (HitPoints <= 0)
            {
                HitPoints = 0;
                HP.GetComponent<Text>().text = ("You died.");
                Time.timeScale = 0;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SkullPickup")
        {

            SkullCount++;
            Destroy(other.gameObject);
        }
    }

    //TODO; SHOP BUTTONS
    // [X] Health upgrade
    // [] Attack buff
    // [] Dash buff

    //buying health at ingame shop
    public void HealthUpgrade()
    {
        HitPoints += 3;
        HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
        SkullCount -= 20;
        SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());
    }


    //quit game ingame option
    public void QuitGame()
    {
        //TODO: SINGLETON STUFF TO KILL MUSIC ON QUITTING BACK TO MENU
        SceneManager.LoadScene("Main menu");
    }


    public void DoAttack(List<Transform> ER)
    {
        //Enemies in Range list placed here so AttackPrefab spawns regardless if enemies in range or not
        if (ER.Count > 0 && DmgBuff == false)
        {
            int index = Random.Range(0, ER.Count);
            //Sets enemy's death location to instantiate skull prefab
            Vector3 enemyLocationDeath = ER[index].transform.position;
            //index is a random number of how many targets are within Weapon Reach
            Destroy(ER[index].gameObject);

            AudioSource.PlayClipAtPoint(DeathRattle[Random.Range(0, DeathRattle.Count)], transform.position);

            //spawn skull on enemy death location                            //drops variance
            GameObject Skull = Instantiate(SkullPrefab, enemyLocationDeath + Random.insideUnitSphere * 0.3f, Quaternion.Euler(0, 180, 0));
            KillCount++;

        }
        //This is the attack once damage buff has been bought NOTE: Want this to scale so player can keep increasing it
        else if (ER.Count > 1 && DmgBuff != false)
        {
            //should loop twice
            for (int i = 0; i < 2; i++)
            {
                int index = Random.Range(0, ER.Count);
                Vector3 enemyLocationDeath = ER[index].transform.position;
                Destroy(ER[index].gameObject);
                AudioSource.PlayClipAtPoint(DeathRattle[Random.Range(0, DeathRattle.Count)], transform.position);

                //spawn skull on enemy death location                            //drops variance
                GameObject Skull = Instantiate(SkullPrefab, enemyLocationDeath + Random.insideUnitSphere * 0.3f, Quaternion.Euler(0, 180, 0));

                KillCount ++;
            }
        } 
        else if (ER.Count > 0 && DmgBuff != false)
        //should take into account killing only 1 enemy, and drop 1 skull rather than 2
        {
            int index = Random.Range(0, ER.Count);
            //Sets enemy's death location to instantiate skull prefab
            Vector3 enemyLocationDeath = ER[index].transform.position;
            //index is a random number of how many targets are within Weapon Reach
            Destroy(ER[index].gameObject);
            AudioSource.PlayClipAtPoint(DeathRattle[Random.Range(0, DeathRattle.Count)], transform.position);
            //spawn skull on enemy death location                            //drops variance
            GameObject Skull = Instantiate(SkullPrefab, enemyLocationDeath + Random.insideUnitSphere * 0.3f, Quaternion.Euler(0, 180, 0));
            KillCount++;
        }
    }
    public void AttackUpgrade()
    {
        Debug.Log("ATTTTAAACK BUUUUFF!");
        //Boolean that enables, MainAttack checks for enabled and doubles dmg
        DmgBuff = true;
    }

    void Update()
    {
        //Pausing the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Paused = !Paused;
            PauseUI.SetActive(Paused);
            if (Paused == true)
            {
                Time.timeScale = 0;
                //no slow transition as it isn't allowed when timescale freezes
                PauseMusic.TransitionTo(0);
            }
            else
            {
                Time.timeScale = 1f;
                IngameMusic.TransitionTo(0);
            }
        }
        //Opening ingame shop
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InShop = !InShop;
            ShopUI.SetActive(InShop);
            if (InShop == true)
            {
                Time.timeScale = 0; //< SET TO SUPER SLOWMO FOR ACTUAL GAME RELEASE []
                PauseMusic.TransitionTo(0);
            }
            else
            {
                Time.timeScale = 1f;
                IngameMusic.TransitionTo(0);
                //Resets texts and font sizes if player had insufficient skulls for purchase
                HealButton.GetComponentInChildren<Text>().fontSize = 18;
                HealButton.GetComponentInChildren<Text>().text = ("Heal");
                AttackBuffButton.GetComponentInChildren<Text>().fontSize = 18;
                AttackBuffButton.GetComponentInChildren<Text>().text = ("Smite your foes");
                DashBuffButton.GetComponentInChildren<Text>().fontSize = 18;
                DashBuffButton.GetComponentInChildren<Text>().text = ("Dash faster");
            }
        }

        //Healing function (Keybinds used until buttons are fixed)
        //TODO [] Add sounds on purchase!
        //TODO [] Add sound for insufficient skulls!

        //Health purchase
        if (InShop == true && Input.GetKeyDown(KeyCode.Alpha1) && SkullCount >= 20)
        {
            HealthUpgrade();
        }
        else if (InShop == true && Input.GetKeyDown(KeyCode.Alpha1) && SkullCount < 20)
        {
            HealButton.GetComponentInChildren<Text>().fontSize = 12;
            HealButton.GetComponentInChildren<Text>().text = ("Need more skulls!");
        }

        //Attack buff purchase
        //NOTE: CHANGE TO HIGHER AFTER TESTING
        if (InShop == true && Input.GetKeyDown(KeyCode.Alpha2) && SkullCount >= 5)
        {
            AttackUpgrade();
        }
        else if (InShop == true && Input.GetKeyDown(KeyCode.Alpha2) && SkullCount < 5)
        {

            AttackBuffButton.GetComponentInChildren<Text>().fontSize = 12;
            AttackBuffButton.GetComponentInChildren<Text>().text = ("need more skulls!");
        }

        //Dash buff purchase
        if (InShop == true && Input.GetKeyDown(KeyCode.Alpha3) && SkullCount >= 40)
        {
            //DashUpgrade();
            Debug.Log("dash upgrade purchased");
        }
        else if (InShop == true && Input.GetKeyDown(KeyCode.Alpha3) && SkullCount < 40)
        {
            DashBuffButton.GetComponentInChildren<Text>().fontSize = 12;
            DashBuffButton.GetComponentInChildren<Text>().text = ("Need more skulls!");
        }



        //changing music on music slider
        Music.GetComponent<AudioSource>().volume = MusicUI.GetComponent<Slider>().value;

        //stops keys being registered whilst paused
        if (Paused || InShop) return;


        //cooldown between taking dmg
        DamageCooldownTimer -= Time.deltaTime;
        if (DamageCooldownTimer <= 0)
        {
            DamageCooldownTimer = 0;
        }

        //Gets player component as this script is embedded within player object
        rb = GetComponent<Rigidbody>();

        //Disables player collider whilst dashing
        GetComponent<CapsuleCollider>().enabled = !IsDashing;

        //tallies the kills made/skulls collected
        KillText.GetComponent<Text>().text = ("Kill Count: " + KillCount.ToString());
        SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());

        //function calls for generic game mechanics
        DashCooldown();
        Moving();
        MainAttack();
        EnemySpawn();




        //Enemy Spawns
        void EnemySpawn()
        {
            Spawn -= Time.deltaTime;
            if (Spawn <= 0)
            {
                //picks one of the spawn point gameobjects and pulls its transform info
                Transform RandomSpawnpoint = SpawnLocations[Random.Range(0, SpawnLocations.Count)].transform;

                Spawn = SpawnReset;                              //grabs random transform.position info to spawn from
                GameObject EnemyBasic = Instantiate(EnemyPrefab, RandomSpawnpoint.position, Quaternion.Euler(65, 0, 0));

                //this code allows prefabs to use a script and follow an object
                EnemyBasic.GetComponent<Chaser>().TargetPosition = Player.transform;
            }

        }
        //Attacking, random kill code
        void MainAttack()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //TODO: ADD SOUND [X]
                //      ADD ANIMATION []

                //Weapon sound triggers
                AudioSource.PlayClipAtPoint(WeaponSwings[Random.Range(0, WeaponSwings.Count)], transform.position);
                //creates Attack collider prefab
                GameObject AttackZone = Instantiate(AttackPrefab, transform.position, Quaternion.identity);
                //Attack collider prefab grabs the 'Attacking' script and sets the game part of the script to refer to THIS script
                AttackZone.GetComponent<Attacking>().game = this;
            }


        }


        //Dash cooldown
        void DashCooldown()
        {
            Cooldown -= Time.deltaTime;
            if (Cooldown < 0)
            {
                DashText.GetComponent<Text>().text = ("Dash ready!");
                Cooldown = 0;
            }
            else
            {
                DashText.GetComponent<Text>().text = ("Dash recharging..");
            }

        }
        //Movement
        void Moving()
        {
            if (IsDashing == false)
            {
                if (Input.GetKey(KeyCode.W)) rb.velocity += Vector3.forward * VelocitySpeed * Time.deltaTime;
                if (Input.GetKey(KeyCode.S)) rb.velocity += Vector3.back * VelocitySpeed * Time.deltaTime;
                if (Input.GetKey(KeyCode.A)) rb.velocity += Vector3.left * VelocitySpeed * Time.deltaTime;
                if (Input.GetKey(KeyCode.D)) rb.velocity += Vector3.right * VelocitySpeed * Time.deltaTime;

            }

            ////rotates player to follow 2 Axis direction(4 axis if using 3d object)
            //left
            if (rb.velocity.x > 0) transform.rotation = Quaternion.Euler(245, 0, 180);
            //right
            if (rb.velocity.x < 0) transform.rotation = Quaternion.Euler(65, 0, 0);

            //Dash ability, the direction player is moving
            if (Input.GetKeyDown(KeyCode.Space) && Cooldown <= 0f)
            {
                Cooldown += DashReset;
                TargetPosition = transform.position + rb.velocity.normalized * DashDistance;
                AudioSource.PlayClipAtPoint(DashNoise, transform.position);
                IsDashing = true;
            }
            //Leaving the below comments in to highlight that LERP and smoothing isn't necessary for physics based dash move
            //...
            //Ensuring SmoothingSpeed stops messing with player movement
            //Smoothing the dash visual  ... SmoothingSpeed affects normal movement also..

            if (IsDashing)
            {
                transform.position = Vector3.MoveTowards(transform.position, TargetPosition, SmoothingSpeed * Time.deltaTime);
                if (transform.position == TargetPosition) IsDashing = false;
            }
        }


    }
}
