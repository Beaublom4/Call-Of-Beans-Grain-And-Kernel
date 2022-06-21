using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScrObjs/Perk")]
public class PerkScrObj : ScriptableObject
{
    public string perkName;
    public int perkSlot;
    public int perkId;
    public Sprite perkIcon;
}
