using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Singleton;

    [SerializeField]
    private TMP_Text text;

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string str)
    {
        SetText_ClientRpc(str);
    }

    [ClientRpc]
    private void SetText_ClientRpc(string str)
    {
        text.text = str;
    }
}
