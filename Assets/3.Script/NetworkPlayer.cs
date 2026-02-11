using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    NetworkRelayManager networkRelayManager;

    private static ulong serverPlayerId = default;
    public static ulong ServerPlayerId => serverPlayerId;
    private static ulong clientPlayerId = default;
    public static ulong ClientPlayerId => clientPlayerId;


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

        if (IsServer)
        {
            if(IsOwner)
            {
                serverPlayerId = OwnerClientId;
                Debug.Log($"Set server player id to {serverPlayerId} in server");
            }
            else
            {
                clientPlayerId = OwnerClientId;
                Debug.Log($"Set clinet player id to {clientPlayerId} in server");
            }
        }
        else
        {
            if(IsOwner)
            {
                clientPlayerId = OwnerClientId;
                Debug.Log($"Set client player id to {clientPlayerId} in clinet");
            }
            else
            {
                serverPlayerId = OwnerClientId;
                Debug.Log($"Set server player id to {serverPlayerId} in clinet");
            }
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

            SceneChangeManager.Singleton.ChangeSceneByNetwork("SongJunYeop");
        }
    }
}
