using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRelayUIManager : MonoBehaviour
{
    [SerializeField]
    private Button quickJoin_btn;

    NetworkRelayManager NetworkRelayManager;

    [SerializeField]
    private int maxStage = 1;

    [Header("tooltip")]
    [SerializeField]
    private TMP_Text tooltip_text;
    [SerializeField]
    private List<string> tooltipList = new List<string>();

    private void Start()
    {
        NetworkRelayManager = FindAnyObjectByType<NetworkRelayManager>();
        SetRandomTooltip();
    }

    private void SetRandomTooltip()
    {
        tooltip_text.text = tooltipList[UnityEngine.Random.Range(0, tooltipList.Count)];
    }
}
