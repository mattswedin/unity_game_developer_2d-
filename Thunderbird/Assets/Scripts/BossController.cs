using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] bool begin = true;
    
    [Header("Boss Stats")]
    [SerializeField] int health = 1000;

    [Header("Boss Timing")]
    [SerializeField] float startDelay = 1.5f;
    [SerializeField] bool canShoot = true;

    [Header("Boss Objects")]
    [SerializeField] GameObject[] bossPhases;
    [SerializeField] int bossIndex;

    [Header("Boss Phase 3 Only")]
    [SerializeField] int handsDefeated = 0;

    GameObject currentBossPhase;
    PlayerController playerController;

    void Start() 
    {
        if (begin) StartCoroutine(BossBegins());
        playerController = FindObjectOfType<PlayerController>();
        
    }

    IEnumerator BossBegins() 
    {
        yield return new WaitForSeconds(startDelay);
        currentBossPhase = bossPhases[bossIndex];
        currentBossPhase.SetActive(true);
    }

    public IEnumerator EndBossPhase() 
    {
        yield return new WaitForSeconds(startDelay);
        Destroy(bossPhases[bossIndex]);
        bossIndex++;
        if (bossIndex == bossPhases.Length - 1) 
        {

        }
        else if (bossIndex < bossPhases.Length)
        {
            currentBossPhase = bossPhases[bossIndex];
            currentBossPhase.SetActive(true);
        }
        else
        {
            playerController.MissionComplete();
        }
    }

    public void CanBossShoot(bool state) 
    {
        currentBossPhase.GetComponent<Enemy>().CanBossShoot(state);
    }

    //Boss Final Phase Only

    public void HandDefeated()
    {
        handsDefeated += 1;
    }

    public int GetHandsDefeated()
    {
        return handsDefeated;
    }



}
