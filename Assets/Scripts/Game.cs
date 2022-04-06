using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    //animation
    public Animator animator;

    //Sounds
    public AudioClip DashNoise;
    public AudioClip EnterGame;
    public List<AudioClip> DeathRattle;
    public List<AudioClip> WeaponSwings;
    public List<AudioClip> PlayerDamage;
    public List<AudioClip> PlayerDeath;
    public AudioMixerSnapshot PauseMusic;
    public AudioMixerSnapshot IngameMusic;
    public AudioClip TooPoor;
    public AudioClip UpgradedSFX;

    //Prefabs
    public GameObject EnemyGoblinPrefab;
    public GameObject EnemyGorPrefab;
    public GameObject Player;
    public GameObject SkullPrefab;
    public GameObject AttackPrefab;
    public GameObject BloodPrefab;

    //Enemy
    float GoblinSpawnStart;
    float GorSpawnStart;
    public float GoblinSpawnReset;
    public float GorSpawnReset;
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
    public Transform TimerText;
    public Transform DashText;
    public Transform KillText;

    //in-game menu UI active/inactive
    public bool Paused;
    public bool InShop;
    public GameObject PauseUI;
    public GameObject ShopUI;

    //ingame menu currency
    public int HpCost;
    public int DmgBuffCost;
    public int DashBuffCost;

    //Shop UI
    public GameObject HealButton;
    public GameObject AttackBuffButton;
    public GameObject DashBuffButton;
    public GameObject HealCostText;
    public GameObject DmgBuffCostText;
    public GameObject DashBuffCostText;
    public bool DmgBuff;
    public int DmgModifier;

    public enum Direction
    {
        Left, Right, Up, Down
    }
    public Direction currentFacingDirection;


    //Misc
    private GameObject Music;
    public GameObject Camera;

    //Time & difficulty
    public Text timeText;
    public bool IsDead;
    public float timeRemaining;
    public bool timerIsRunning;

    //Movement
    private Rigidbody rb;
    public float VelocitySpeed;
    Vector3 TargetPosition;

    //Dash
    public GameObject DashEffectPrefab;
    private float Cooldown;
    public float DashReset;
    public float DashDistance;
    public float SmoothingSpeed;
    public bool IsDashing;

    private void Start()
    {
        Time.timeScale = 1f;
        timerIsRunning = true;
        DamageCooldownTimer = 1f;
        Music = GameObject.Find("Music");
        AudioSource.PlayClipAtPoint(EnterGame, transform.position);
        HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Taking damage from Goblins
        if ((collision.collider.tag == "ChasingPlayer") && DamageCooldownTimer == 0)
        {
            //Cooldown for taking damage from enemy resets
            DamageCooldownTimer += DamageCooldownReset;
            if (HitPoints >= 1)
            {
                HitPoints -= 1;
                AudioSource.PlayClipAtPoint(PlayerDamage[Random.Range(0, PlayerDamage.Count)], transform.position);
                HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
            }
        }

        //Taking damage from Gors
        if ((collision.collider.tag == "Gor") && DamageCooldownTimer == 0)
        {
            //Cooldown for taking damage from enemy resets
            DamageCooldownTimer += DamageCooldownReset;
            if (HitPoints >= 1)
            {
                HitPoints -= 3;
                AudioSource.PlayClipAtPoint(PlayerDamage[Random.Range(0, PlayerDamage.Count)], transform.position);
                HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
            }
        }

        if ((collision.collider.tag == "ChasingPlayer") && HitPoints <= 0)
        {
            IsDead = true;
            //Plays death animation
            animator.SetBool("PlayerDeath", true);
            //Sets all movement animations to stop (was a bug)
            animator.SetBool("MoveUp", false);
            animator.SetBool("MoveDown", false);
            animator.SetBool("Moving", false);
            animator.SetBool("Moving", false);
            HP.GetComponent<Text>().text = ("You died");
            AudioSource.PlayClipAtPoint(PlayerDeath[Random.Range(0, PlayerDeath.Count)], transform.position);
        }

        if ((collision.collider.tag == "Gor") && HitPoints <= 0)
            {
                IsDead = true;
                //Plays death animation
                animator.SetBool("PlayerDeath", true);
                //Sets all movement animations to stop (was a bug)
                animator.SetBool("MoveUp", false);
                animator.SetBool("MoveDown", false);
                animator.SetBool("Moving", false);
                animator.SetBool("Moving", false);
                HP.GetComponent<Text>().text = ("You died");
                AudioSource.PlayClipAtPoint(PlayerDeath[Random.Range(0, PlayerDeath.Count)], transform.position);
            }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SkullPickup")
        {
            SkullCount += 1;
            Destroy(other.gameObject);
        }
    }

    //Timer function
    public void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60); //0 being minutes, 1 being seconds in a 00 format
        TimerText.GetComponent<Text>().text = ("Time left: " + string.Format("{0:00}:{1:00}", minutes, seconds));

    }

    //buying health at ingame shop
    public void HealthUpgrade()
    {
        HitPoints += 3;
        HP.GetComponent<Text>().text = ("HP: " + HitPoints.ToString());
        SkullCount -= HpCost;
        SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());
    }

    public void AttackUpgrade()
    {
        //Boolean that enables, MainAttack checks for enabled and adds 1 more dmg to enemies in range
        if (DmgModifier < 5)
        {
            DmgModifier += 1;
            SkullCount -= DmgBuffCost;
            SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());
        }
        else
        {
            DmgModifier = 5;
            AttackBuffButton.GetComponentInChildren<Text>().text = ("Fully upgraded!");
        }
    }

    public void DashUpgrade()
    {
        if (DashReset > 1)
        {
            DashReset -= 1;
            SkullCount -= DashBuffCost;
            SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());
        }
        else
        {
            DashReset = 1;
            DashBuffButton.GetComponentInChildren<Text>().text = ("Fully upgraded!");
        }
    }


    //quit game ingame option
    public void QuitGame()
    {
        SceneManager.LoadScene("Main menu");
        //Allows the singleton class to transition music back to menu with the proper sound settings
        IngameMusic.TransitionTo(0);

    }

    //___________________________Big attack function_____________________//
    public void DoAttack(List<Transform> ER)
    {
        List<int> EnemiesInRangeIndexes = new List<int>();
        for (int i = 0; i < ER.Count; i++) EnemiesInRangeIndexes.Add(i);

        //clamps DmgModifier to number of enemies in range to prevent overkillllll
        int count = DmgModifier;
        if (ER.Count < count) count = ER.Count;

        //loops depending on dmg modifier, clamped to count value
        for (int i = 0; i < count; i++)
        {
            //index of enemy to be killed is based off index of items
            int index = EnemiesInRangeIndexes[Random.Range(0, EnemiesInRangeIndexes.Count)];

            Vector3 enemyLocationDeath = ER[index].transform.position;
            //object destroyed after 5 seconds
            Destroy(ER[index].gameObject, 30);
            Destroy(ER[index].gameObject.GetComponent<Chaser>());
            Destroy(ER[index].gameObject.GetComponent<MonstrousChaser>());
            Destroy(ER[index].gameObject.GetComponent<CapsuleCollider>());
            Destroy(ER[index].gameObject.GetComponent<Rigidbody>());
            //Triggers both enemy dying animations
            ER[index].gameObject.GetComponent<Animator>().SetTrigger("IsDying");

            //item from list is now destroyed to loop back over the next one again
            EnemiesInRangeIndexes.Remove(index);

            AudioSource.PlayClipAtPoint(DeathRattle[Random.Range(0, DeathRattle.Count)], transform.position);

            //spawn skull on enemy death location                            //drops variance
            GameObject Skull = Instantiate(SkullPrefab, enemyLocationDeath + Random.insideUnitSphere * 0.3f, Quaternion.Euler(0, 180, 0));

            //blood effect on enemy death location
            GameObject Blood = Instantiate(BloodPrefab, enemyLocationDeath, Quaternion.identity);
            Destroy(Blood, 2);
        }
        KillCount += count;

    }

    void Update()
    {
        //difficulty setting = set timeRemaining = whatever value
        DisplayTime(timeRemaining);

        //countdown timer
        if (timerIsRunning)
        {
            if (timeRemaining > 0) timeRemaining -= Time.deltaTime;
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                SceneManager.LoadScene("End Level");
            }

        }

        //cooldown between taking dmg
        DamageCooldownTimer -= Time.deltaTime;
        if (DamageCooldownTimer <= 0)
        {
            DamageCooldownTimer = 0;
        }

        //Postprocess effect for low HP
        if (HitPoints >= 3) Camera.GetComponent<PostProcessVolume>().enabled = false;
        else Camera.GetComponent<PostProcessVolume>().enabled = true;


        //Gets player component as this script is embedded within player object
        rb = GetComponent<Rigidbody>();

        //shop UI text cost updates as cost increases
        HealCostText.GetComponent<Text>().text = (HpCost.ToString() + " skulls");
        DmgBuffCostText.GetComponent<Text>().text = (DmgBuffCost.ToString() + " skulls");
        DashBuffCostText.GetComponent<Text>().text = (DashBuffCost.ToString() + " skulls");

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
                Time.timeScale = 0.1f;
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
        //Health purchase
        if (InShop == true && Input.GetKeyDown(KeyCode.Alpha1) && SkullCount >= HpCost)
        {
            HealthUpgrade();
            HealButton.GetComponentInChildren<Text>().text = ("Claimed!");
            AudioSource.PlayClipAtPoint(UpgradedSFX, transform.position);

        }
        else if (InShop == true && Input.GetKeyDown(KeyCode.Alpha1) && SkullCount < HpCost)
        {
            HealButton.GetComponentInChildren<Text>().fontSize = 12;
            HealButton.GetComponentInChildren<Text>().text = ("Need more skulls!");
            AudioSource.PlayClipAtPoint(TooPoor, transform.position);
        }

        //Attack buff purchase
        if (InShop == true && Input.GetKeyDown(KeyCode.Alpha2) && SkullCount >= DmgBuffCost)
        {
            AttackUpgrade();
            AttackBuffButton.GetComponentInChildren<Text>().text = ("CLAIMED!");
            //doubles the cost of the upgrade for each purchase
            DmgBuffCost *= 2;
            //makes healing incrementally more expensive. bAlaNcE
            HpCost += 5;
            AudioSource.PlayClipAtPoint(UpgradedSFX, transform.position);

        }
        else if (InShop == true && Input.GetKeyDown(KeyCode.Alpha2) && SkullCount < DmgBuffCost)
        {

            AttackBuffButton.GetComponentInChildren<Text>().fontSize = 12;
            AttackBuffButton.GetComponentInChildren<Text>().text = ("need more skulls!");
            AudioSource.PlayClipAtPoint(TooPoor, transform.position);
        }

        //Dash buff purchase
        if (InShop == true && Input.GetKeyDown(KeyCode.Alpha3) && SkullCount >= DashBuffCost)
        {
            DashUpgrade();
            DashBuffButton.GetComponentInChildren<Text>().text = ("CLAIMED!");
            //doubles the cost of the upgrade for each purchase
            DashBuffCost *= 2;
            AudioSource.PlayClipAtPoint(UpgradedSFX, transform.position);

        }
        else if (InShop == true && Input.GetKeyDown(KeyCode.Alpha3) && SkullCount < DashBuffCost)
        {
            DashBuffButton.GetComponentInChildren<Text>().fontSize = 12;
            DashBuffButton.GetComponentInChildren<Text>().text = ("Need more skulls!");
            AudioSource.PlayClipAtPoint(TooPoor, transform.position);
        }



        //changing music on music slider
        Music.GetComponent<AudioSource>().volume = MusicUI.GetComponent<Slider>().value;

        //stops keys being registered whilst paused
        if (Paused || InShop) return;





        //Disables player collider whilst dashing
        GetComponent<CapsuleCollider>().enabled = !IsDashing;

        //tallies the kills made/skulls collected
        KillText.GetComponent<Text>().text = ("Kill Count: " + KillCount.ToString());
        SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());

        //function calls for generic game mechanics
        DashCooldown();
        Moving();
        MainAttack();
        GoblinSpawn();
        GorSpawn();

        //Enemy Gor spawns
        void GorSpawn()
        {
            GorSpawnStart -= Time.deltaTime;
            if (GorSpawnStart <= 0)
            {
                Transform RandomSpawnpoint = SpawnLocations[Random.Range(0, SpawnLocations.Count)].transform;

                GorSpawnStart = GorSpawnReset;
                GameObject EnemyGor = Instantiate(EnemyGorPrefab, RandomSpawnpoint.position, Quaternion.Euler(65, 0, 0));

                // this code allows prefabs to use a script and follow an object
                EnemyGor.GetComponent<MonstrousChaser>().TargetPosition = Player.transform;
            }
        }

        //Enemy Gobbo Spawns
        void GoblinSpawn()
        {
            GoblinSpawnStart -= Time.deltaTime;
            if (GoblinSpawnStart <= 0)
            {
                //picks one of the spawn point gameobjects and pulls its transform info
                Transform RandomSpawnpoint = SpawnLocations[Random.Range(0, SpawnLocations.Count)].transform;

                GoblinSpawnStart = GoblinSpawnReset;                              //grabs random transform.position info to spawn from
                GameObject EnemyBasic = Instantiate(EnemyGoblinPrefab, RandomSpawnpoint.position, Quaternion.Euler(65, 0, 0));

                //this code allows prefabs to use a script and follow an object
                EnemyBasic.GetComponent<Chaser>().TargetPosition = Player.transform;
            }

        }
        //Attacking, random kill code
        void MainAttack()
        {
            if (IsDead) return;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //triggers attack animation
                animator.SetTrigger("Attacking");

                //Weapon sound triggers
                AudioSource.PlayClipAtPoint(WeaponSwings[Random.Range(0, WeaponSwings.Count)], transform.position);
                //creates Attack collider prefab

                Vector3 facing = Vector3.zero;
                if (currentFacingDirection == Direction.Left)
                {
                    facing = Vector3.left;
                    animator.SetTrigger("Attacking");
                }
                if (currentFacingDirection == Direction.Right)
                {
                    facing = Vector3.right;
                    animator.SetTrigger("Attacking");
                }
                if (currentFacingDirection == Direction.Up)
                {
                    facing = Vector3.forward;
                    animator.SetTrigger("AttackUp");
                }
                if (currentFacingDirection == Direction.Down)
                {
                    facing = Vector3.back;
                    animator.SetTrigger("AttackDown");
                }

                GameObject AttackZone = Instantiate(AttackPrefab, transform.position + facing, Quaternion.identity);

                Debug.DrawLine(transform.position, transform.position + facing, Color.green, 3);

                //Attack collider prefab grabs the 'Attacking' script and sets the game part of the script to refer to THIS script
                AttackZone.GetComponent<Attacking>().game = this;
            }


        }


        //Dash cooldown
        void DashCooldown()
        {

            if (IsDead) return;

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
            if (IsDead) return;

            if (IsDashing == false)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    rb.velocity += Vector3.forward * VelocitySpeed * Time.deltaTime;
                    animator.SetBool("MoveUp", true);
                    currentFacingDirection = Direction.Up;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    rb.velocity += Vector3.back * VelocitySpeed * Time.deltaTime;
                    animator.SetBool("MoveDown", true);
                    currentFacingDirection = Direction.Down;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    rb.velocity += Vector3.left * VelocitySpeed * Time.deltaTime;
                    animator.SetBool("Moving", true);
                    currentFacingDirection = Direction.Left;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    rb.velocity += Vector3.right * VelocitySpeed * Time.deltaTime;
                    animator.SetBool("Moving", true);
                    currentFacingDirection = Direction.Right;
                }
                //Sets animator to move back to player_idle
                if (Input.GetKeyUp(KeyCode.W)) animator.SetBool("MoveUp", false);
                if (Input.GetKeyUp(KeyCode.S)) animator.SetBool("MoveDown", false);
                if (Input.GetKeyUp(KeyCode.A)) animator.SetBool("Moving", false);
                if (Input.GetKeyUp(KeyCode.D)) animator.SetBool("Moving", false);

            }

            ////rotates player to follow 2 Axis direction(4 axis if using 3d object)
            //left
            if (rb.velocity.x < 0) transform.rotation = Quaternion.Euler(245, 0, 180);
            //right
            if (rb.velocity.x > 0) transform.rotation = Quaternion.Euler(65, 0, 0);

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
                GameObject DashEffect = Instantiate(DashEffectPrefab, transform.position, Quaternion.identity);
                Destroy(DashEffect, 0.3f);
                if (transform.position == TargetPosition) IsDashing = false;
            }
        }


    }


}
