using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    PhotonView pv;

    public float moveSpeed, damage;
    public bool force;
    public PlayerShoot ps;
    public Vector3 shotFromPos;
    [SerializeField] bool explosive;
    [SerializeField] GameObject explosionPrefab;
    public float explotionRange;
    public LayerMask explotionMask;
    public string gunName;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            StartCoroutine(Timer());
        }
    }
    IEnumerator Timer()
    {
        yield return new WaitForSeconds(50);
        Explode(null);
    }
    private void Start()
    {
        if (!pv.IsMine)
            return;
        if (force)
        {
            GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, moveSpeed));
        }
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;

        if (!force)
        {
            transform.Translate(0, 0, moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if(collision != null)
            if (collision.collider.isTrigger)
                return;

        Explode(collision);
    }
    void Explode(Collision collision)
    {
        if (!explosive)
        {
            if (collision == null)
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }
            if (collision.collider.tag == "Player")
            {
                if (collision.collider.GetComponent<Health>().team != PhotonManager.team)
                {
                    collision.collider.GetComponent<Health>().GetHit(damage, ps, false, shotFromPos, false, gunName, false, false);
                    HudManager.Instance.ShowHitMarker();
                    ps.soundManager.HitMarkerSound();
                }
            }
            else if (collision.collider.tag == "Glasses" && collision.collider.transform.root.tag == "Player")
            {
                if (collision.collider.transform.root.GetComponent<Health>().team != PhotonManager.team) 
                {
                    collision.collider.transform.root.GetComponent<Health>().GetHit(damage * 2, ps, false, shotFromPos, true, gunName, false, false);
                    HudManager.Instance.ShowHitMarker();
                    ps.soundManager.HitMarkerSound();
                }
            }
            else if (collision.collider.tag == "Streak")
            {
                if (collision.collider.transform.root.GetComponent<ScorestreakHealth>().team != PhotonManager.team)
                {
                    collision.collider.transform.root.GetComponent<ScorestreakHealth>().GetHit(damage);
                    HudManager.Instance.ShowHitMarker();
                    ps.soundManager.HitMarkerSound();
                }
            }
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explotionRange, explotionMask);
            pv.RPC("Particles", RpcTarget.All);
            Transform latestRoot = null;
            foreach (Collider c in colliders)
            {
                if (latestRoot == c.transform.root)
                    return;
                latestRoot = c.transform.root;

                if (c.tag == "Player")
                {
                    if (c.GetComponent<Health>().team == PhotonManager.team)
                    {
                        if(!c.GetComponent<PhotonView>().IsMine)
                            return;
                    }

                    float dist = explotionRange - Vector3.Distance(transform.position, c.transform.position);
                    float fallOff = dist / explotionRange;
                    float calcDmg = damage * fallOff;

                    //FlakJacket
                    if (c.GetComponent<Health>().flakJacket)
                        calcDmg *= .75f;

                    if (calcDmg < 0)
                        calcDmg = 0;

                    c.GetComponent<Health>().GetHit(calcDmg, ps, false, shotFromPos, false, gunName, true, false);
                    HudManager.Instance.ShowHitMarker();
                    ps.soundManager.HitMarkerSound();
                }
                else if (c.tag == "Streak")
                {
                    if (c.transform.root.GetComponent<ScorestreakHealth>().team == PhotonManager.team)
                        return;

                    c.transform.root.GetComponent<ScorestreakHealth>().GetHit(damage);
                    HudManager.Instance.ShowHitMarker();
                    ps.soundManager.HitMarkerSound();
                }
            }
        }
        PhotonNetwork.Destroy(gameObject);
    }
    [PunRPC]
    void Particles()
    {
        Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.Euler(0, 0, 0)), 2);
    }
}
