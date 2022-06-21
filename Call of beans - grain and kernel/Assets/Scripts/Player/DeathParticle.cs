using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathParticle : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 1);
    }
}
