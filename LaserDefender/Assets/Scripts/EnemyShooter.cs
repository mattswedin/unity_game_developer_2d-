using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector3 positionOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileLifetime = 5f;
    [SerializeField] float baseFiringRate = .2f;
    [SerializeField] float firingRateVarience = 0f;
    [SerializeField] float minimumFiringRate = .1f;
    Coroutine firingCoroutine;
    AudioPlayer audioPlayer;

    public bool isFiring;

    void Awake()
    {
        audioPlayer = FindObjectOfType<AudioPlayer>();
    }

    void Update()
    {
        Fire();
    }

    void Fire()
    {
        if (isFiring && firingCoroutine == null)
        {
            firingCoroutine = StartCoroutine(FireContinuously());
        }
        else if (!isFiring && firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }

    }

    IEnumerator FireContinuously() 
    {
        while(isFiring) 
        {
            GameObject instance = Instantiate(projectilePrefab, transform.position + positionOffset, Quaternion.identity);
            Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
            if (rb != null) 
            {
                rb.velocity = -transform.up * projectileSpeed;
            }
            Destroy(instance, projectileLifetime);

            float timeToNextProjectile = Random.Range(baseFiringRate - firingRateVarience, baseFiringRate + firingRateVarience);

            timeToNextProjectile = Mathf.Clamp(timeToNextProjectile, minimumFiringRate, float.MaxValue);

            yield return new WaitForSeconds(timeToNextProjectile);
        }

    }
}
