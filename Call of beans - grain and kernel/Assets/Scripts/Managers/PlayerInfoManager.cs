using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInfoManager : MonoBehaviour
{
    public static PlayerInfoManager Instance;

    [Header("Class")]
    [SerializeField] GunScrObj[] gunSlots;
    public static GameObject[] staticGunSlots;
    public static int[] staticGunCamos = new int[2];
    public static ThrowableScrObj staticThrowableSlot;
    [SerializeField] ThrowableScrObj standardThrowable;
    public static PerkScrObj perk1, perk2, perk3;

    [Header("Level")]
    public int level;
    [SerializeField] float currentXp;
    [SerializeField] float XpIncrease;
    [SerializeField] float xpToNextLevel;
    public static int xpLastGame;

    [Header("Last match xp")]
    public static int kills;
    public static int assists;
    public static int headshots;
    public static int destroys;
    public static int captures;
    public static int killStreak5s, killStreak10s, killStreak15s;

    [Header("Leveling settings")]
    [SerializeField] int settings;
    [SerializeField] float gettingXp;
    [SerializeField] float gettingXpSpeed;
    [Space]
    [SerializeField] int killXp;
    [SerializeField] Sprite killSprite;
    [SerializeField] int assistXp;
    [SerializeField] Sprite assistSprite;
    [SerializeField] int headshotKillXp;
    [SerializeField] Sprite headshotSprite;
    [SerializeField] int destroyXp;
    [SerializeField] Sprite destroySprite;
    [SerializeField] int captureXP;
    [SerializeField] Sprite captureSprite;
    [SerializeField] int killStreak5Xp;
    [SerializeField] Sprite killStreak5Sprite;
    [SerializeField] int killStreak10XP;
    [SerializeField] Sprite killStreak10Sprite;
    [SerializeField] int killStreak15XP;
    [SerializeField] Sprite killStreak15Sprite;

    [Header("Leveling setups")]
    [SerializeField] GameObject getLevelsPanel;
    [SerializeField] Transform showXpHolder;
    [SerializeField] GameObject showXpPrefab;
    [SerializeField] TMP_Text currentLevelText, nextLevelText;
    [SerializeField] TMP_Text currentXpText;
    [SerializeField] Slider xpSlider;
    [SerializeField] GameObject skipButton;

    [Header("Weapon list setups")]
    [SerializeField] GameObject[] gunSlotsShow;

    [SerializeField] GameObject gunTypePrefab;
    [SerializeField] GameObject gunButtonPrefab;

    [SerializeField] Transform gunTypesHolder;

    [SerializeField] GameObject gunPreviews;

    [System.Serializable]
    public class GunType
    {
        public string typeName;
        public GunScrObj[] guns;
        public int sort;
    }
    [SerializeField] GunType[] gunTypes;

    [Header("GunStats")]
    [SerializeField] TMP_Text statsGunNameText;
    [SerializeField] GameObject statsHolder;
    [SerializeField] GameObject[] statTabs;
    [SerializeField] TMP_Text statslvlUnlocked;

    [Header("Camos")]
    [SerializeField] GameObject[] camoHolders;
    [SerializeField] Image[] camoBases;
    [SerializeField] GameObject[] displayHeadshotsText;

    [Header("Throwables")]
    [SerializeField] TMP_Text throwableName;
    [SerializeField] Image throwableSprite;

    [Header("Perks")]
    [SerializeField] PerkSelect[] perkSelects;
    [SerializeField] GameObject[] perkDisplays;
    [SerializeField] Sprite noneSprite;

    [System.Serializable]
    public class Unlock
    {
        public string unlockName;
        public Sprite unlockSprite;
    }
    [Header("Unlocks")]
    [SerializeField] GameObject newUnlock;
    [SerializeField] Image newUnlockSprite;
    List<Unlock> unlockList = new List<Unlock>();
    IEnumerator unlockRoutine;

    [System.Serializable]
    public class Code
    {
        public string codeName;
        public Sprite codeSprite;
    }
    [Header("PromoCodes")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Code[] codes;


    private void Awake()
    {
        Instance = this;
        if(staticGunSlots == null)
            staticGunSlots = new GameObject[gunSlots.Length];
        else
        {
            for (int i = 0; i < staticGunSlots.Length; i++)
            {
                gunSlots[i] = staticGunSlots[i].GetComponent<Gun>().gun;
            }
        }

        if (staticThrowableSlot == null)
            staticThrowableSlot = standardThrowable;
        SyncThrowable();

        #region Perks set up
        if(perk1 == null)
            perkSelects[0].SetPerk("None", noneSprite);
        else
            perkSelects[0].SetPerk(perk1.perkName, perk1.perkIcon);
        if (perk2 == null)
            perkSelects[1].SetPerk("None", noneSprite);
        else
            perkSelects[1].SetPerk(perk2.perkName, perk2.perkIcon);
        if (perk3 == null)
            perkSelects[2].SetPerk("None", noneSprite);
        else
            perkSelects[2].SetPerk(perk3.perkName, perk3.perkIcon);
        #endregion

        if (PlayerPrefs.HasKey("Level"))
        {
            if (PlayerPrefs.GetInt("LevelSync") != 3)
            {
                PlayerPrefs.SetInt("LevelSync", 3);

                PlayerPrefs.SetInt("Level", 1);
                PlayerPrefs.SetInt("Xp", 0);
            }
            level = PlayerPrefs.GetInt("Level");
            currentXp = PlayerPrefs.GetInt("Xp");
        }
        PlayerPrefs.SetInt("LevelSync", 3);
        float newIncrease = XpIncrease * level;
        xpToNextLevel = xpToNextLevel + newIncrease;
    }
    private void Start()
    {
        if (staticGunSlots[0] == null)
        {
            for (int i = 0; i < gunSlots.Length; i++)
            {
                staticGunSlots[i] = gunSlots[i].gunPrefab;
            }
        }
        for (int i = 0; i < gunSlotsShow.Length; i++)
        {
            SyncGun(gunSlots[i], i);
        }

        if (PhotonNetwork.InRoom)
        {
            float newIncrease = XpIncrease * level;
            xpToNextLevel = xpToNextLevel + newIncrease; ;
            GetPointsForLevel();
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
        gunSlots[sort] = gun;
        staticGunSlots[sort] = gun.gunPrefab;
        staticGunCamos[sort] = 0;
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
    public void SelectPrimaryCamo(int id)
    {
        staticGunCamos[0] = id;
    }
    public void SelectSecondaryCamo(int id)
    {
        staticGunCamos[1] = id;
    }
    public void SelectThrowable(ThrowableScrObj throwable)
    {
        staticThrowableSlot = throwable;
        SyncThrowable();
    }
    void SyncThrowable()
    {
        throwableName.text = staticThrowableSlot.throwableName;
        throwableSprite.sprite = staticThrowableSlot.throwableSprite;
    }
    public void GetPointsForLevel()
    {
        SyncCurrentLevel();
        SyncCurrentXP();

        getLevelsPanel.SetActive(true);

        StartCoroutine(GetXp());
    }
    IEnumerator GetXp()
    {
        int currentSetting = 0;
        for (int i = 0; i < settings; i++)
        {
            gettingXp = 0;
            GameObject currentInstant = Instantiate(showXpPrefab, showXpHolder);
            bool zero = false;
            switch (currentSetting) 
            {
                case 0:
                    if (kills <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = killXp * kills;
                    currentInstant.GetComponent<Image>().sprite = killSprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = kills.ToString();
                    break;
                case 1:
                    if (assists <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = assistXp * assists;
                    currentInstant.GetComponent<Image>().sprite = assistSprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = assists.ToString();
                    break;
                case 2:
                    if (headshots <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = headshotKillXp * headshots;
                    currentInstant.GetComponent<Image>().sprite = headshotSprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = headshots.ToString();
                    break;
                case 3:
                    if (destroys <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = destroyXp * destroys;
                    currentInstant.GetComponent<Image>().sprite = destroySprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = destroys.ToString();
                    break;
                case 4:
                    if (captures <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = captureXP * captures;
                    currentInstant.GetComponent<Image>().sprite = captureSprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = captures.ToString();
                    break;
                case 5:
                    if (killStreak5s <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = killStreak5Xp * killStreak5s;
                    currentInstant.GetComponent<Image>().sprite = killStreak5Sprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = killStreak5s.ToString();
                    break;
                case 6:
                    if(killStreak10s <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = killStreak10XP * killStreak10s;
                    currentInstant.GetComponent<Image>().sprite = killStreak10Sprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = killStreak10s.ToString();
                    break;
                case 7:
                    Debug.Log(killStreak15s);
                    if(killStreak15s <= 0)
                    {
                        Destroy(currentInstant);
                        zero = true;
                        break;
                    }
                    gettingXp = killStreak15XP * killStreak15s;
                    currentInstant.GetComponent<Image>().sprite = killStreak15Sprite;
                    currentInstant.GetComponentInChildren<TMP_Text>().text = killStreak15s.ToString();
                    break;
            }
            while (gettingXp > 0)
            {
                currentXp += gettingXpSpeed * Time.deltaTime;
                gettingXp -= gettingXpSpeed * Time.deltaTime;
                SyncCurrentXP();
                CheckLevelUp();
                yield return null;
            }
            currentXp = Mathf.Floor(currentXp);
            currentSetting++;
            if (!zero)
            {
                Debug.Log("test");
                yield return new WaitForSeconds(.5f);
            }
        }

        kills = 0;
        assists = 0;
        headshots = 0;
        destroys = 0;
        captures = 0;
        killStreak5s = 0;
        killStreak10s = 0;
        killStreak15s = 0;

        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("Xp", (int)currentXp);

        PhotonManager.Instance.ChangeLevel();

        skipButton.SetActive(true);
    }
    public void Skip()
    {
        getLevelsPanel.SetActive(false);
        skipButton.SetActive(false);
    }
    void SyncCurrentXP()
    {
        currentXpText.text = currentXp.ToString("F0") + "/" + xpToNextLevel;
        xpSlider.maxValue = xpToNextLevel;
        xpSlider.value = currentXp;
    }
    void SyncCurrentLevel()
    {
        currentLevelText.text = level.ToString();
        nextLevelText.text = (level + 1).ToString();
    }
    void CheckLevelUp()
    {
        if(currentXp >= xpToNextLevel)
        {
            level++;
            currentXp = 0;
            xpToNextLevel += XpIncrease;
            SyncCurrentLevel();
        }
    }
    public void SelectPerk(PerkScrObj perk)
    {
        switch (perk.perkSlot)
        {
            case 0:
                if (perk1 != perk)
                {
                    perk1 = perk;
                    perkSelects[0].SetPerk(perk.perkName, perk.perkIcon);
                }
                else
                {
                    perk1 = null;
                    perkSelects[0].SetPerk("None", noneSprite);
                }
                break;
            case 1:
                if (perk2 != perk)
                {
                    perk2 = perk;
                    perkSelects[1].SetPerk(perk.perkName, perk.perkIcon);
                }
                else
                {
                    perk2 = null;
                    perkSelects[1].SetPerk("None", noneSprite);
                }
                break;
            case 2:
                if (perk3 != perk)
                {
                    perk3 = perk;
                    perkSelects[2].SetPerk(perk.perkName, perk.perkIcon);
                }
                else
                {
                    perk3 = null;
                    perkSelects[2].SetPerk("None", noneSprite);
                }
                break;
        }
        foreach(GameObject g in perkDisplays)
        {
            g.SetActive(false);
        }
    }
    public void OpenCamoPanel(int slot)
    {
        string keyName = gunSlots[slot].gunName + "headshots";
        Camos[] camos = camoHolders[slot].GetComponent<CamosHolder>().camos;
        for (int i = 1; i < camos.Length; i++)
        {
            if (camos[i].specialUnlock)
            {
                if(camos[i].camoId == 7)
                {
                    if (PlayerPrefs.HasKey("patatje"))
                    {
                        camos[i].gameObject.SetActive(true);
                    }
                }
                continue;
            }
            if (PlayerPrefs.GetInt(keyName) >= gunSlots[slot].camoUnlockPoints[i - 1])
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
        staticGunCamos[slot] = id;
        camoBases[slot].sprite = camo;
        camoHolders[slot].SetActive(false);
        displayHeadshotsText[slot].SetActive(false);
    }
    public void HeadshotsShow(bool toggle, int slot, int id, bool specialUnlock)
    {
        if (!specialUnlock)
        {
            string keyName = gunSlots[slot].gunName + "headshots";
            displayHeadshotsText[slot].GetComponentInChildren<TMP_Text>().text = PlayerPrefs.GetInt(keyName) + "/" + gunSlots[slot].camoUnlockPoints[id - 1] + " " + gunSlots[slot].camoUnlockWay;
            displayHeadshotsText[slot].SetActive(toggle);
        }
    }
    public void UnlockItem(string unlockName, Sprite icon)
    {
        Unlock unlock = new Unlock();
        unlock.unlockName = unlockName;
        unlock.unlockSprite = icon;
        unlockList.Add(unlock);
        if(unlockRoutine == null)
        {
            StartCoroutine(UnlockItemRoutine());
        }
    }
    IEnumerator UnlockItemRoutine()
    {
        yield return new WaitForEndOfFrame();
        while (unlockList.Count > 0)
        {
            newUnlock.GetComponentInChildren<TMP_Text>().text = unlockList[0].unlockName;
            newUnlockSprite.sprite = unlockList[0].unlockSprite;
            newUnlock.SetActive(true);
            yield return new WaitForSeconds(2);
            unlockList.RemoveAt(0);
            yield return new WaitForSeconds(.1f);
        }
        newUnlock.SetActive(false);
    } 
    public void PromoCode(string code)
    {
        inputField.text = "";
        if (code == "DN76ZEWW")
        {
            PlayerPrefs.SetString("patatje", "patatje");
            UnlockItem(codes[0].codeName, codes[0].codeSprite);
        }
        else if(code == "3RN74M8X")
        {
            PlayerPrefs.SetString("giveMeXp", "giveMeXp");
            UnlockItem(codes[1].codeName, codes[1].codeSprite);
        }

    }
}
