using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCam : MonoBehaviour
{
    public float moveSpeed;
    public Vector3 newLoc, deathLoc;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, newLoc, moveSpeed * Time.deltaTime);

        Vector3 targetDirection = deathLoc - transform.position;
        float singleStep = moveSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
