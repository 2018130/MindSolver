using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager SingletonManager;

    [SerializeField]
    private string currentStage = "-";

    [SerializeField]
    private int canRemoveObstacleCount = 3;

    [SerializeField]
    private int remainRemoveObstacleCount = 0;

    private bool isOpened = false;

    [SerializeField]
    private List<PuzzleMissonListener> puzzleList = new List<PuzzleMissonListener>();

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
    }
    private void Start()
    {
        ResetStage();
    }

    private void ResetStage()
    {
        remainRemoveObstacleCount = canRemoveObstacleCount;
        GameUIManager.Singleton.SetCanMoveText(remainRemoveObstacleCount);
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
    }
}
