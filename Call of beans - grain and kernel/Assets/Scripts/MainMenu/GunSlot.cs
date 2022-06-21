using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GunSlot : MonoBehaviour
{
    [SerializeField] TMP_Text gunName;
    [SerializeField] Image gunSprite;

    public void Set(GunScrObj gun)
    {
        gunName.text = gun.gunName;
        gunSprite.sprite = gun.gunSprite;
    }
}
