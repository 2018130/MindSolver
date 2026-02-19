using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spanwer : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject networkObjectPrefab;
    [SerializeField]
    private bool spawnServerOnStart = true;
    [SerializeField]
    private bool spawnClientOnStart = true;

    private void Start()
    {
        if(IsServer)
        {
            if (spawnServerOnStart)
            {
                SpawnObject(networkObjectPrefab, NetworkPlayer.ServerPlayerId);
            }

            if (spawnClientOnStart)
            {
                SpawnObject(networkObjectPrefab, NetworkPlayer.ClientPlayerId);
            }
        }
    }
    public void SpawnObject(NetworkObject networkObject, ulong netId)
    {
        NetworkObject net = Instantiate(networkObject);
        net.SpawnWithOwnership(netId);
    }
}
