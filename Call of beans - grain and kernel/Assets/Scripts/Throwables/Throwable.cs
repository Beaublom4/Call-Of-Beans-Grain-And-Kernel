using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Throwable : MonoBehaviour
{
    PhotonView pv;

    public ThrowableScrObj throwable;
    [SerializeField] LayerMask hittableMask;
    [SerializeField] GameObject explosion;
    [SerializeField] float particleDestroyDelay = 2;
    PlayerShoot ps;

    public bool started;
    public float currentDelay;
    Vector3 throwLoc;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void StartNade(float _currentDelay, Vector3 _throwLoc, PlayerShoot _ps)
    {
        ps = _ps;
        currentDelay = _currentDelay;
        started = true;
        Rigidbody r = GetComponent<Rigidbody>();
        r.useGravity = true;
        r.constraints = RigidbodyConstraints.None;
        float throwMultiplier = 1;
        if (PlayerInfoManager.perk3 != null && PlayerInfoManager.perk3.perkId == 1 && MultiplayerManager.hasPerks)
        {
            Debug.Log("Throw perk test");
            throwMultiplier = 1.5f;
        }
        r.AddRelativeForce(throwable.throwInput * throwMultiplier);
        throwLoc = _throwLoc;
    }
    private void Update()
    {
        if (!started)
            return;

        if (currentDelay > 0)
        {
            currentDelay -= Time.deltaTime;
            if (currentDelay <= 0)
            {
                started = false;
                RangeCheck();
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, throwable.explodeRange);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Collide(collision.transform);
        if (throwable.explodeOnImpact)
        {
            started = false;
            RangeCheck();
        }
    }
    void RangeCheck()
    {
        Collider[] col = Physics.OverlapSphere(transform.position, throwable.explodeRange, hittableMask);

        Transform latestRoot = null;
        foreach (Collider c in col)
        {
            if (latestRoot == c.transform.root)
                continue;
            latestRoot = c.transform.root;

            if (c.tag == "Player")
            {
                if (c.GetComponent<Health>().team == PhotonManager.team)
                {
                    if (!c.GetComponent<PhotonView>().IsMine)
                        continue;
                }

                float dist = throwable.explodeRange - Vector3.Distance(transform.position, c.transform.position);
                float fallOff = dist / throwable.explodeRange;
                float calcDmg = throwable.explodeDamage * fallOff;

                //FlakJacket
                if (c.GetComponent<Health>().flakJacket)
                    calcDmg *= .75f;

                if (calcDmg < 0)
                    calcDmg = 0;

                HudManager.Instance.ShowHitMarker();
                ps.soundManager.HitMarkerSound();
                DoNade(c.GetComponent<Health>(), calcDmg);
            }
        }
        pv.RPC("Particles", RpcTarget.All);
        PhotonNetwork.Destroy(gameObject);
    }
    void Collide(Transform hit)
    {
        switch (throwable.nadeId)
        {
            case 1:
                Rigidbody r = GetComponent<Rigidbody>();
                r.useGravity = false;
                r.constraints = RigidbodyConstraints.FreezeAll;
                if (hit.tag == "Player")
                    transform.SetParent(hit);
                break;
        }
    }
    void DoNade(Health health, float damage)
    {
        switch (throwable.nadeId)
        {
            case 0:
                if (!pv.IsMine)
                    if (health.team == PhotonManager.team)
                        return;
                health.GetHit(damage, null, false, transform.position, false, throwable.name, true, false);
                break;
            case 1:
                if (!pv.IsMine)
                    if (health.team == PhotonManager.team)
                        return;
                health.GetHit(damage, null, false, transform.position, false, throwable.name, true, false);
                break;
            case 2:
                health.GetHit(damage, null, false, transform.position, false, throwable.name, true, false);
                health.Flash();
                break;
        }
    }
    [PunRPC]
    void Particles()
    {
        Destroy(Instantiate(explosion, transform.position, Quaternion.Euler(0, 0, 0), null), particleDestroyDelay);
    }
}
