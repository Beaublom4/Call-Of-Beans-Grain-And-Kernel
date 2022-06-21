using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScrObjs/Gun")]
public class GunScrObj : ScriptableObject
{
    public string gunName;
    public int gunID;
    public Sprite gunSprite;
    public GameObject gunPrefab;
    public int levelUnlocked;
    [Header("Shooting")]
    public AnimationCurve gunDamageCurve;
    public float gunRecoil;
    public float gunAimDivider = 1;
    public float gunRecoilTime;
    //public AnimationCurve damageDrop;
    [Header("Fireing")]
    public float gunFireRate;
    public bool autoFire;
    [Header("Burst")]
    public bool burstFire;
    public int burstRonds;
    public float burstSpeed;
    [Header("Shells")]
    public bool shootsShells;
    public int shellCount;
    [Header("Projectile")]
    public bool shootProjectile;
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public float explotionRange;
    [Header("Ammo")]
    public int magSize;
    public int maxAmmo;
    public float reloadTime;
    public bool reloadPerBullet;
    public float reloadBulletTime;
    [Header("Aiming")]
    public float scopeTime;
    public float gunAimingAccuracy;
    public float gunNotAimingAccuracy;
    public float scopeFOV, scopedSensMultiplier;
    [Header("Camos")]
    public string camoUnlockWay;
    public int[] camoUnlockPoints;
}
