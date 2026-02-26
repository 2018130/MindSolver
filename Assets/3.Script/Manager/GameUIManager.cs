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
    private TMP_Text clearText;

    [SerializeField]
    private TMP_Text canMoveText;

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

    public void SetCanMoveText(int moveCount)
    {
        Debug.Log("1111");
        canMoveText.text = "이동가능 횟수\n" + moveCount;
    }

    public void SetclearText(string str, bool isNetwork = true)
    {
        if(isNetwork)
        {
            SetclearText_ClientRpc(str);
        }
        else
        {
            clearText.text = str;
        }
    }

    [ClientRpc]
    private void SetclearText_ClientRpc(string str)
    {
        clearText.text = str;
    }
}
