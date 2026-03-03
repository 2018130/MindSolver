using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PipeManager : NetworkBehaviour
{
    [SerializeField]
    private LayerMask pipeLayer;

    [SerializeField]
    private WaterSpawner waterSpawner_red;
    [SerializeField]
    private WaterSpawner waterSpawner_blue;
    [SerializeField]
    private Camera waterCamera;
    [SerializeField]
    private GameObject bg;

    Pipe[] pipes;

    private bool isPlayingGame = false;

    private void Start()
    {
        pipes = GetComponentsInChildren<Pipe>();
        InputManager.Singleton.OnClickedLeftBtn += RotateTargetPipe;

        GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle += StartGame;
        GetComponentInParent<PuzzleMissonListener>().OnEndPuzzle += EndGame;

        isPlayingGame = false;
        for (int i = 0; i < pipes.Length; i++)
        {
            pipes[i].gameObject.SetActive(false);
        }
        waterCamera.gameObject.SetActive(false);
    }

    private void StartGame(MultiMissionType multiMissionType)
    {
        if(multiMissionType == MultiMissionType.Pipe)
        {
            if (GetComponentInParent<PuzzleMissonListener>().MissionType ==MissionType.Multi)
            {
                StartGame_ClientRpc();
            }
            waterSpawner_red.OpenFauset_ServerRpc();
            waterSpawner_blue.OpenFauset_ServerRpc();
        }
    }
    private void EndGame(bool isClear, MultiMissionType multiMissionType)
    {
        if (multiMissionType == MultiMissionType.Pipe)
        {
            if (GetComponentInParent<PuzzleMissonListener>().MissionType == MissionType.Multi)
            {
                EndGame_ClientRpc();
            }
            waterSpawner_red.CloseFauset_ServerRpc();
            waterSpawner_blue.CloseFauset_ServerRpc();
        }
    }
    [ClientRpc]
    private void StartGame_ClientRpc()
    {
        isPlayingGame = true;
            for (int i = 0; i < pipes.Length; i++)
            {
                pipes[i].gameObject.SetActive(true);
        }
        bg.SetActive(true);
        waterCamera.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void EndGame_ClientRpc()
    {
        isPlayingGame = false;
            for (int i = 0; i < pipes.Length; i++)
            {
                pipes[i].gameObject.SetActive(false);
        }
        waterCamera.gameObject.SetActive(false);
        bg.SetActive(false);
    }

    public void RotateTargetPipe()
    {
        if (!isPlayingGame)
            return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);

        Collider2D target = Physics2D.OverlapCircle(worldPosition, 0.01f, pipeLayer);
        if(target != null)
        {
            target.GetComponent<Pipe>().RotateCW_ServerRpc();
        }
    }

}
