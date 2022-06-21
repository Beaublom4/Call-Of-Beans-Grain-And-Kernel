using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class CapturePoint : MonoBehaviour
{
    PhotonView pv;

    public int pointsId;
    [SerializeField] int flagId;

    public int currentTeam = -1;
    public bool inRange;
    [SerializeField] int playersInRange;

    [SerializeField] float currentCaptureTime = 0;
    [SerializeField] float capturedTime = 5;

    [SerializeField] MeshRenderer flagObj;
    [SerializeField] Material[] flagMats;
    [SerializeField] MeshRenderer shader;
    [SerializeField] Image mapIcon;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                if (currentTeam != PhotonManager.team)
                {
                    inRange = true;
                    pv.RPC("RPC_SetPlayerInRange", RpcTarget.All, 1);
                }
            }
        }
    }
    [PunRPC]
    void RPC_SetPlayerInRange(int add)
    {
        playersInRange += add;
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                if (currentTeam != PhotonManager.team)
                {
                    inRange = false;
                    HudManager.Instance.StopCapturing();
                    pv.RPC("RPC_SetPlayerInRange", RpcTarget.All, -1);
                }
            }
        }
    }
    private void Update()
    {
        if(playersInRange == 0)
        {
            currentCaptureTime = 0;
            return;
        }

        currentCaptureTime += playersInRange * Time.deltaTime;
        if (!inRange || currentTeam == PhotonManager.team)
            return;

        HudManager.Instance.SyncCapturing(currentCaptureTime, capturedTime);

        if(currentCaptureTime >= capturedTime)
        {
            currentCaptureTime = 0;
            currentTeam = PhotonManager.team;
            playersInRange = 0;

            GameManager.Instance.AddCapture(flagId);
            HudManager.Instance.StopCapturing();

            flagObj.material = flagMats[1];
            shader.material.SetColor("Color_e81837c39bbf405885d189da6e7fcc56", flagMats[1].color);
            mapIcon.color = flagMats[1].color;

            inRange = false;

            pv.RPC("RPC_SyncTeam", RpcTarget.Others, currentTeam);
        }
    }
    [PunRPC]
    void RPC_SyncTeam(int team)
    {
        if (!inRange)
        {
            currentTeam = team;
            playersInRange = 0;

            if(currentTeam == PhotonManager.team)
            {
                flagObj.material = flagMats[1];
                shader.material.SetColor("Color_e81837c39bbf405885d189da6e7fcc56", flagMats[1].color);
                mapIcon.color = flagMats[1].color;
            }
            else
            {
                flagObj.material = flagMats[2];
                shader.material.SetColor("Color_e81837c39bbf405885d189da6e7fcc56", flagMats[2].color);
                mapIcon.color = flagMats[2].color;
            }
        }

    }
}
