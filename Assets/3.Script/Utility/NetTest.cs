using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetTest : MonoBehaviour
{
    [SerializeField]
    private Button server;
    [SerializeField]
    private Button clinet;

    private void Start()
    {
        server.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        clinet.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }
}
