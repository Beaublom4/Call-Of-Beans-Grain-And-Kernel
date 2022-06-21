using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScrObjs/Gamemode")]
public class GamemodeScrObj : ScriptableObject
{
    public string gamemodeName;
    public bool hasTeams;
    public int gamePointsToWin;

    public int pointsPerKill;
    public int pointsPerAssist;

    public bool hasKillStreaks = true;
    public bool hasPerks = true;
}
