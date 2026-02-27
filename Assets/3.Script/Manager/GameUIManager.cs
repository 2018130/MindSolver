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
    private GameObject canMoveText;

    [Header("Puzzle")]
    [SerializeField]
    private GameObject maxDrawLineText;

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
        canMoveText.GetComponentInChildren<TMP_Text>().text = "ÀÌµ¿°¡´É È½¼ö\n" + moveCount;
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

    public void SetActiveMainUI(bool active)
    {
        canMoveText.gameObject.SetActive(active);
    }

    [ClientRpc]
    private void SetclearText_ClientRpc(string str)
    {
        clearText.text = str;
    }

    public void SetMaxLineText(int maxLineCount)
    {
        if(maxLineCount > 0)
        {
            maxDrawLineText.gameObject.SetActive(true);
        }
        else
        {
            maxDrawLineText.gameObject.SetActive(false);
        }

        maxDrawLineText.GetComponentInChildren<TMP_Text>().text = "³²Àº È¹¼ö : " + maxLineCount;
    }
}
