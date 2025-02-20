using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // [SerializeField] ParticleSystem scratchspolsion;
    [SerializeField] int health = 50;
    int maxHealth;

    [Header("Player Only")]
    [SerializeField] bool isPlayer = false;
    [SerializeField] float suckPower = 2f;
    [SerializeField] float stunTime = 30f;
    [SerializeField] float stunMovement = .01f;
    [SerializeField] bool applyCameraShake;

    [Header("Boss Only")]
    [SerializeField] bool isBoss = false;
    [SerializeField] bool isFinalBoss = false;
    [SerializeField] ParticleSystem bossExplosion;

    [Header("Enemy Only")]
    [SerializeField] int enemyValue = 0;
    [SerializeField] bool isVacuum;

    Rigidbody2D playerRB;
    CameraShake cameraShake;
    AudioPlayer audioPlayer;
    ScoreKeeper scoreKeeper;
    EnemySpawner enemySpawner;
    UIDisplay uIDisplay;
    Player playerScript;
    LevelManager levelManager;
    
    void Awake() 
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();
        levelManager = FindObjectOfType<LevelManager>();
        uIDisplay = FindObjectOfType<UIDisplay>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
        audioPlayer = FindObjectOfType<AudioPlayer>();
    }

    void Start() {

        if (isPlayer)
        {
            playerRB = GetComponent<Rigidbody2D>();
            playerScript = FindObjectOfType<Player>();
            maxHealth = GetRemainingHealth();
        }
    }


    public int GetRemainingHealth() 
    {
        return health;
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        DamageDealer damageDealer = other.GetComponent<DamageDealer>();

        if (damageDealer || other.tag == "suckRange" || other.tag == "powerUp")
        {
            if (other.tag == "lightningBolt" && isPlayer)
            {
                if (!playerScript.getPoweredUp())
                {
                    audioPlayer.PlayTakeDamage();
                    uIDisplay.ChangeFace("shocked");
                    StartCoroutine(Stunned());
                    TakeDamage(damageDealer.GetDamage());
                    damageDealer.Hit();
                }
                
            }
            else if (other.tag == "redLightningBolt" && isPlayer)
            {
                if (!playerScript.getPoweredUp())
                {
                    audioPlayer.PlayTakeDamage();
                    uIDisplay.ChangeFace("redShocked");
                    StartCoroutine(Stunned());
                    TakeDamage(damageDealer.GetDamage());
                    damageDealer.Hit();
                }

            }
            else if (other.tag == "suckRange" && isPlayer)
            {
                if (!playerScript.getPoweredUp())
                {
                    uIDisplay.ChangeFace("sucked");
                    playerScript.stunned = false;
                    playerRB.velocity = (transform.up * suckPower);
                }
                
            }
            else if (other.tag == "powerUp" && isPlayer)
            {
                RecoverHealth(playerScript.getRecoveryAmount());
                playerScript.setPoweredUp(true);
                uIDisplay.ChangeFace("powerUp");
                Destroy(other.gameObject);
                StartCoroutine(PoweredUp());

            }
            else if (isPlayer)
            {
                if (!playerScript.getPoweredUp())
                {
                    uIDisplay.ChangeFace("takeDamage");
                    audioPlayer.PlayTakeDamage();
                    TakeDamage(damageDealer.GetDamage());
                    ShakeCamera();
                    damageDealer.Hit();
                }
               
            }
            else if (other.tag != "suckRange")
            {
                TakeDamage(damageDealer.GetDamage());
                damageDealer.Hit();
            }
        }
       
    }

    void OnTriggerExit2D(Collider2D other) 
    {
        if (other.tag == "suckRange")
        {
            uIDisplay.NormalFace();
            playerRB.velocity = Vector2.zero;
        }
    }

    IEnumerator PoweredUp()
    {
        yield return new WaitForSeconds(playerScript.getPoweredUpTime());
        playerScript.setPoweredUp(false);
        uIDisplay.NormalFace();
    }


    IEnumerator Stunned()
    {
        Vector3 ogPosition = transform.position;

        playerScript.stunned = true;
        for (int i = 0; i < stunTime; i++){
            if (transform.position.x > ogPosition.x){
                transform.position = ogPosition;
                transform.position += new Vector3(-stunMovement, 0f);
            }
            else
            {
                transform.position = ogPosition;
                transform.position += new Vector3(stunMovement, 0f);
            }
            yield return new WaitForSeconds(.01f);
        }
        uIDisplay.NormalFace();
        transform.position = ogPosition;
        playerScript.stunned = false;

    }

    void TakeDamage(int damageTaken) 
    {
        
        health -= damageTaken;
       
        
        if (health <= 0)
        {
            if (isVacuum) 
            {
                Destroy(transform.parent.gameObject);
                audioPlayer.StopVacuumAudio();
            }
            else
            {
                Destroy(gameObject);
            }

            if (!isPlayer)
            {
                scoreKeeper.AddToScore(enemyValue);
            }

            if (isPlayer)
            {
                FindObjectOfType<LevelManager>().LoadGameOver();
            }

            if (isBoss)
            {
                if (!isFinalBoss)
                {
                    enemySpawner.bossDefeated = true;
                }
                PlayBossExplosion();
            }
            else if (!isBoss && isFinalBoss)
            {
                enemySpawner.bossDefeated = true;
                PlayBossExplosion();
            }



            audioPlayer.PlayDamageClip();
            
        }
    }

    void RecoverHealth(int recoveryAmount) 
    {
        if (health + recoveryAmount > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += recoveryAmount;
        }
        
    }

    void ShakeCamera() 
    {
        if (cameraShake != null && applyCameraShake)
        {
            cameraShake.Play();
        }
    }

    void PlayBossExplosion() 
    {
        if (bossExplosion != null)
        {
            ParticleSystem instance = Instantiate(bossExplosion, transform.position, Quaternion.identity);
            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
        }    
    }
}
