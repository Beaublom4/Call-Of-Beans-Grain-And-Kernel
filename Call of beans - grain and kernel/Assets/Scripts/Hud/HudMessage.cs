using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudMessage : MonoBehaviour
{
    [SerializeField] float destroyTime;
    private void Start()
    {
        Invoke("DestroyObj", destroyTime);
    }
    void DestroyObj()
    {
        Destroy(gameObject);
    }
}
