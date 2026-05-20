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

    Pipe_Single[] pipes;

    int bucketIdx = -1;

    private bool isPlayingGame = false;

    private void Awake()
    {
        pipes = GetComponentsInChildren<Pipe_Single>();
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
        //Destroy(pipes[bucketIdx].GetComponent<PipeWaterChecker>());
    }

    private void StartGame()
    {
        isPlayingGame = true;
        waterCamera.gameObject.SetActive(true);

        // 시작타일 지정
        int randValue = UnityEngine.Random.Range(0, 4);
        waterSpawner.transform.position = pipes[randValue].transform.position;
        pipes[randValue].IsStartTile = true;

        /*
        randValue = UnityEngine.Random.Range(0, 4);
        bucketIdx = pipes.Length - 1 - randValue;
        pipes[bucketIdx].gameObject.AddComponent<PipeWaterChecker>();
        for(int i = 0; i < pipes.Length; i++)
        {
            pipes[i].gameObject.SetActive(true);
        }*/
        SoundManager.Instance.PlaySFX("water");
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
