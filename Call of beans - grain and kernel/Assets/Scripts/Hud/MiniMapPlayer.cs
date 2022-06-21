using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class MiniMapPlayer : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] GameObject player;
    [SerializeField] GameObject teamPlayer;
    [SerializeField] Image enemy;

    [SerializeField] Color showColor, disabledColor;
    [SerializeField] float fadeSpeed;
    [SerializeField] float showLocTime;
    IEnumerator coroutine;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine)
            return;
        player.SetActive(true);
    }
    private void Start()
    {
        if (!pv.IsMine && GetComponentInParent<Health>().team == PhotonManager.team)
        {
            teamPlayer.SetActive(true);
        }
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;
        if(MultiplayerManager.staticUav)
            ShowLocOnMap();
    }
    public void ShowLocOnMap()
    {
        pv.RPC("ShowOnMap", RpcTarget.Others);
    }
    public void ShowMeYourLoc()
    {
        if (pv.IsMine)
            return;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = ShowCoroutine();
        StartCoroutine(coroutine);
    }
    [PunRPC]
    void ShowOnMap()
    {
        if (GetComponentInParent<Health>().team == PhotonManager.team)
            return;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = ShowCoroutine();
        StartCoroutine(coroutine);
    }
    IEnumerator ShowCoroutine()
    {
        enemy.color = showColor;
        yield return new WaitForSeconds(showLocTime);
        while(enemy.color != disabledColor)
        {
            enemy.color = Color.Lerp(enemy.color, disabledColor, fadeSpeed);
            yield return null;
        }
        enemy.color = disabledColor;
    }
}
