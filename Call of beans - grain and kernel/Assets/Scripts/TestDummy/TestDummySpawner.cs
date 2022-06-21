using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummySpawner : MonoBehaviour
{
    [SerializeField] private GameObject testDummyPrefab;
    [SerializeField] private GameObject currentDummy;

    private void Update()
    {
        if(currentDummy == null)
        {
            NewDummy();
        }
    }
    void NewDummy()
    {
        currentDummy = Instantiate(testDummyPrefab, transform);
    }
}
