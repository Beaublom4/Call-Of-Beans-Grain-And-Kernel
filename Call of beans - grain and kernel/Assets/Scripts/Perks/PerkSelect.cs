using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PerkSelect : MonoBehaviour
{
    [SerializeField] TMP_Text perkNameText;
    [SerializeField] Image perkImage;

    public void SetPerk(string _perkName, Sprite _perkIcon)
    {
        perkNameText.text = _perkName;
        perkImage.sprite = _perkIcon;
    }
}
