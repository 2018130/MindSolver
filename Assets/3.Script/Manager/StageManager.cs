using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;

public class StageManager : NetworkBehaviour
{
    public static StageManager SingletonManager;

    [SerializeField]
    private List<GameObject> stages = new List<GameObject>();

    [SerializeField]
    private List<Pair<Transform>> redStagePoint;
    [SerializeField]
    private List<Pair<Transform>> blueStagePoint;

    [SerializeField]
    private string currentStage = "-";

    [SerializeField]
    private int canRemoveObstacleCount = 3;

    [SerializeField]
    private int remainRemoveObstacleCount = 0;

    private bool isOpened = false;

    [SerializeField]
    private List<PuzzleMissonListener> puzzleList = new List<PuzzleMissonListener>();

    [SerializeField]
    private bool isClientMoveEnded = false;

    [Header("Reference"), Space(10f)]

    [SerializeField]
    private PathFinding redPathFinding;
    [SerializeField]
    private PathFinding bluePathFinding;

    [SerializeField]
    private PlayerController redPlayer;
    [SerializeField]
    private PlayerController bluePlayer;
    
    private void Awake()
    {
        if(SingletonManager == null)
        {
            SingletonManager = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if(redPathFinding == null ||
            bluePathFinding == null)
        {
            PathFinding[] pathFindings = FindObjectsByType<PathFinding>(FindObjectsSortMode.None);
            foreach (var path in pathFindings)
            {
                if (path.gameObject.name.Contains("Red"))
                {
                    redPathFinding = path;
                }
                else if (path.gameObject.name.Contains("Blue"))
                {
                    bluePathFinding = path;
                }
            }
        }
    }

    private void Start()
    {
        ResetStage();
    }

    private void ResetStage()
    {
        remainRemoveObstacleCount = canRemoveObstacleCount;
        GameUIManager.Singleton.SetCanMoveText(remainRemoveObstacleCount);

        for(int i = 0; i < stages.Count; i++)
        {
            if(i == PersistentDataManager.Singleton.PlayerData.MaxClearStage)
            {
                stages[PersistentDataManager.Singleton.PlayerData.MaxClearStage].SetActive(true);
            }
            else
            {
                stages[i].SetActive(false);
            }
        }
    }

    public PuzzleMissonListener GetNextPuzzle()
    {
        if (puzzleList.Count == 0 || isOpened)
            return null;

        isOpened = true;
        PuzzleMissonListener puzzleMissonListener = puzzleList[0];
        puzzleList.RemoveAt(0);

        return puzzleMissonListener;
    }

    public void ClosePuzzleQueue()
    {
        isOpened = false;
    }

    public void InsertPuzzle(PuzzleMissonListener puzzleMissonListener)
    {
        puzzleList.Insert(0, puzzleMissonListener);
    }

    public void ReduceRemainRemoveObstacleCount()
    {
        remainRemoveObstacleCount--;
        GameUIManager.Singleton.SetCanMoveText(remainRemoveObstacleCount);

        if (!IsServer)
        {
            if (remainRemoveObstacleCount == 0)
            {
                EndMoveCount_ServerRpc();
            }
        }

        CheckEndOfPuzzle();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndMoveCount_ServerRpc()
    {
        isClientMoveEnded = true;
        CheckEndOfPuzzle();
    }

    private void CheckEndOfPuzzle()
    {
        Debug.Log((remainRemoveObstacleCount == 0) + " " + isClientMoveEnded);
        if(IsServer)
        {
            if (remainRemoveObstacleCount == 0 && isClientMoveEnded)
            {
                Debug.Log($"Start path finding");
                redPathFinding.StartPathFinding();
                bluePathFinding.StartPathFinding();
            }
        }
    }

    [ClientRpc]
    public void ClearStage_ClientRpc()
    {
        int level = PersistentDataManager.Singleton.PlayerData.MaxClearLevel;
        int stage = PersistentDataManager.Singleton.PlayerData.MaxClearStage;
        FallingTilemapEffect fallingTilemapEffect = stages[stage].GetComponent<FallingTilemapEffect>();

        stage++;
        if (stage > 3)
        {
            level++;
            stage = 0;
            PersistentDataManager.Singleton.PlayerData.MaxClearLevel = level;
            PersistentDataManager.Singleton.PlayerData.MaxClearStage = stage;

            Debug.Log($"스테이지 종료 레벨 상승!!");

            // TODO : change level;
        }
        else
        {
            PersistentDataManager.Singleton.PlayerData.MaxClearLevel = level;
            PersistentDataManager.Singleton.PlayerData.MaxClearStage = stage;

            fallingTilemapEffect.StartIndividualFall();
            Debug.Log(PersistentDataManager.Singleton.PlayerData.MaxClearStage);
            StartCoroutine(ChangeStage_co(fallingTilemapEffect, stage));
        }

    }

    private IEnumerator ChangeStage_co(FallingTilemapEffect fallingTilemapEffect, int nextStage)
    {
        fallingTilemapEffect.StartIndividualFall();

        yield return new WaitForSeconds(5f);

        redPathFinding.Origin = redStagePoint[nextStage].first;
        redPathFinding.Destination = redStagePoint[nextStage].second;
        redPlayer.transform.position = redPathFinding.Origin.position;

        bluePathFinding.Origin = blueStagePoint[nextStage].first;
        bluePathFinding.Destination = blueStagePoint[nextStage].second;
        bluePlayer.transform.position = bluePathFinding.Origin.position;

        ResetStage();
    }
}
