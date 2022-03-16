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
    public Transform SpawnLocationOne;

    //UI stuff
    int KillCount;
    private float DamageCooldownTimer;
    public float DamageCooldownReset;
    public int HitPoints;
    public int SkullCount;
    public GameObject PauseUI;
    public GameObject MusicUI;
    public Transform HP;
    public Transform SkullText;
    public Transform DashText;
    public Transform KillText;

    //Misc
    public bool Paused;

    ////Attacking
    //public List<Transform> EnemiesInRange = new List<Transform>();
    //private float AttackZoneDecay = 0.1f;
    //public float AttackZoneDecayReset;

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

    //TO FIX: Review these once core gameplay and basic gfx/audio is in place
    //public Transform DashCam;
    //public Transform Shaker;

    private void Start()
    {
        DamageCooldownTimer = 0;
        Time.timeScale = 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.collider.tag == "ChasingPlayer") && DamageCooldownTimer == 0)
        {
            DamageCooldownTimer += DamageCooldownReset;
            Debug.Log("Player hit!");
            HP.GetComponent<Text>().text = ("Health left: " + HitPoints.ToString());
            HitPoints--;
            if (HitPoints <= 0)
            {
                HitPoints = 0;
                HP.GetComponent<Text>().text = ("Look how they massarcred ma boi");
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





    public void DoAttack(List<Transform> ER)
    {
        //Enemies in Range list placed here so AttackPrefab spawns regardless if enemies in range or not
        if (ER.Count > 0)
        {
            int index = Random.Range(0, ER.Count);

            //Sets enemy's death location to instantiate skull prefab
            Vector3 enemyLocationDeath = ER[index].transform.position;
            //index is a random number of how many targets are within Weapon Reach
            Destroy(ER[index].gameObject);

            AudioSource.PlayClipAtPoint(DeathRattle[Random.Range(0, DeathRattle.Count)], transform.position);


            //spawn skull on enemy death location                            //drops variance
            GameObject Skull = Instantiate(SkullPrefab, enemyLocationDeath + Random.insideUnitSphere * 0.3f, Quaternion.Euler(0, 180, 0));
            //TODO: Add skull flying animation
            KillCount++;
        }
    }

    //quit game in game options
    void QuitGame()
    {
        Debug.Log("GAME DIED");
        SceneManager.LoadScene("Main menu");
    }

    void Update()
    {
        DamageCooldownTimer -= Time.deltaTime;
        if (DamageCooldownTimer <= 0)
        {
            //Debug.Log("Ready to get hurt again");
            DamageCooldownTimer = 0;
        }

        //Gets player component as this script is embedded within player object
        rb = GetComponent<Rigidbody>();

        //Disables player collider whilst dashing
        GetComponent<CapsuleCollider>().enabled = !IsDashing;

        KillText.GetComponent<Text>().text = ("Kill Count: " + KillCount.ToString());
        SkullText.GetComponent<Text>().text = ("Skulls Collected: " + SkullCount.ToString());
        DashCooldown();
        Moving();
        MainAttack();
        EnemySpawn();

        //Generic actions like pausing, opening menu in-game etc.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //PAUSES GAME, OPENS IN GAME MENU WITH OPTION TO QUIT (Using LoadScene below) []
            Paused = !Paused;
            PauseUI.SetActive(Paused);
            if (Paused == true)
            {
                Time.timeScale = 0;
                PauseMusic.TransitionTo(0);
            }
            else
            {
                Time.timeScale = 1f;
                IngameMusic.TransitionTo(0);
            }

        }

        //changing music on music slider


       
        //stops keys being registered whilst paused
        if (Paused) return;


        //Enemy Spawns
        void EnemySpawn()
        {
            Spawn -= Time.deltaTime;
            if (Spawn <= 0)
            {
                Spawn = SpawnReset;
                GameObject EnemyBasic = Instantiate(EnemyPrefab, SpawnLocationOne.position, Quaternion.Euler(65, 0, 0));

                //this code allows prefabs to use a script and follow an object
                EnemyBasic.GetComponent<Chaser>().TargetPosition = Player.transform;
            }

        }
        //Attacking, random kill code
        void MainAttack()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)/* && EnemiesInRange.Count > 0*/)
            {
                //TODO: ADD SOUND [X]
                //      ADD ANIMATION []

                //Weapon sound triggers
                AudioSource.PlayClipAtPoint(WeaponSwings[Random.Range(0, WeaponSwings.Count)], transform.position);

                //creates Attack collider prefab
                GameObject AttackZone = Instantiate(AttackPrefab, transform.position, Quaternion.identity);
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
                TargetPosition = transform.position + rb.velocity.normalized * DashDistance; //direction * distance dashed, ready to be smoothed
                AudioSource.PlayClipAtPoint(DashNoise, transform.position);
                IsDashing = true;
            }
            //TODO:**** Ensuring SmoothingSpeed stops messing with player movement
            //Smoothing the dash visual  ... SmoothingSpeed affects normal movement also..

            if (IsDashing)
            {
                transform.position = Vector3.MoveTowards(transform.position, TargetPosition, SmoothingSpeed * Time.deltaTime);
                if (transform.position == TargetPosition) IsDashing = false;
            }
        }

        //TO FIX:
        //Shake effect that links to the camera object NOTE: currently resets camera position to Players starting position

        //if (Cooldown > 0)
        //{
        //    Shaker.position = Random.insideUnitSphere * (Cooldown / 2);
        //}

    }
}
