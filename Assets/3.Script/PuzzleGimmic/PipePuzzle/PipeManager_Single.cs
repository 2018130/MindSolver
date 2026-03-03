using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager_Single : MonoBehaviour
{
    [SerializeField]
    private LayerMask pipeLayer;

    [SerializeField]
    private WaterSpawner_Single waterSpawner;
    [SerializeField]
    private Camera waterCamera;

    Pipe[] pipes;

    private bool isPlayingGame = false;

    private void Awake()
    {
        pipes = GetComponentsInChildren<Pipe>();
        InputManager.Singleton.OnClickedLeftBtn += RotateTargetPipe;
    }

    private void OnEnable()
    {
        StartGame();
    }

    private void OnDisable()
    {
        isPlayingGame = false;
        waterCamera.gameObject.SetActive(false);
        waterSpawner.CloseFaucet();
    }

    private void StartGame()
    {
        isPlayingGame = true;
        waterCamera.gameObject.SetActive(true);

        waterSpawner.OpenFaucet();
    }


    public void RotateTargetPipe()
    {
        if (!isPlayingGame)
            return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);

        Collider2D target = Physics2D.OverlapCircle(worldPosition, 0.01f, pipeLayer);
        if (target != null)
        {
            // 이전에 수정한 Pipe의 싱글용 회전 메서드 호출 (_ServerRpc 제거됨)
            target.GetComponent<Pipe_Single>().RotateCW();
        }
    }
}
