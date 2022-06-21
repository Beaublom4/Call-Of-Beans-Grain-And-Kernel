using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Health : MonoBehaviour
{
    [HideInInspector] public PhotonView pv;

    public PlayerManager pm;
    public int team;

    [SerializeField] MeshRenderer playerRenderer;
    [SerializeField] Material enemyMaterial;

    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private float regenWaitTime;
    [SerializeField] private float regenSpeed;

    IEnumerator regenCoroutine;

    bool assist;

    bool dead;
    [SerializeField] GameObject deathParticlePrefab;

    [Header("Perks")]
    public bool flakJacket;
    public bool ghost;
    [SerializeField] GameObject scavengerPackPrefab;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        currentHealth = maxHealth;
        if (!pv.IsMine)
            return;
        pv.RPC("SyncTeam", RpcTarget.All, PhotonManager.team);
        if(PlayerInfoManager.perk1 != null && MultiplayerManager.hasPerks)
            pv.RPC("SyncPerk", RpcTarget.All, PlayerInfoManager.perk1.perkId);
    }
    [PunRPC]
    void SyncTeam(int _team)
    {
        team = _team;
        if (team != PhotonManager.team)
            playerRenderer.material = enemyMaterial;
    }
    [PunRPC]
    void SyncPerk(int perk1Id)
    {
        switch (perk1Id)
        {
            case 0:
                flakJacket = true;
                break;
            case 1:
                ghost = true;
                break;
        }
    }
    public void GetHit(float _damage, PlayerShoot player, bool isZombie, Vector3 hitFrom, bool wasHeadshot, string weaponName, bool wasExplosion, bool wasKnife)
    {
        pv.RPC("RPC_GetHit", RpcTarget.All, _damage, hitFrom, PhotonManager.playerId, wasHeadshot, PhotonNetwork.NickName, weaponName, wasExplosion, wasKnife);
    }
    [PunRPC]
    void RPC_GetHit(float _damage, Vector3 hitFromLoc, byte playerShotId, bool wasHeadshot, string killedByName, string killedByWeapon, bool wasExplosion, bool wasKnife)
    {
        if (dead)
            return;

        currentHealth -= _damage;

        if (currentHealth > 0 && playerShotId == PhotonManager.playerId)
        {
            assist = true;
        }

        if (!pv.IsMine)
            return;

        HudManager.Instance.ShowRedScreen(currentHealth, maxHealth);

        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);
        regenCoroutine = HealthRegen();
        StartCoroutine(regenCoroutine);

        FindObjectOfType<HudManager>().GetHit(transform, hitFromLoc, playerShotId, _damage);

        if (currentHealth <= 0)
        {
            pv.RPC("RPC_SyncKiller", RpcTarget.All, playerShotId, wasHeadshot, PhotonManager.playerId, wasExplosion, wasKnife, killedByWeapon);
            pv.RPC("DeadMessage", RpcTarget.All, PhotonNetwork.NickName, killedByName);
            pv.RPC("RPC_DeathParticle", RpcTarget.Others);
            Dead(hitFromLoc);
            HudManager.Instance.ShowDeadBy(killedByName, killedByWeapon);
            ScavengerPack[] packs = FindObjectsOfType<ScavengerPack>();
            foreach(ScavengerPack pack in packs)
            {
                Destroy(pack);
            }
            GameManager.Instance.DiedCheck(wasKnife);
        }
    }
    [PunRPC]
    void RPC_DeathParticle()
    {
        Instantiate(deathParticlePrefab, transform.position, transform.rotation, null);
    }
    [PunRPC]
    void RPC_SyncKiller(byte playerShotId, bool wasHeadshot, byte killedPlayerId, bool wasExplosion, bool wasKnife, string killedByWeapon)
    {
        dead = true;

        if (PhotonManager.playerId == playerShotId)
        {
            if (playerShotId == killedPlayerId)
                return;

            if (PlayerInfoManager.perk2 != null && PlayerInfoManager.perk2.perkId == 1 && MultiplayerManager.hasPerks)
            {
                if (!wasExplosion)
                {
                    Instantiate(scavengerPackPrefab, transform.position - new Vector3(0, 1, 0), transform.rotation, null);
                }
            }

            GameManager.Instance.AddKill(wasKnife);

            if (wasExplosion)
            {
                foreach (GameObject g in PlayerInfoManager.staticGunSlots)
                {
                    if (g.GetComponent<Gun>().gun.gunName == killedByWeapon)
                    {
                        string keyName = killedByWeapon + "headshots";
                        int currentHeadshots = PlayerPrefs.GetInt(keyName) + 1;
                        PlayerPrefs.SetInt(keyName, currentHeadshots);
                        Debug.Log(PlayerPrefs.GetInt(keyName));
                    }
                }
            }
            if (wasKnife)
            {
                foreach (GameObject g in PlayerInfoManager.staticGunSlots)
                {
                    if (g.GetComponent<Gun>().gun.gunName == "Riot-Shield")
                    {
                        string keyName = killedByWeapon + "headshots";
                        int currentHeadshots = PlayerPrefs.GetInt(keyName) + 1;
                        PlayerPrefs.SetInt(keyName, currentHeadshots);
                        Debug.Log(PlayerPrefs.GetInt(keyName));
                    }
                }
            }

            if (!wasHeadshot)
            {
                PlayerInfoManager.kills++;
            }
            else
            {
                PlayerInfoManager.headshots++;
                foreach(GameObject g in PlayerInfoManager.staticGunSlots)
                {
                    if(g.GetComponent<Gun>().gun.gunName == killedByWeapon)
                    {
                        string keyName = killedByWeapon +"headshots";
                        int currentHeadshots = PlayerPrefs.GetInt(keyName) + 1;
                        PlayerPrefs.SetInt(keyName, currentHeadshots);
                    }
                }
            }
        }
        else
        {
            if (!pv.IsMine && assist)
            {
                GameManager.Instance.AddAssist();
            }
        }
    }
    [PunRPC]
    void DeadMessage(string player, string killedBy)
    {
        HudManager.Instance.SpawnMessage(killedBy + " killed " + player);
    }
    IEnumerator HealthRegen()
    {
        yield return new WaitForSeconds(regenWaitTime);
        while(currentHealth < maxHealth)
        {
            currentHealth += regenSpeed * Time.deltaTime;
            HudManager.Instance.ShowRedScreen(currentHealth, maxHealth);
            pv.RPC("RPC_SyncHealth", RpcTarget.Others, currentHealth);
            yield return null;
        }
        currentHealth = maxHealth;
        HudManager.Instance.ShowRedScreen(currentHealth, maxHealth);
        pv.RPC("RPC_SyncHealth", RpcTarget.Others, currentHealth);
    }
    [PunRPC]
    void RPC_SyncHealth(float _health)
    {
        assist = false;
        currentHealth = _health;
    }
    void Dead(Vector3 hitFrom)
    {
        HudManager.Instance.SyncScoreboardNumbers(pm, 0, 0, 1, 0);

        foreach (Camera c in GetComponent<PlayerController>().cameras)
        {
            c.enabled = false;
        }

        pm.currentFakePlayer = Instantiate(pm.fakePlayer, transform.position, transform.rotation);
        pm.SpawnDeathCam(GetComponent<PlayerController>().cameras[0].transform.position, hitFrom);

        PhotonNetwork.Destroy(gameObject);
    }
    public void PlayerGetHit(float _damage, ZombieBasics zombie, bool isPlayer)
    {
        currentHealth -= _damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    //public void PlayerGetHit(float _damage, ZombieBasics zombie, bool isPlayer)
    //{
    //    currentHealth -= _damage;
    //    if (currentHealth <= 0)
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    public void Flash()
    {
        pv.RPC("RPC_Flash", RpcTarget.All);
    }
    [PunRPC]
    void RPC_Flash()
    {
        if (!pv.IsMine)
            return;
        HudManager.Instance.FlashBang();
    }
}
