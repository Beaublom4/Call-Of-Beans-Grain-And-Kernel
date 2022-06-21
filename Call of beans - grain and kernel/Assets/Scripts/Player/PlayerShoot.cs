using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerShoot : MonoBehaviour
{
    PhotonView pv;
    [HideInInspector] public PlayerManager pm;

    PlayerController pc;
    public Transform mainCam;
    public Transform weaponObj;
    bool setUp;

    [Header("Gun Inv")]
    [SerializeField] private GameObject[] guns;
    [SerializeField] private GameObject[] allGunsList;
    [SerializeField] private int currentSlot;
    [SerializeField] Material[] gunCamos;

    public GameObject currentGun;
    [HideInInspector] public Gun currentGunScript;
    bool isSwitching;

    [Header("Shooting")]
    [SerializeField] private LayerMask shootMask;
    [SerializeField] private GameObject hitPrefab;
    bool canShoot = true;
    bool isShooting = false;

    [Header("Aiming")]
    [SerializeField] private Transform aimPos, handPos;
    float normalFov;

    [HideInInspector] public bool aiming;
    public bool fullyAimed;
    IEnumerator aimingCoroutine;

    [Header("Throwables")]

    [SerializeField] Transform throwLoc;
    [SerializeField] GameObject currentNade;
    [SerializeField] ThrowableScrObj throwable;
    [SerializeField] float nadeDelay;
    int nadeCount;
    bool canThrow = true;
    
    bool cook;
    float currentDelay;

    [Header("Knifing")]
    [SerializeField] GameObject knifeObj;
    [SerializeField] BoxCollider knifeCol;
    [SerializeField] Animator knifeAnim;
    [SerializeField] float knifeDamage;
    [SerializeField] float knifeTime;

    bool isKnifing;
    IEnumerator knifeCoroutine;

    [Header("Reloading")]
    bool isReloading;
    IEnumerator reloadCoroutine;

    [Header("Sounds")]
    [HideInInspector] public SoundManager soundManager;

    [Header("Perks")]
    int strongReflexes = 1;
    float sleightOfHand = 1;

    #region SetUp
    public void SetUp(PlayerManager _pm)
    {
        pm = _pm;
    }
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        pc = GetComponent<PlayerController>();
        normalFov = mainCam.GetComponent<Camera>().fieldOfView;
        soundManager = GetComponentInChildren<SoundManager>();
        if (MultiplayerManager.hasPerks)
        {
            if (PlayerInfoManager.perk1 != null && PlayerInfoManager.perk1.perkId == 2)
                strongReflexes = 2;
            if (PlayerInfoManager.perk2 != null && PlayerInfoManager.perk2.perkId == 2)
                sleightOfHand = 1.25f;
        }
    }
    private void Start()
    {
        if (!pv.IsMine)
            return;

        setUp = true;

        if (!GameManager.Instance.gamemodeLoadout)
        {
            for (int i = 0; i < guns.Length; i++)
            {
                foreach (GameObject g in allGunsList)
                {
                    if (g.GetComponent<Gun>().gun.gunID == PlayerInfoManager.staticGunSlots[i].GetComponent<Gun>().gun.gunID)
                    {
                        guns[i] = g;
                        if(PlayerInfoManager.staticGunCamos[i] > 0)
                            g.GetComponent<Gun>().SetCamo(gunCamos[PlayerInfoManager.staticGunCamos[i] - 1]);
                    }
                }
                pv.RPC("RPC_SyncGuns", RpcTarget.OthersBuffered, i, guns[i].GetComponent<Gun>().gun.gunID, PlayerInfoManager.staticGunCamos[i] - 1);
            }
            SelectGun(0, 0);

            throwable = PlayerInfoManager.staticThrowableSlot;
            nadeCount = throwable.nadeCount;
            HudManager.Instance.ShowNades(throwable, nadeCount);
        }
        else
        {
            GameObject[] loadout = GameManager.Instance.GetLoadout();
            for (int i = 0; i < guns.Length; i++)
            {
                foreach (GameObject g in allGunsList)
                {
                    if (loadout[i] == null)
                        continue;
                    if (g.GetComponent<Gun>().gun.gunID == loadout[i].GetComponent<Gun>().gun.gunID)
                    {
                        guns[i] = g;
                    }
                }
                if(guns[i] != null)
                    pv.RPC("RPC_SyncGuns", RpcTarget.OthersBuffered, i, guns[i].GetComponent<Gun>().gun.gunID, 0);
            }
            SelectGun(0, 0);

            if (loadout[2] != null)
            {
                throwable = loadout[2].GetComponent<Throwable>().throwable;
                nadeCount = throwable.nadeCount;
                HudManager.Instance.ShowNades(throwable, nadeCount);
            }
            else
            {
                HudManager.Instance.HideNades();
            }
        }
    }
    public void ChangeGun(GameObject[] loadout)
    {
        for (int i = 0; i < guns.Length; i++)
        {
            foreach (GameObject g in allGunsList)
            {
                if (loadout[i] == null)
                    continue;
                if (g.GetComponent<Gun>().gun.gunID == loadout[i].GetComponent<Gun>().gun.gunID)
                {
                    guns[i] = g;
                }
                else if (g.GetComponent<Gun>().gunHolder.activeSelf)
                {
                    g.GetComponent<Gun>().gunHolder.SetActive(false);
                }
            }
            if (guns[i] != null)
                pv.RPC("RPC_SyncGuns", RpcTarget.OthersBuffered, i, guns[i].GetComponent<Gun>().gun.gunID, 0);
        }
        SelectGun(0, 0);
    }
    [PunRPC]
    void RPC_SyncGuns(int gunSlot,  int gunId, int camoId)
    {
        foreach (GameObject g in allGunsList)
        {
            if (g.GetComponent<Gun>().gun.gunID == gunId)
            {
                guns[gunSlot] = g;
                if (camoId > 0)
                    g.GetComponent<Gun>().SetCamo(gunCamos[camoId]);
            }
            else if (g.GetComponent<Gun>().gunHolder.activeSelf)
            {
                g.GetComponent<Gun>().gunHolder.SetActive(false);
            }
        }
    }
    #endregion
    private void Update()
    {
        if (!pv.IsMine)
            return;

        KnifeInput();
        InventoryManagementInputs();
        GunInputs();
        AimInput();
        ReloadInputs();
        ThrowableInput();
    }
    #region Knife
    void KnifeInput()
    {
        if (Input.GetButtonDown("Knife"))
        {
            if (isKnifing)
                return;

            isKnifing = true;

            if (isReloading)
            {
                isReloading = false;
                currentGunScript.anim.SetBool("Reloading", false);
                StopCoroutine(reloadCoroutine);
            }

            StartCoroutine(Knife());
        }
    }
    [PunRPC]
    void ShowKnife(bool active)
    {
        knifeObj.SetActive(active);
    }
    IEnumerator Knife()
    {
        currentGunScript.anim.SetBool("Active", false);
        knifeObj.GetComponentInChildren<KnifeHitBox>().SetUp(mainCam, knifeDamage, this);
        knifeObj.SetActive(true);
        knifeCol.enabled = true;
        pv.RPC("ShowKnife", RpcTarget.Others, true);
        knifeAnim.SetTrigger("Knife");
        yield return new WaitForSeconds(knifeTime);
        currentGunScript.anim.SetBool("Active", true);
        knifeCol.enabled = false;
        knifeObj.SetActive(false);
        pv.RPC("ShowKnife", RpcTarget.Others, false);
        isKnifing = false;
    }
    #endregion
    #region Inventory
    void InventoryManagementInputs()
    {
        if (guns[1] != null)
        {
            if (Input.GetButtonDown("Primary"))
            {
                int oldSlot = currentSlot;
                currentSlot = 0;
                if (isReloading)
                {
                    StopCoroutine(reloadCoroutine);
                }

                SelectGun(oldSlot, currentSlot);
            }
            else if (Input.GetButtonDown("Secondary"))
            {

                int oldSlot = currentSlot;
                currentSlot = 1;
                if (isReloading)
                {
                    StopCoroutine(reloadCoroutine);
                }
                SelectGun(oldSlot, currentSlot);
            }

            if (Input.mouseScrollDelta.y > 0)
            {
                int oldSlot = currentSlot;
                currentSlot++;
                if (currentSlot >= guns.Length)
                    currentSlot = 0;
                SelectGun(oldSlot, currentSlot);
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                int oldSlot = currentSlot;
                currentSlot--;
                if (currentSlot < 0)
                    currentSlot = guns.Length - 1;
                SelectGun(oldSlot, currentSlot);
            }
        }
    }
    void SelectGun(int oldSlot, int slot)
    {
        if (guns[slot] == null || isSwitching)
            return;

        Gun newGun = guns[slot].GetComponent<Gun>();
        if (aiming)
        {
            if (newGun.gun.scopeFOV == guns[oldSlot].GetComponent<Gun>().gun.scopeFOV)
            {
                Switch(oldSlot, slot);
            }
            else
            {
                isSwitching = true;
                StopAiming();
                StartCoroutine(SwitchCoroutine(oldSlot, slot));
            }
        }
        else
        {
            Switch(oldSlot, slot);
        }
    }
    IEnumerator SwitchCoroutine(int oldSlot, int slot)
    {
        yield return new WaitForSeconds(currentGunScript.gun.scopeTime);
        Switch(oldSlot, slot);
    }
    void Switch(int oldSlot, int slot)
    {
        if(currentGun != null)
            StopAiming();
        isReloading = false;
        currentGun = guns[slot];
        currentGunScript = currentGun.GetComponent<Gun>();
        if (oldSlot != slot)
        {
            guns[oldSlot].GetComponent<Gun>().gunHolder.SetActive(false);
            guns[oldSlot].GetComponent<Gun>().anim.SetBool("Reloading", false);
            guns[oldSlot].GetComponent<Gun>().anim.SetBool("Active", false);
        }
        currentGunScript.GetComponent<Gun>().gunHolder.SetActive(true);
        currentGunScript.anim.SetInteger("Ammo", currentGunScript.currentMag);
        currentGunScript.anim.SetBool("Active", true);

        isSwitching = false;
        canShoot = true;

        if (!aiming && pc.scopeSensMultiplier != 1)
            pc.scopeSensMultiplier = 1;

        HudManager.Instance.ShowGunInfo(currentGunScript);
        HudManager.Instance.ShowAmmo(currentGunScript);

        pv.RPC("RPC_SelectGun", RpcTarget.Others, slot, oldSlot);
    }
    [PunRPC]
    void RPC_SelectGun(int gunSlot, int oldSlot)
    {
        guns[oldSlot].GetComponent<Gun>().gunHolder.SetActive(false);
        guns[gunSlot].GetComponent<Gun>().gunHolder.SetActive(true);
    }
    public void FullAmmo()
    {
        foreach(GameObject game in guns)
        {
            Gun g = game.GetComponent<Gun>();
            g.currentMag = g.gun.magSize;
            g.currentAmmo = g.gun.maxAmmo;
        }
        HudManager.Instance.ShowAmmo(currentGunScript);
    }
    #endregion
    #region Shooting
    void GunInputs()
    {
        if (HudManager.Instance.menuObj.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isShooting = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isShooting = false;
        }

        if (isShooting)
        {
            if (currentGunScript.currentMag <= 0)
                return;
            if (!canShoot)
                return;

            if (!currentGunScript.gun.autoFire)
                isShooting = false;

            StartCoroutine(ShootCooldown(currentGunScript.gun.gunFireRate));
            GetComponentInChildren<MiniMapPlayer>().ShowLocOnMap();

            if (isReloading)
            {
                isReloading = false;
                currentGunScript.anim.SetBool("Reloading", false);
                StopCoroutine(reloadCoroutine);
            }

            canShoot = false;

            if (!currentGunScript.gun.burstFire)
            {
                //gun adds not burst fire
                currentGunScript.currentMag--;
                currentGunScript.anim.SetTrigger("Shoot");
                float recoil = currentGunScript.gun.gunRecoil;
                if (fullyAimed) recoil /= currentGunScript.gun.gunAimDivider;
                pc.AddRecoil(recoil, currentGunScript.gun.gunRecoilTime);
                HudManager.Instance.ShowAmmo(currentGunScript);
            }

            if (currentGunScript.gun.shootProjectile)
            {
                //Projectile fire
                GameObject currentProjectile = PhotonNetwork.Instantiate(currentGunScript.gun.projectilePrefab.name, currentGunScript.projectilePos.position, currentGunScript.projectilePos.rotation);
                Projectile p = currentProjectile.GetComponent<Projectile>();
                p.moveSpeed = currentGunScript.gun.projectileSpeed;
                p.damage = currentGunScript.gun.gunDamageCurve.Evaluate(0);
                p.explotionRange = currentGunScript.gun.explotionRange;
                p.ps = this;
                p.shotFromPos = mainCam.position;
                p.gunName = currentGunScript.gun.gunName;

                return;
            }

            if (!currentGunScript.gun.shootsShells)
            {
                if (!currentGunScript.gun.burstFire)
                {
                    //Normal fire
                    Quaternion shootDir = Quaternion.Euler(0, 0, 0);
                    float aimedAccuracy = currentGunScript.gun.gunAimingAccuracy / 10;
                    float notAimedAccuracy = currentGunScript.gun.gunNotAimingAccuracy / 10;
                    if (fullyAimed) shootDir.eulerAngles = new Vector3(Random.Range(-aimedAccuracy, aimedAccuracy), Random.Range(-aimedAccuracy, aimedAccuracy), Random.Range(-aimedAccuracy, aimedAccuracy));
                    else if (!fullyAimed) shootDir.eulerAngles = new Vector3(Random.Range(-notAimedAccuracy, notAimedAccuracy), Random.Range(-notAimedAccuracy, notAimedAccuracy), Random.Range(-notAimedAccuracy, notAimedAccuracy));
                    ShootBullet(shootDir, currentGunScript.gun);
                }
                else
                {
                    //Burst fire
                    StartCoroutine(BurstFire());
                }
            }
            else
            {
                //Shell fire
                Quaternion shootDir = Quaternion.Euler(0, 0, 0);
                float aimedAccuracy = currentGunScript.gun.gunAimingAccuracy / 10;
                float notAimedAccuracy = currentGunScript.gun.gunNotAimingAccuracy / 10;
                for (int i = 0; i < currentGunScript.gun.shellCount; i++)
                {
                    if (fullyAimed) shootDir.eulerAngles = new Vector3(Random.Range(-aimedAccuracy, aimedAccuracy), Random.Range(-aimedAccuracy, aimedAccuracy), Random.Range(-aimedAccuracy, aimedAccuracy));
                    else if (!fullyAimed) shootDir.eulerAngles = new Vector3(Random.Range(-notAimedAccuracy, notAimedAccuracy), Random.Range(-notAimedAccuracy, notAimedAccuracy), Random.Range(-notAimedAccuracy, notAimedAccuracy));
                    ShootBullet(shootDir, currentGunScript.gun);
                }
            }
        }
    }
    IEnumerator BurstFire()
    {
        for (int i = 0; i < currentGunScript.gun.burstRonds; i++)
        {
            currentGunScript.currentMag--;
            currentGunScript.anim.SetTrigger("Shoot");
            float recoil = currentGunScript.gun.gunRecoil;
            if (fullyAimed) recoil /= currentGunScript.gun.gunAimDivider; 
            pc.AddRecoil(recoil, currentGunScript.gun.gunRecoilTime);

            Quaternion shootDir = Quaternion.Euler(0, 0, 0);
            float aimedAccuracy = currentGunScript.gun.gunAimingAccuracy / 10;
            float notAimedAccuracy = currentGunScript.gun.gunNotAimingAccuracy / 10;
            if (fullyAimed) shootDir.eulerAngles = new Vector3(Random.Range(-aimedAccuracy, aimedAccuracy), Random.Range(-aimedAccuracy, aimedAccuracy), Random.Range(-aimedAccuracy, aimedAccuracy));
            else if (!fullyAimed) shootDir.eulerAngles = new Vector3(Random.Range(-notAimedAccuracy, notAimedAccuracy), Random.Range(-notAimedAccuracy, notAimedAccuracy), Random.Range(-notAimedAccuracy, notAimedAccuracy));
            ShootBullet(shootDir, currentGunScript.gun);
            HudManager.Instance.ShowAmmo(currentGunScript);

            yield return new WaitForSeconds(currentGunScript.gun.burstSpeed);
        }
    }
    void ShootBullet(Quaternion shootDir, GunScrObj gun)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCam.position, shootDir * mainCam.forward, out hit, 1000, shootMask, QueryTriggerInteraction.Ignore))
        {
            float distance = Vector3.Distance(mainCam.position, hit.point);
            float calcDamage = gun.gunDamageCurve.Evaluate(distance);
            if (gun.shootsShells)
                calcDamage /= gun.shellCount;

            Destroy(Instantiate(hitPrefab, hit.point, Quaternion.identity, null), 1);

            if(hit.collider.tag == "Player")
            {
                if (hit.collider.GetComponent<Health>().team == PhotonManager.team)
                    return;
                hit.collider.GetComponent<Health>().GetHit(calcDamage, this, false, mainCam.position, false, gun.gunName, false, false);
                HudManager.Instance.ShowHitMarker();
                soundManager.HitMarkerSound();
            }
            else if(hit.collider.tag == "Glasses" && hit.collider.transform.root.tag == "Player")
            {
                if (hit.collider.transform.root.GetComponent<Health>().team == PhotonManager.team)
                    return;
                //Deadshot
                if (PlayerInfoManager.perk1 != null && PlayerInfoManager.perk1.perkId == 3 && MultiplayerManager.hasPerks)
                    calcDamage *= 1.2f;
                hit.collider.transform.root.GetComponent<Health>().GetHit(calcDamage * 1.4f, this, false, mainCam.position, true, gun.gunName,false, false);
                HudManager.Instance.ShowHitMarker();
                soundManager.HitMarkerSound();
            }
            else if(hit.collider.tag == "Streak")
            {
                if (!hit.collider.transform.root.GetComponent<PhotonView>().IsMine)
                {
                    if (hit.collider.transform.root.GetComponent<ScorestreakHealth>().team == PhotonManager.team)
                        return;
                    hit.collider.transform.root.GetComponent<ScorestreakHealth>().GetHit(calcDamage);
                    HudManager.Instance.ShowHitMarker();
                    soundManager.HitMarkerSound();
                }
            }

            //if (hit.collider.tag == "Zombie")
            //{
            //    hit.collider.GetComponent<Health>().GetHit(damage, this, true);
            //}
            //else if (hit.transform.parent.tag == "Zombie" && hit.collider.tag == "Glasses")
            //{
            //    if (hit.collider.transform.parent.tag == "Zombie")
            //    {
            //        hit.collider.GetComponentInParent<Health>().GetHit(damage * 2, this, true);
            //    }
            //}
        }
    }
    IEnumerator ShootCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }
    #endregion
    #region Aiming
    void AimInput()
    {
        if (Input.GetMouseButton(1))
        {
            if (isReloading || aiming || isSwitching)
                return;

            aiming = true;
            pc.isSprinting = false;
            if (aimingCoroutine != null)
                StopCoroutine(aimingCoroutine);
            aimingCoroutine = Aim();
            StartCoroutine(aimingCoroutine);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (isReloading)
                return;

            StopAiming();
        }
    }
    void StopAiming()
    {
        aiming = false;
        if (aimingCoroutine != null)
            StopCoroutine(aimingCoroutine);
        aimingCoroutine = Aim();
        StartCoroutine(aimingCoroutine);
    }
    IEnumerator Aim()
    {
        float timer = 0;
        Camera cam = mainCam.GetComponent<Camera>();

        if (aiming)
        {
            HudManager.Instance.crossHair.SetActive(false);
            if (currentGunScript.gun.scopedSensMultiplier != 0)
                pc.scopeSensMultiplier = currentGunScript.gun.scopedSensMultiplier;
            while (timer < currentGunScript.gun.scopeTime)
            {
                currentGun.transform.position = Vector3.Lerp(currentGun.transform.position, aimPos.position, timer / currentGunScript.gun.scopeTime);
                if (currentGunScript.gun.scopedSensMultiplier != 0)
                    pc.scopeSensMultiplier = Mathf.Lerp(pc.scopeSensMultiplier, currentGunScript.gun.scopedSensMultiplier, timer / currentGunScript.gun.scopeTime);
                if (currentGunScript.gun.scopeFOV != 0)
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentGunScript.gun.scopeFOV, timer / currentGunScript.gun.scopeTime);

                float aimedCheck = currentGunScript.gun.scopeTime * .2f;
                if (timer > aimedCheck)
                    fullyAimed = true;

                timer += Time.deltaTime * strongReflexes;
                yield return null;
            }
        }
        else
        {
            HudManager.Instance.crossHair.SetActive(true);
            if (currentGunScript.gun.scopedSensMultiplier != 0)
                pc.scopeSensMultiplier = 1;
            fullyAimed = false;
            while (timer < currentGunScript.gun.scopeTime)
            {
                currentGun.transform.position = Vector3.Lerp(currentGun.transform.position, handPos.position, timer / currentGunScript.gun.scopeTime);
                if (currentGunScript.gun.scopeFOV != 0)
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFov, timer / currentGunScript.gun.scopeTime);
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
    #endregion
    #region Reloading
    void ReloadInputs()
    {
        if (isReloading)
            return;
        if (Input.GetButtonDown("Reload"))
        {
            if (currentGunScript.currentAmmo <= 0 || currentGunScript.currentMag == currentGunScript.gun.magSize)
                return;

            StopAiming();
            isReloading = true;
            currentGunScript.anim.SetFloat("ReloadSpeed", sleightOfHand);
            currentGunScript.anim.SetBool("Reloading", true);
            if (reloadCoroutine != null)
                StopCoroutine(reloadCoroutine);
            reloadCoroutine = Reload();
            StartCoroutine(reloadCoroutine);
        }
    }
    IEnumerator Reload()
    {
        if (!currentGunScript.gun.reloadPerBullet)
        {
            yield return new WaitForSeconds(currentGunScript.gun.reloadTime / sleightOfHand);
            currentGunScript.currentAmmo += currentGunScript.currentMag;
            if (currentGunScript.currentAmmo > currentGunScript.gun.magSize)
            {
                currentGunScript.currentMag = currentGunScript.gun.magSize;
                currentGunScript.currentAmmo -= currentGunScript.gun.magSize;
            }
            else
            {
                currentGunScript.currentMag = currentGunScript.currentAmmo;
                currentGunScript.currentAmmo = 0;
            }
            currentGunScript.anim.SetBool("Reloading", false);
        }
        else
        {
            yield return new WaitForSeconds(currentGunScript.gun.reloadTime / sleightOfHand);
            int bulletsToReload = currentGunScript.gun.magSize - currentGunScript.currentMag;
            for (int i = 0; i < bulletsToReload; i++)
            {
                if (currentGunScript.currentAmmo <= 0)
                    break;
                currentGunScript.currentAmmo--;
                currentGunScript.currentMag++;
                HudManager.Instance.ShowAmmo(currentGunScript);
                yield return new WaitForSeconds(currentGunScript.gun.reloadBulletTime);
            }
            currentGunScript.anim.SetBool("Reloading", false);
            yield return new WaitForSeconds(currentGunScript.gun.reloadTime / sleightOfHand);
        }
        isReloading = false;
        HudManager.Instance.ShowAmmo(currentGunScript);
    }
    #endregion
    #region Throwables
    void ThrowableInput()
    {
        if (nadeCount <= 0)
            return;

        if (cook)
        {
            currentDelay -= Time.deltaTime;
            if (currentDelay < 0)
            {
                cook = false;
                currentDelay = .1f;
                Throw();
            }
        }

        if (currentNade != null && Input.GetButtonUp("Throw"))
        {
            cook = false;
            Throw();
        }

        if (!canThrow)
            return;

        if (Input.GetButtonDown("Throw"))
        {
            canThrow = false;
            currentNade = Instantiate(throwable.throwablePrefab, throwLoc);
            currentDelay = throwable.explodeDelay;
            cook = throwable.canCook;
        }
    }
    void Throw()
    {
        nadeCount--;
        cook = false;
        Destroy(currentNade);
        currentNade = PhotonNetwork.Instantiate(Path.Combine("Throwables", throwable.throwablePrefab.name), throwLoc.position, throwLoc.rotation);
        currentNade.GetComponent<Throwable>().StartNade(currentDelay, throwLoc.position, this);
        currentNade = null;
        HudManager.Instance.ShowNades(throwable, nadeCount);
        StartCoroutine(NadeDelay());
    }
    IEnumerator NadeDelay()
    {
        yield return new WaitForSeconds(nadeDelay);
        canThrow = true;
    }
    #endregion
}
