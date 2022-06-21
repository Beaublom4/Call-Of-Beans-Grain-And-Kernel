using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    InteractBuy interact;
    private void Awake()
    {
        interact = GetComponent<InteractBuy>();
    }
    public void BuyDoor(PlayerInteract player)
    {

    }
}
