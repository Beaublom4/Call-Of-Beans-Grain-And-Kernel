using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicZombie : ZombieBasics
{
    bool smashing;

    public GameObject smashHitbox;
    //private void Update()
    //{
        //if (plankDetected && !smashing)
        //{
        //    StartCoroutine(HitPlank());
        //}
        //if (playerDetected && !smashing)
        //{
        //    StartCoroutine(SmashPlayer());
        //}
    //}
    //public IEnumerator HitPlank()
    //{
    //    smashing = true;
    //    anim.SetLayerWeight(anim.GetLayerIndex("BasicSmash"), 1);
    //    anim.SetTrigger("BasicSmash");
    //    yield return new WaitForSecondsRealtime(1);
    //    DestroyPlank();
    //    anim.SetLayerWeight(anim.GetLayerIndex("BasicSmash"), 0);
    //    yield return new WaitForSecondsRealtime(0.2f);
    //    smashing = false;
    //}
    public IEnumerator SmashPlayer()
    {
        smashing = true;
        smashHitbox.SetActive(true);
        anim.SetLayerWeight(anim.GetLayerIndex("BasicSmash"), 1);
        anim.SetTrigger("BasicSmash");
        yield return new WaitForSecondsRealtime(1);
        smashHitbox.SetActive(false);
        anim.SetLayerWeight(anim.GetLayerIndex("BasicSmash"), 0);
        playerDetected = false;
        smashing = false;
    }
}
