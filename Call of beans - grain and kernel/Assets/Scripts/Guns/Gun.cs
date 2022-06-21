using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GunScrObj gun;
    public GameObject gunHolder;
    public Transform projectilePos;
    public int currentMag, currentAmmo;
    [HideInInspector] public Animator anim;
    [SerializeField] GameObject[] getCamo;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentMag = gun.magSize;
        currentAmmo = gun.maxAmmo;
    }
    public void SetCamo(Material camo)
    {
        foreach(GameObject g in getCamo)
        {
            g.GetComponent<MeshRenderer>().material = camo;
        }
    }
}
