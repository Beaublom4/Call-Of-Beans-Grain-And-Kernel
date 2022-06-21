using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterUAVWiggl : MonoBehaviour
{
    [SerializeField] float wiggleSpeed;
    [SerializeField] float wiggleSize;

    Vector2 startPos, randomPos;

    private void Start()
    {
        startPos = transform.position;
    }
    void Update()
    {
        Vector2 loc = startPos + new Vector2(Random.Range(-wiggleSize, wiggleSize), Random.Range(-wiggleSize, wiggleSize));
        transform.position = Vector2.MoveTowards(transform.position, loc, wiggleSpeed * Time.deltaTime);
    }
}
