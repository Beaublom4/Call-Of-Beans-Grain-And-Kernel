using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camos : MonoBehaviour
{
    public int camoId;
    [SerializeField] Sprite camo;
    [SerializeField] GameObject lockedObj;
    public bool specialUnlock;
    public void Locked(bool toggle)
    {
        lockedObj.SetActive(toggle);
    }
    public void SelectCamo(int slot)
    {
        if (!lockedObj.activeSelf)
        {
            if (FindObjectOfType<HudManager>())
            {
                HudManager.Instance.SelectCamo(slot, camoId, camo);
            }
            else
                PlayerInfoManager.Instance.SelectCamo(slot, camoId, camo);
        }
    }
    public void DisplayHeadshots(int slot)
    {
        if (FindObjectOfType<HudManager>())
        {
            HudManager.Instance.HeadshotsShow(true, slot, camoId, specialUnlock);
        }
        else
            PlayerInfoManager.Instance.HeadshotsShow(true, slot, camoId, specialUnlock);
    }
    public void StopDisplayHeadshots(int slot)
    {
        if (FindObjectOfType<HudManager>())
        {
            HudManager.Instance.HeadshotsShow(false, slot, camoId, specialUnlock);
        }
        else
            PlayerInfoManager.Instance.HeadshotsShow(false, slot, camoId, specialUnlock);
    }
}
