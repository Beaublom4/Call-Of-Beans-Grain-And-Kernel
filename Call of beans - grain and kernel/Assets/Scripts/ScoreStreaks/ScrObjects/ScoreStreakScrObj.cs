using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScrObjs/ScoreStreak")]
public class ScoreStreakScrObj : ScriptableObject
{
    public int streakId;
    public Sprite streakSprite;
    public int streakWantedPoints;
    public int destroyPoints;
}
