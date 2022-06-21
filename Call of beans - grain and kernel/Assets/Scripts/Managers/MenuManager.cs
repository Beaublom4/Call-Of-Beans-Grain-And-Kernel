using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    public MenuItem[] menuItems;

    private void Awake()
    {
        Instance = this;
    }
    public void SwitchMenu(string _name)
    {
        foreach(MenuItem mi in menuItems)
        {
            if (mi.menuName == _name)
                mi.gameObject.SetActive(true);
            else
                mi.gameObject.SetActive(false);
        }
    }
}
