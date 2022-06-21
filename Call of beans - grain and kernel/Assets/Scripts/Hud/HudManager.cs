using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class HudManager : MonoBehaviour
{
    PhotonView pv;

    public static HudManager Instance;

    public GameObject crossHair;
    [SerializeField] GameObject deathHolder, deathHolder2;
    [SerializeField] TMP_Text deathByText, deathByWeaponText;

    [Header("Pause menu")]
    public GameObject menuObj;
    [SerializeField] MenuItem[] menus;

    [System.Serializable]
    public class GunType
    {
        public string typeName;
        public GunScrObj[] guns;
        public int sort;
    }
    [SerializeField] GunType[] gunTypes;

    [SerializeField] Transform gunTypesHolder;
    [SerializeField] GameObject gunTypePrefab, gunButtonPrefab;

    [SerializeField] GameObject gunPreviews;

    [SerializeField] GameObject[] gunSlotsShow;

    [Header("GunStats")]
    [SerializeField] TMP_Text statsGunNameText;
    [SerializeField] GameObject statsHolder;
    [SerializeField] GameObject[] statTabs;
    [SerializeField] TMP_Text statslvlUnlocked;

    [Header("Gun")]
    [SerializeField] TMP_Text gunName;
    [SerializeField] TMP_Text gunType;
    [SerializeField] TMP_Text gunAmmo;

    [Header("Throwables")]
    [SerializeField] GameObject ThrowableHolder;
    [SerializeField] Image nadeImage;
    [SerializeField] TMP_Text nadesLeft;

    [Header("Scoreboard")]
    public GameObject scoreboardObj;
    public TMP_Text gameName;
    public Transform playerHolder;
    public GameObject playerInfoPrefab;

    List<PlayerInfo> playerInfosList = new List<PlayerInfo>();

    public TMP_Text WinnerName;

    [Header("ScoreStreaks")]
    [SerializeField] GameObject scoreStreaksHolder;
    [SerializeField] Slider pointsBar;
    [SerializeField] Color cantUse, canUse;
    [SerializeField] Image[] streaks;
    [SerializeField] Transform highlighterStreak;

    [HideInInspector] public CounterUAV counterUAV;
    [SerializeField] GameObject counterUAVObj;

    [Header("Points")]
    [SerializeField] Image teamImage;
    public TMP_Text pointsToWin;
    [SerializeField] TMP_Text yourPoints;
    [SerializeField] TMP_Text otherPoints;

    [SerializeField] Sprite[] teamSprites;

    [Header("Get hit")]
    [SerializeField] Image redScreen;
    [SerializeField] Color normal, max;
    [SerializeField] float deadZone;

    [SerializeField] float getHitShowTime;
    [SerializeField] float delayTime;
    [SerializeField] Transform getHitHolder;
    [SerializeField] GameObject getHitPrefab;
    [SerializeField] float fadeTime;
    [SerializeField] Color beginColor, invis;
    bool canShow = true;

    [System.Serializable]
    public class HitLocStorer
    {
        public int playerId;
        public RectTransform hitLoc;
        public IEnumerator coroutine;
    }
    [SerializeField] List<HitLocStorer> storers = new List<HitLocStorer>();

    [Header("Flash bang")]
    [SerializeField] Image flash;
    [SerializeField] float flashTime, flashFade;
    IEnumerator flashBang;
    [SerializeField] Color flashGone;

    [Header("Crosshair")]
    [SerializeField] float crosshairSpeed;
    [SerializeField] float wantedDist;
    [SerializeField] float[] setDistances;
    [SerializeField] RectTransform[] crosshairPieces;

    [Header("Hitmarker")]
    [SerializeField] Image[] hitMarker;
    [SerializeField] float hitMarkerShowTime;
    IEnumerator hitCoroutine;
    [SerializeField] Color marker1, marker2;

    [Header("Get points")]
    [SerializeField] GameObject pointsGetObj;
    [SerializeField] float showTime;
    int currentGetPoints;
    IEnumerator showCoroutine;

    [Header("Buy")]
    public TMP_Text buyText;
    IEnumerator buyCoroutine;

    [Header("Capture")]
    [SerializeField] GameObject captureObj;
    [SerializeField] Slider captureSlider;

    [Header("Messages")]
    [SerializeField] GameObject messagePrefab;
    [SerializeField] Transform messageHolder;

    [Header("Camos")]
    [SerializeField] GameObject[] camoHolders;
    [SerializeField] Image[] camoBases;
    [SerializeField] GameObject[] displayHeadshotsText;
    [SerializeField] Sprite noneSprite;

    //*********************************************************************************************************************************************************\\

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        Instance = this;
    }
    private void Start()
    {
        SetUpPoints();
        SetUpScoreboard();
        for (int i = 0; i < PlayerInfoManager.staticGunSlots.Length; i++)
        {
            SyncGun(PlayerInfoManager.staticGunSlots[i].GetComponent<Gun>().gun, i);
        }
    }
    private void Update()
    {
        CrossHair();
        UpdateScoreboard();
        PauseMenuUpdate();

        if (counterUAV != null)
            counterUAVObj.SetActive(true);
        else
            counterUAVObj.SetActive(false);
    }
    public void Death(bool active)
    {
        deathHolder.SetActive(active);
        deathHolder2.SetActive(!active);
    }
    public void ShowDeadBy(string playerName, string weaponName)
    {
        deathByText.text = playerName;
        deathByWeaponText.text = weaponName;
    }
    public void HitShowOff()
    {
        StopAllCoroutines();
        storers.Clear();
        foreach (Transform t in getHitHolder)
        {
            Destroy(t.gameObject);
        }
        foreach(Image i in hitMarker)
        {
            i.color = marker2;
        }
        canShow = true;
        pointsGetObj.SetActive(false);
        currentGetPoints = 0;
        ShowRedScreen(1, 1);
        flash.color = flashGone;
    }
    #region Pauze menu
    void PauseMenuUpdate()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            MenuActivation();
        }
    }
    public void MenuActivation()
    {
        if (menuObj.activeSelf == false)
        {
            menuObj.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            menuObj.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void ChangeMenu(string menuName)
    {
        foreach (MenuItem m in menus)
        {
            if (m.menuName == menuName)
                m.gameObject.SetActive(true);
            else
                m.gameObject.SetActive(false);
        }
    }
    public void ShowGuns(int _sort)
    {
        for (int i = 0; i < gunTypes.Length; i++)
        {
            if (gunTypes[i].sort == _sort)
            {
                GameObject currentType = Instantiate(gunTypePrefab, gunTypesHolder);
                GunTypeHolder currentHolder = currentType.GetComponent<GunTypeHolder>();
                currentHolder.typeTitleText.text = gunTypes[i].typeName;
                foreach (GunScrObj gun in gunTypes[i].guns)
                {
                    GameObject currentGun = Instantiate(gunButtonPrefab, currentHolder.gunHolder);
                    GunButton currentGunButton = currentGun.GetComponent<GunButton>();
                    currentGunButton.SetUp(gun, gunTypes[i].sort);
                }
            }
        }
        gunPreviews.SetActive(true);
    }
    public void ShowGunStats(GunScrObj gun, bool active)
    {
        if (statsHolder == null)
            return;
        if (active)
        {
            statsGunNameText.text = gun.gunName;
            statslvlUnlocked.text = "Unlocked on lv: " + gun.levelUnlocked;
            statTabs[0].GetComponentInChildren<Slider>().value = gun.gunDamageCurve.Evaluate(0);
            statTabs[1].GetComponentInChildren<Slider>().value = gun.gunFireRate;
            statTabs[2].GetComponentInChildren<Slider>().value = gun.gunDamageCurve[1].time;
            float sumAccuracy = gun.gunAimingAccuracy + gun.gunNotAimingAccuracy;
            statTabs[3].GetComponentInChildren<Slider>().value = sumAccuracy / 2;
            statsHolder.SetActive(true);
        }
        else
            statsHolder.SetActive(false);
    }
    public void SelectGun(GunScrObj gun, int sort)
    {
        PlayerInfoManager.staticGunSlots[sort] = gun.gunPrefab;
        PlayerInfoManager.staticGunCamos[sort] = 0;
        camoBases[sort].sprite = noneSprite;
        camoHolders[sort].SetActive(false);
        SyncGun(gun, sort);
        gunPreviews.SetActive(false);
        for (int i = gunTypesHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(gunTypesHolder.GetChild(i).gameObject);
        }
    }
    void SyncGun(GunScrObj _gun, int sort)
    {
        gunSlotsShow[sort].GetComponent<GunSlot>().Set(_gun);
    }
    public void OpenCamoPanel(int slot)
    {
        string keyName = PlayerInfoManager.staticGunSlots[slot].GetComponent<Gun>().gun.gunName + "headshots";
        Camos[] camos = camoHolders[slot].GetComponent<CamosHolder>().camos;
        for (int i = 1; i < camos.Length; i++)
        {
            if (camos[i].specialUnlock)
            {
                if (camos[i].camoId == 7)
                {
                    if (PlayerPrefs.HasKey("patatje2"))
                    {
                        camos[i].gameObject.SetActive(true);
                    }
                }
                continue;
            }
            if (PlayerPrefs.GetInt(keyName) >= PlayerInfoManager.staticGunSlots[slot].GetComponent<Gun>().gun.camoUnlockPoints[i - 1])
            {
                camos[i].Locked(false);
            }
            else
                camos[i].Locked(true);
        }

        camoHolders[slot].SetActive(true);
    }
    public void SelectCamo(int slot, int id, Sprite camo)
    {
        PlayerInfoManager.staticGunCamos[slot] = id;
        camoBases[slot].sprite = camo;
        camoHolders[slot].SetActive(false);
        displayHeadshotsText[slot].SetActive(false);
    }
    public void HeadshotsShow(bool toggle, int slot, int id, bool specialUnlock)
    {
        if (!specialUnlock)
        {
            string keyName = PlayerInfoManager.staticGunSlots[slot].GetComponent<Gun>().gun.gunName + "headshots";
            displayHeadshotsText[slot].GetComponentInChildren<TMP_Text>().text = PlayerPrefs.GetInt(keyName) + "/" + PlayerInfoManager.staticGunSlots[slot].GetComponent<Gun>().gun.camoUnlockPoints[id - 1] + " " + PlayerInfoManager.staticGunSlots[slot].GetComponent<Gun>().gun.camoUnlockWay;
            displayHeadshotsText[slot].SetActive(toggle);
        }
    }
    #endregion
    #region Scoreboard
    void SetUpScoreboard()
    {
        gameName.text = GameManager.Instance.GetGameName();
        pv.RPC("RPC_SetUpPlayer", RpcTarget.All, PhotonManager.playerId, PhotonNetwork.NickName);
    }
    [PunRPC]
    void RPC_SetUpPlayer(byte playerId, string playerName)
    {
        PlayerInfo pi = Instantiate(playerInfoPrefab, playerHolder).GetComponent<PlayerInfo>();
        playerInfosList.Add(pi);
        pi.playerId = playerId;
        pi.points = 0;
        pi.playerName.text = playerName;
    }
    void UpdateScoreboard()
    {
        if (Input.GetButtonDown("Tab"))
        {
            scoreboardObj.SetActive(true);
        }
        else if (Input.GetButtonUp("Tab"))
        {
            scoreboardObj.SetActive(false);
        }
    }
    public void SyncScoreboardNumbers(PlayerManager pm, int addPoints, int addKills, int addDeaths, int addAssists)
    {
        pm.points += addPoints;
        pm.PointsThisLive(addPoints);
        pm.kills += addKills;
        pm.deaths += addDeaths;
        pm.assists += addAssists;
        pv.RPC("RPC_SyncPlayerInfo", RpcTarget.All, PhotonManager.playerId, pm.points, pm.kills, pm.deaths, pm.assists);
    }
    [PunRPC]
    void RPC_SyncPlayerInfo(byte playerId, int points, int kills, int deaths, int assists)
    {
        foreach(PlayerInfo info in playerInfosList)
        {
            if(info.playerId == playerId)
            {
                info.points = points;
                info.playerPoints.text = points.ToString();
                info.playerKills.text = kills.ToString();
                info.playerDeaths.text = deaths.ToString();
                info.playerAssists.text = assists.ToString();
            }
        }
    }
    #endregion
    #region Crosshair
    public void ShowHitMarker()
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = HitMarkerCoroutine();
        StartCoroutine(hitCoroutine);
    }
    IEnumerator HitMarkerCoroutine()
    {
        foreach (Image i in hitMarker)
        {
            i.color = marker1;
        }
        float timer = hitMarkerShowTime;
        while(timer > 0)
        {
            Color c = Color.Lerp(marker2, marker1, timer / hitMarkerShowTime);
            foreach (Image i in hitMarker)
            {
                i.color = c;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        foreach (Image i in hitMarker)
        {
            i.color = marker2;
        }
    }
    public void SetCrosshairDist(int set)
    {
        wantedDist = setDistances[set];
    }
    void CrossHair()
    {
        crosshairPieces[0].anchoredPosition = Vector2.Lerp(crosshairPieces[0].anchoredPosition, new Vector2(wantedDist, crosshairPieces[0].anchoredPosition.y), crosshairSpeed * Time.deltaTime);
        crosshairPieces[1].anchoredPosition = Vector2.Lerp(crosshairPieces[1].anchoredPosition, new Vector2(-wantedDist, crosshairPieces[1].anchoredPosition.y), crosshairSpeed * Time.deltaTime);
        crosshairPieces[2].anchoredPosition = Vector2.Lerp(crosshairPieces[2].anchoredPosition, new Vector2(crosshairPieces[2].anchoredPosition.x, wantedDist), crosshairSpeed * Time.deltaTime);
        crosshairPieces[3].anchoredPosition = Vector2.Lerp(crosshairPieces[3].anchoredPosition, new Vector2(crosshairPieces[3].anchoredPosition.x, -wantedDist), crosshairSpeed * Time.deltaTime);
    }
    #endregion
    #region Getting hit
    public void ShowRedScreen(float currentHealth, float maxHealth)
    {
        float f = (maxHealth - currentHealth) / maxHealth;
        redScreen.color = Color.Lerp(normal, max, f);
    }

    public void GetHit(Transform player, Vector3 hitPoint, int shotById, float damage)
    {
        RectTransform rect;
        if (damage > 100)
            damage = 100;
        
        float calcDmg = damage / 50;
        foreach (HitLocStorer storer in storers)
        {
            if (storer.playerId == shotById)
            {
                StopCoroutine(storer.coroutine);

                rect = storer.hitLoc.GetChild(0).GetComponent<RectTransform>();
                rect.localScale += new Vector3(calcDmg, 0, 0);

                storer.coroutine = GetHitCoroutine(player, hitPoint, storer.hitLoc);
                StartCoroutine(storer.coroutine);
                return;
            }
        }
        GameObject newHitLoc = Instantiate(getHitPrefab, getHitHolder);
        HitLocStorer newStorer = new HitLocStorer();
        newStorer.playerId = shotById;
        newStorer.hitLoc = newHitLoc.GetComponent<RectTransform>();
        rect = newStorer.hitLoc.GetChild(0).GetComponent<RectTransform>();
        rect.localScale += new Vector3(calcDmg, 0, 0);

        storers.Add(newStorer);
        newStorer.coroutine = GetHitCoroutine(player, hitPoint, newStorer.hitLoc);
        StartCoroutine(newStorer.coroutine);
    }
    IEnumerator GetHitCoroutine(Transform player, Vector3 hitPoint, RectTransform currentHit)
    {
        Image currentImage = currentHit.GetChild(0).GetComponent<Image>();
        currentImage.color = beginColor;
        float hitTimer = getHitShowTime;
        while (hitTimer > 0)
        {
            if (player != null)
            {
                Vector3 pos = new Vector3(hitPoint.x, player.position.y, hitPoint.z);

                Vector3 dir = pos - player.position;
                float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                float newAngle = -angle + player.eulerAngles.y;

                currentHit.rotation = Quaternion.Euler(currentHit.rotation.x, currentHit.rotation.y, newAngle);

            }
            hitTimer -= Time.deltaTime;
            yield return null;
        }
        float hitTimer2 = 0;
        while (hitTimer2 < fadeTime)
        {
            hitTimer2 += Time.deltaTime / fadeTime;
            currentImage.color = Color.Lerp(currentImage.color, invis, hitTimer2);
            yield return null;
        }
        foreach(HitLocStorer storer in storers)
        {
            if(storer.hitLoc == currentHit)
            {
                storers.Remove(storer);
                break;
            }
        }
        Destroy(currentHit.gameObject);
    }
    public void FlashBang()
    {
        if (flashBang != null)
            StopCoroutine(flashBang);
        flashBang = FlashRoutine();
        StartCoroutine(flashBang);
    }
    IEnumerator FlashRoutine()
    {
        flash.color = Color.white;
        float flashTimer = 1;
        yield return new WaitForSeconds(flashTime);
        while (flashTimer > 0)
        {
            flash.color = Color.Lerp(flashGone, Color.white, flashTimer);
            flashTimer -= flashFade * Time.deltaTime;
            yield return null;
        }
        flash.color = flashGone;
    }
    #endregion
    #region Gun info
    public void ShowGunInfo(Gun gunScript)
    {
        gunName.text = gunScript.gun.gunName;
        if (gunScript.gun.autoFire)
        {
            gunType.text = "Full-Auto";
        }
        else if (gunScript.gun.burstFire)
        {
            gunType.text = "Burst";
        }
        else
        {
            gunType.text = "Single-Shot";
        }
    }
    public void ShowAmmo(Gun gunScript)
    {
        gunAmmo.text = gunScript.currentMag + "/" + gunScript.currentAmmo;
    }
    public void ShowNades(ThrowableScrObj throwable, int _nadesLeft)
    {
        nadeImage.sprite = throwable.throwableSprite;
        nadesLeft.text = _nadesLeft.ToString();
    }
    public void HideNades()
    {
        ThrowableHolder.SetActive(false);
    }
    #endregion
    #region Points
    public void SetUpPoints()
    {
        teamImage.sprite = teamSprites[Random.Range(0, teamSprites.Length)];
        yourPoints.text = 0.ToString();
        otherPoints.text = 0.ToString();
    }
    public void ShowGamePoints(int[] pointsList)
    {
        int highestPoints = 0;
        for (int i = 0; i < pointsList.Length; i++)
        {
            if (i == PhotonManager.playerId)
            {
                yourPoints.text = pointsList[i].ToString();
                continue;
            }
            else
            {
                if(highestPoints == 0)
                {
                    highestPoints = pointsList[i];
                }
                else
                {
                    if(highestPoints < pointsList[i])
                    {
                        highestPoints = pointsList[i];
                    }
                }
            }
        }
        otherPoints.text = highestPoints.ToString();
    }
    public void ShowTeamPoints(int[] scores)
    {
        for (int i = 0; i < scores.Length; i++)
        {
            if(i == PhotonManager.team)
            {
                yourPoints.text = scores[i].ToString();
            }
            else
            {
                otherPoints.text = scores[i].ToString();
            }
        }
    }
    public void GetPoints(int addPoints)
    {
        currentGetPoints += addPoints;
        pointsGetObj.GetComponent<TMP_Text>().text = "+" + currentGetPoints.ToString();
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        showCoroutine = GetPointsCoroutine();
        StartCoroutine(showCoroutine);
    }
    IEnumerator GetPointsCoroutine()
    {
        pointsGetObj.SetActive(true);
        yield return new WaitForSeconds(showTime);
        pointsGetObj.SetActive(false);
        currentGetPoints = 0;
    }
    #endregion
    #region Buying
    public void ShowBuyText(string name, int cost)
    {
        buyText.text = "Press E for " + name + " [Cost: " + cost + "]";
        if (buyCoroutine != null)
            StopCoroutine(buyCoroutine);
        buyCoroutine = Buy();
        StartCoroutine(buyCoroutine);
    }
    IEnumerator Buy()
    {
        buyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.1f);
        buyText.gameObject.SetActive(false);
    }
    #endregion
    #region ScoreStreaks
    public void SelectScoreStreak(int id)
    {
        highlighterStreak.position = streaks[id].transform.position;
    }
    public void SyncScoreStreaks(int id, Sprite sprite)
    {
        streaks[id].sprite = sprite;
    }
    public void DisableScoreStreak(int id)
    {
        streaks[id].color = cantUse;
    }
    public void UnlockScoreStreak(int id)
    {
        streaks[id].color = canUse;
    }
    public void PointsToScoreStreak(int points, int nextToUnlock)
    {
        pointsBar.maxValue = nextToUnlock;
        pointsBar.value = points;
    }
    public void HideScoreStreaks()
    {
        scoreStreaksHolder.SetActive(false);
    }
    #endregion
    #region Capturing
    public void SyncCapturing(float value, float maxValue)
    {
        captureObj.SetActive(true);
        captureSlider.maxValue = maxValue;
        captureSlider.value = value;
    }
    public void StopCapturing()
    {
        captureObj.SetActive(false);
    }
    #endregion
    #region Messages
    public void SpawnMessage(string message)
    {
        Instantiate(messagePrefab, messageHolder).GetComponentInChildren<TMP_Text>().text = message;

        if (messageHolder.childCount > 4)
        {
            int lastChild = messageHolder.childCount - 1;
            Destroy(messageHolder.GetChild(lastChild).gameObject);
        }
    }
    #endregion
}
