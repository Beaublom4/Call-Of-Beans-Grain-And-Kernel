using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GunButton : MonoBehaviour
{
    [SerializeField] TMP_Text gunNameText;
    [SerializeField] Image gunImage;
    [SerializeField] GameObject lockedImage;
    int sort;
    GunScrObj gun;

    public void SetUp(GunScrObj _gun,  int _sort)
    {
        gun = _gun;
        gunNameText.text = gun.gunName;
        gunImage.sprite = gun.gunSprite;
        sort = _sort;

        if (PlayerInfoManager.Instance.level < gun.levelUnlocked)
            lockedImage.SetActive(true);
    }
    public void SelectGun()
    {
        if (PlayerInfoManager.Instance.level < gun.levelUnlocked)
            return;

        if (SceneManager.GetActiveScene().buildIndex == 0)
            PlayerInfoManager.Instance.SelectGun(gun, sort);
        else
            HudManager.Instance.SelectGun(gun, sort);
    }
    public void DisplayGun(bool display)
    {
        if (FindObjectOfType<HudManager>())
        {
            HudManager.Instance.ShowGunStats(gun, display);
        }
        else
            PlayerInfoManager.Instance.ShowGunStats(gun, display);
    }
}
