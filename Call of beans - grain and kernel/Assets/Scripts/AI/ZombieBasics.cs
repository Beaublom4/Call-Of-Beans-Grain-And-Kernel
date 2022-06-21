using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBasics : MonoBehaviour
{
    Transform targetDestination;
    public GameObject[] players;
    public float basicSmashDamage;
    public GameObject smashHitBox;

    [HideInInspector]public Animator anim;
    Rigidbody rb;
    ZombieManager _zombieManager;
    [HideInInspector] public GameObject barrier;
    [HideInInspector] public GameObject player;
    [HideInInspector] public NavMeshAgent nav;
    [HideInInspector] public float navSpeed;
    [HideInInspector] public bool playerDetected;

    bool cooldown;
    [HideInInspector] public bool plankDetected;
    [HideInInspector] public Barrier currentBarrier;
    [HideInInspector] public Health currentPlayer;
    private void Awake()
    {
        _zombieManager = FindObjectOfType<ZombieManager>();
        players = _zombieManager.players;
        GetClosestPlayer();
    }
    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        navSpeed = nav.speed;
        rb = gameObject.GetComponent<Rigidbody>();
        anim = gameObject.GetComponent<Animator>();
        InvokeRepeating("GetClosestPlayer", 0, 10);
    }
    private void Update()
    {
        nav.destination = targetDestination.transform.position;

        if (plankDetected && !cooldown)
        {
            anim.SetLayerWeight(anim.GetLayerIndex("BasicSmash"), 1);
            anim.SetTrigger("BasicSmash");
            cooldown = true;
            DestroyPlank();
        }
        if(playerDetected && !cooldown)
        {
            StartCoroutine(smashPlayer());
            IEnumerator smashPlayer()
            {
                anim.SetLayerWeight(anim.GetLayerIndex("BasicSmash"), 1);
                anim.SetTrigger("BasicSmash");
                yield return new WaitForSecondsRealtime(0.1f);
                smashHitBox.SetActive(true);
                yield return new WaitForSecondsRealtime(0.1f);
                smashHitBox.SetActive(false);
            }
        }
    }
    public void GetClosestPlayer()
    {
        GameObject currentClosest = null;
        float currentClosestDistance = 0;
        foreach(GameObject player in players)
        {
            if(currentClosest == null)
            {
                currentClosest = player;
                currentClosestDistance = Vector3.Distance(transform.position, currentClosest.transform.position);
            }
            else
            {
                float newDistance = Vector3.Distance(transform.position, currentClosest.transform.position);
                if(newDistance < currentClosestDistance)
                {
                    currentClosest = player;
                    currentClosestDistance = newDistance;
                }
            }
        }
        targetDestination = currentClosest.transform;
    }
    //public void PlayerDetection(Health health)
    //{
    //    playerDetected = true;
    //    health.GetComponent<Health>().PlayerGetHit(basicSmashDamage, this, true);
    //}
    public void PlankDetection(Barrier barrier)
    {
        currentBarrier = barrier;
        if (currentBarrier.currentPlank > 0)
        {
            nav.speed = 0;
            plankDetected = true;
        }
    }
    public void DestroyPlank()
    {
        
        if (currentBarrier.currentPlank >= 0)
        {
            currentBarrier.DestroyPlank(1);
        }
        else
            plankDetected = false;
        StartCoroutine(Cooldown());
    }
    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1);
        if (currentBarrier.currentPlank < 0)
            nav.speed = navSpeed;
        cooldown = false;
    }

    public void HitPlayer()
    {
        //player.gameObject.GetComponent<Player>().Gethit();
    }
}
