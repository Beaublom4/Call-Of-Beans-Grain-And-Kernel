using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class PlayerHolder : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] TMP_Text playerLevel;
    public Player player;

    public void SetUp(string name, int level, Player p)
    {
        playerName.text = name;
        playerLevel.text = level.ToString();
        player = p;
    }
}
