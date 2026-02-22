using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spanwer : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject networkObjectPrefab;

    private void Start()
    {
        if(IsServer)
        {
            GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle += StartGame;
        }
    }

    private void OnApplicationQuit()
    {
        if (IsServer)
        {
            GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle -= StartGame;
        }
    }

    public void StartGame(MultiMissionType multiMissionType)
    {
        if(multiMissionType == MultiMissionType.DrawLine)
        {
            SpawnObject(networkObjectPrefab, NetworkPlayer.ServerPlayerId);
            SpawnObject(networkObjectPrefab, NetworkPlayer.ClientPlayerId);
        }
    }


    public void SpawnObject(NetworkObject networkObject, ulong netId)
    {
        NetworkObject net = Instantiate(networkObject, transform.parent);
        net.SpawnWithOwnership(netId);
    }
}
