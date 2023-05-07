using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Fly In")]
    [SerializeField] Vector3 startingPos;
    [SerializeField] float flyInSpeed = 10f;

    [Header("Movement")]
    float xThrow, yThrow;
    [SerializeField] float xRange = 5f;
    [SerializeField] float yRangeTop = 10f;
    [SerializeField] float yRangeBottom = -3f;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float positionPitchFactor = -3f;
    [SerializeField] float controlPitchFactor = -10f;
    [SerializeField] float positionYawFactor = 2;
    [SerializeField] float controlRollFactor = 5;
    float xMovement;
    float yMovement;
    bool introFlight = false;
    bool cantMove = false;
    [SerializeField] bool invincible = false;

    [Header("Lasers")]
    [SerializeField] GameObject lasers;
    [SerializeField] Transform laserSpawnPointR;
    [SerializeField] Transform laserSpawnPointL;
    [SerializeField] int laserBasePower = 1;
    [SerializeField] int laserPowerUpPower = 1;
    [SerializeField] string laserLevelName;
    [SerializeField] bool isShooting;
    [SerializeField] bool isBetweenShots;
    bool needToSwitchLasers = true;
    Coroutine shooting;
    Transform laserLevel;

    GameStats gameStats;
    Enemy enemy;
    EnemyProjectile enemyProjectile;
    BossController bossController;
    [Header("End of Boss")]
    [SerializeField] bool missionComplete;

    void Awake() 
    {   
        gameStats = FindObjectOfType<GameStats>();

    }

    void Start() 
    {
        // laserLevelName = lasers.transform.GetChild(gameStats.GetLaserLevel()).name;
    }

    void Update()
    {
        // if (needToSwitchLasers && !cantMove) SwitchLasers(gameStats.GetLaserLevel());

        if (!cantMove)
        {
            if (!introFlight)
            {
                FlyIntoView();
            }
            else
            {
                Fly();
                FlyRotation();
            }
            Shoot();
            if (isShooting && !isBetweenShots)
            {
                StartCoroutine(ShootLaser());
            }
            else
            {
                StopCoroutine(ShootLaser());
            }
            
        }
        
        if (missionComplete)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(), moveSpeed * Time.deltaTime);
        }
    }

    IEnumerator ShootLaser() 
    {
        Instantiate(lasers, laserSpawnPointR.position, transform.rotation);
        Instantiate(lasers, laserSpawnPointL.position, transform.rotation);
        isBetweenShots = true;
        yield return new WaitForSeconds(.05f);
        isBetweenShots = false;
    }

    void Shoot() 
    {
        if (Input.GetButton("Fire1"))
        {
            ActivateLasers(true);
        }
        else
        {
            ActivateLasers(false);
        }
    }

    void ActivateLasers(bool state) 
    {
        isShooting = state;
        // laserLevel = lasers.transform.GetChild(gameStats.GetLaserLevel());

        // if (laserLevelName != laserLevel.name)
        // {
        //     needToSwitchLasers = true;
        //     laserLevelName = laserLevel.name;
        // } 

        // for(int i = 0; i < laserLevel.childCount; i++)
        // {
        //     GameObject laser = laserLevel.GetChild(i).gameObject;
        //     var emissionModule = laser.GetComponent<ParticleSystem>().emission;
        //     emissionModule.enabled = state;
        // }
    }

    void FlyIntoView()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, startingPos, flyInSpeed * Time.deltaTime);

        if (transform.localPosition == startingPos)
        {
            introFlight = true;
            return;
        }
    }

    void Fly() 
    {
        xThrow = Input.GetAxis("Horizontal");
        yThrow = Input.GetAxis("Vertical");
        xMovement = xThrow * Time.deltaTime * moveSpeed;
        yMovement = yThrow * Time.deltaTime * moveSpeed;
        float xClamp = Mathf.Clamp(transform.localPosition.x 
                                + xMovement, -xRange, xRange);
        float yClamp = Mathf.Clamp(transform.localPosition.y 
                                + yMovement, yRangeBottom, yRangeTop);
        Vector3 newMovement = new Vector3(xClamp, yClamp, 0);

        transform.localPosition = new Vector3(newMovement.x, newMovement.y, transform.localPosition.z);
    }

    void FlyRotation() 
    {
        float pitchDueToPos = transform.localPosition.y * positionPitchFactor;
        float pitchDueToControl = yThrow * controlPitchFactor;

        float pitch =  pitchDueToPos + pitchDueToControl;
        float roll = xThrow * controlRollFactor;
        float yaw = transform.localPosition.x * positionYawFactor;
        
        transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
    }


    void OnParticleCollision(GameObject other) 
    {
        if (other.tag == "Enemy" && !invincible)
        {
            enemyProjectile = other.gameObject.GetComponentInParent<EnemyProjectile>();
            gameStats.LoseHealth(enemyProjectile.GetAttackPower());
        }  
    }

    IEnumerator Invincibility()
    {
        yield return new WaitForSeconds(1);
        invincible = false;
    }

    private void OnTriggerEnter(Collider other) {
    
        if (other.transform.tag == "Enemy" && !invincible)
        {
            invincible = true;
            StartCoroutine(Invincibility());
            enemy = other.gameObject.GetComponentInParent<Enemy>();
            enemyProjectile = other.gameObject.GetComponent<EnemyProjectile>();
            if (enemy != null) gameStats.LoseHealth(enemy.GetAttackPower());
            if (enemyProjectile != null) gameStats.LoseHealth(enemyProjectile.GetAttackPower());
            
        }

    }

    void SwitchLasers(int laserLevel)
    {
        // for(int i = 0; i < lasers.transform.childCount; i++)
        // {
        //     lasers.transform.GetChild(i).gameObject.SetActive(false);
        // }

        // lasers.transform.GetChild(laserLevel).gameObject.SetActive(true);

        // needToSwitchLasers = false;
        
    }

    public void SetCantMove(bool state) 
    {
        cantMove = state;
        transform.localPosition = startingPos;
        if (state) ActivateLasers(false);
    }

    public Vector3 GetCurrentPos() 
    {
        return transform.position;
    }

    public void MissionComplete() 
    {
        missionComplete = true;
        
    }

}
