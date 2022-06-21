using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Chopper : MonoBehaviour
{
    PhotonView pv;
    public float aliveTime;
    public Health yourHealth;

    [SerializeField] Transform hitPoint;

    [SerializeField] float damage;
    [SerializeField] float shootCooldown;
    bool canShoot = true;

    [SerializeField] float changeTargetSpeed;
    [SerializeField] bool hasTarget, isShooting;
    [SerializeField] Health currentTarget;

    [SerializeField] LayerMask hittableMask;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;
        if (!hasTarget)
        {
            Health[] playerHealths = FindObjectsOfType<Health>();
            List<Health> enemyHealths = new List<Health>();

            foreach(Health health in playerHealths)
            {
                if(health.team != GetComponent<ScorestreakHealth>().team)
                {
                    enemyHealths.Add(health);
                }
            }

            if (playerHealths.Length == 0)
                return;


            Health newTarget = enemyHealths[Random.Range(0, enemyHealths.Count)];

            if (newTarget == yourHealth)
                return;

            currentTarget = newTarget;
            hasTarget = true;
            isShooting = true;
            canShoot = true;
        }
        if (isShooting)
        {
            if (!canShoot)
                return;

            if (currentTarget == null)
            {
                CantFindTarget();
                return;
            }

            RaycastHit hit;
            if (Physics.Linecast(hitPoint.position, currentTarget.transform.position, out hit, hittableMask))
            {
                Debug.DrawLine(hitPoint.position, currentTarget.transform.position, Color.red);
                canShoot = false;
                currentTarget.GetHit(damage, null, false, hitPoint.position, false, "Chopper", false, false);
                StartCoroutine(ShootCooldown());
            }
            else
            {
                CantFindTarget();
            }
        }
    }
    void CantFindTarget()
    {
        isShooting = false;
        canShoot = true;
        StartCoroutine(Changing());
    }
    IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
    IEnumerator Changing()
    {
        yield return new WaitForSeconds(changeTargetSpeed);
        hasTarget = false;
    }
}
