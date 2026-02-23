using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spanwer : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject networkObjectPrefab;

    private PuzzleMissonListener puzzleMissonListener;

    private void Awake()
    {
        puzzleMissonListener = GetComponentInParent<PuzzleMissonListener>();
    }

    private void Start()
    {
        if(IsServer)
        {
            StartGame(MultiMissionType.DrawLine);
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
        DrawLineWithMesh drawLineWithMesh = net.GetComponent<DrawLineWithMesh>();
        CatmullRomPath[] catmullRomPaths = FindObjectsByType<CatmullRomPath>(FindObjectsSortMode.None);

        if (netId == NetworkPlayer.ClientPlayerId)
        {
            catmullRomPaths[0].Initialize(drawLineWithMesh);
        }
        else
        {
            catmullRomPaths[1].Initialize(drawLineWithMesh);
        }

        puzzleMissonListener.OnStartPuzzle += drawLineWithMesh.StartGame;
        puzzleMissonListener.OnEndPuzzle += drawLineWithMesh.EndGame;
    }

}
