using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInteract : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] private float interactRadius;
    [SerializeField] private LayerMask interactLayer;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;
        CheckInteract();
    }
    void CheckInteract()
    {
        Collider[] interacts = Physics.OverlapSphere(transform.position, interactRadius, interactLayer);
        if (interacts.Length == 0)
            return;
        Collider currentClosest = null;
        float currentClosestDistance = 0;
        foreach (Collider interact in interacts)
        {
            if (currentClosest == null)
            {
                currentClosest = interact;
                currentClosestDistance = Vector3.Distance(transform.position, currentClosest.transform.position);
            }
            else
            {
                float newDistance = Vector3.Distance(transform.position, currentClosest.transform.position);
                if (newDistance < currentClosestDistance)
                {
                    currentClosest = interact;
                    currentClosestDistance = newDistance;
                }
            }
        }
        CheckInteracts(currentClosest);
    }
    void CheckInteracts(Collider obj)
    {
        InteractBuy interact = obj.GetComponent<InteractBuy>();
        HudManager.Instance.ShowBuyText(interact.buyName, interact.buyCost);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
