using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    NetworkRelayManager networkRelayManager;

    private void Start()
    {
        networkRelayManager = FindAnyObjectByType<NetworkRelayManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
           NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    private void OnClientConnected(ulong clientId)
    {
        int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log($"Player Connected. Current Count: {connectedCount}");

        if (connectedCount == NetworkRelayManager.MaxConnections)
        {
            Debug.Log("Max players reached! Starting Game...");

            // 1. 로비 삭제/숨김 (더 이상 검색 안 되게)
            networkRelayManager.StartGameAndCloseLobby();

            // 2. 씬 전환 (Host가 실행하면 Client는 자동으로 따라옴)
            // 주의: NetworkManager 인스펙터에서 'Enable Scene Management'가 체크되어 있어야 함
            SceneChangeManager.Singleton.ChangeSceneByNetwork("GameScene");
        }
    }
}
