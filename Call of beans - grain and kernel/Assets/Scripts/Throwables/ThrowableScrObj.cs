using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScrObjs/Throwable")]
public class ThrowableScrObj : ScriptableObject
{
    public string throwableName;
    public Sprite throwableSprite;
    public float explodeDamage;
    public GameObject throwablePrefab;
    public int nadeCount;
    public Vector3 throwInput;
    public float explodeRange;
    public float explodeDelay;
    public bool canCook;
    public bool explodeOnImpact;
    public int nadeId;
}
