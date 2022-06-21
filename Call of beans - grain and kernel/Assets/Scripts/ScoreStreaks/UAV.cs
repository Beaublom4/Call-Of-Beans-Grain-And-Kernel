using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class UAV : MonoBehaviour
{
    PhotonView pv;

    public float aliveTime;

    bool show;
    Transform uavChecker;
    Transform uavStartCheck, uavEndCheck;

    [SerializeField] float moveSpeed;
    [SerializeField] float waitBetweenTime;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        uavChecker = GameManager.Instance.uavCol;
        uavStartCheck = GameManager.Instance.uavCheckMoveSpots[0];
        uavEndCheck = GameManager.Instance.uavCheckMoveSpots[1];
    }
    private void Start()
    {
        if (GetComponent<ScorestreakHealth>().team == PhotonManager.team)
        {
            DoStart();
        }
    }
    private void Update()
    {
        if(show && uavChecker.position != uavEndCheck.position)
        {
            uavChecker.position = Vector3.MoveTowards(uavChecker.position, uavEndCheck.position, moveSpeed * Time.deltaTime);
            if (uavChecker.position == uavEndCheck.position)
            {
                show = false;
                uavChecker.gameObject.SetActive(false);
                StartCoroutine(WaitBetween());
            }
        }
    }
    private void OnDestroy()
    {
        UAV[] uavs = FindObjectsOfType<UAV>();
        int yourUAVS = 0;
        foreach(UAV uav in uavs)
        {
            if(uav.GetComponent<ScorestreakHealth>().team == PhotonManager.team)
            {
                yourUAVS++;
            }
        }
        if(yourUAVS <= 0)
        {
            uavChecker.gameObject.SetActive(false);
        }
    }
    public void DoStart()
    {
        StartShow();
    }
    void StartShow()
    {
        uavChecker.position = uavStartCheck.position;
        uavChecker.gameObject.SetActive(true);
        show = true;
    }
    IEnumerator WaitBetween()
    {
        yield return new WaitForSeconds(waitBetweenTime);
        StartShow();
    }
}
