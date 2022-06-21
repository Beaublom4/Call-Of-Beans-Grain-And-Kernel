using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource source;

    [SerializeField] AudioClip hitMarker;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    public void HitMarkerSound()
    {
        source.PlayOneShot(hitMarker, 1);
    }
}
