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

    private void Start()
    {
        NetworkRelayManager = FindAnyObjectByType<NetworkRelayManager>();
    }
}
