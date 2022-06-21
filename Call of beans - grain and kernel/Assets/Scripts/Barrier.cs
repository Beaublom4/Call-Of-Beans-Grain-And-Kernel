using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public GameObject[] planks;
    public int currentPlank;


    private void Start()
    {
        currentPlank = planks.Length - 1;
    }
    public void DestroyPlank(float waitTime)
    {
        StartCoroutine(DestoryPlank(currentPlank, waitTime));
        currentPlank--;
    }
    IEnumerator DestoryPlank(int plank, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        planks[plank].SetActive(false);
    }
    //public void DestroyPlank()
    //{
    //    if (planks[currentPlank] == null)
    //        return;
    //    planks[currentPlank].SetActive(false);
    //    currentPlank--;
    //}
}
