using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDrawer : MonoBehaviour, IInteractable
{
    [SerializeField]
    private GameObject gridPrefab;

    [SerializeField]
    private int size = 10;

    [SerializeField]
    private float distance = 0.3f;

    private Vector3[,] gridPosition;

    private bool isInteracting = false;

    private LineRenderer lineRenderer;

    [SerializeField]
    private LineRenderer previewLineRenderer;

    [Header("Result"), Space(10f)]

    [SerializeField]
    private LineRenderer resultLineRenderer;

    private bool isGamePlaying = false;

    [SerializeField]
    private List<ConnectLinePuzzleData> resultLineData = new List<ConnectLinePuzzleData>();

    private int maxLineCount = 0;

    public void EndInteract()
    {
        if (!isGamePlaying)
            return;

        if (isInteracting)
        {
            Vector3 worldPosWithMouse = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
            worldPosWithMouse.z = 0;

            Vector2 nearest = GetNearestGridPos(worldPosWithMouse);

            // 처음 방문한 노드인지 검사
            if (lineRenderer.positionCount > 0 &&
               nearest.x == lineRenderer.GetPosition(0).x &&
               nearest.y == lineRenderer.GetPosition(0).y)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, nearest);

                // 정답 검사
                bool isClear = CheckLineToCorrect();
                isGamePlaying = false;
                GetComponentInParent<PuzzleMissonListener>().EndPuzzle(isClear);

            }
            else
            {
                if (CanConnect(nearest))
                {

                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, nearest);
                }
            }

            SetMaxLineCount(maxLineCount - 1);
            isInteracting = false;
            previewLineRenderer.positionCount = 0;
        }
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if (!isGamePlaying)
            return;

        if (!isInteracting)
        {
            if (lineRenderer.positionCount == 0)
            {
                Vector2 nearest = GetNearestGridPos(worldPosFromMousePosition);

                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, nearest);
            }

            isInteracting = true;
        }
        else
        {
            previewLineRenderer.positionCount = 2;
            previewLineRenderer.SetPosition(0, lineRenderer.GetPosition(lineRenderer.positionCount - 1));
            previewLineRenderer.SetPosition(1, worldPosFromMousePosition);
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        float half = (distance * size) / 2;
        Vector3 spawnPos = new Vector3(-half, -half);
        gridPosition = new Vector3[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                gridPosition[i, j] = spawnPos;
                spawnPos.x += distance;
            }

            spawnPos.x = -half;
            spawnPos.y += distance;
        }
    }

    private void OnEnable()
    {
        int dataIdx = UnityEngine.Random.Range(0, resultLineData.Count);
        DrawResultLine(dataIdx);
        isGamePlaying = true;
    }

    private void OnDisable()
    {
        GameUIManager.Singleton.SetMaxLineText(0);
        lineRenderer.positionCount = 0;
        previewLineRenderer.positionCount = 0;
    }

    private void DrawResultLine(int idx)
    {
        ConnectLinePuzzleData currentLineData = resultLineData[idx];

        resultLineRenderer.positionCount = currentLineData.correctPath.Count + 1;
        for (int i = 0; i < currentLineData.correctPath.Count; i++)
        {
            Vector2Int pathIdx = currentLineData.correctPath[i];

            resultLineRenderer.SetPosition(i, gridPosition[pathIdx.y, pathIdx.x]);
        }

        SetMaxLineCount(currentLineData.correctPath.Count);
        resultLineRenderer.SetPosition(resultLineRenderer.positionCount - 1, resultLineRenderer.GetPosition(0));
    }

    private Vector3 GetNearestGridPos(Vector2 position)
    {
        Vector2 nearstPos = gridPosition[0, 0];
        float minDist = Vector2.Distance(nearstPos, position);

        foreach (var currentGrid in gridPosition)
        {
            float dist = Vector2.Distance(currentGrid, position);

            if (dist < minDist)
            {
                minDist = dist;
                nearstPos = currentGrid;
            }
        }

        return nearstPos;
    }

    private bool CanConnect(Vector2 position)
    {
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector2 visitedPos = lineRenderer.GetPosition(i);

            if (Mathf.Approximately(position.x, visitedPos.x) &&
                Mathf.Approximately(position.y, visitedPos.y))
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckLineToCorrect()
    {
        int playerCount = lineRenderer.positionCount;
        int targetCount = resultLineRenderer.positionCount;

        if (playerCount != targetCount) return false;

        List<Vector3> playerPoints = new List<Vector3>();
        List<Vector3> targetPoints = new List<Vector3>();

        for (int i = 0; i < playerCount - 1; i++)
        {
            playerPoints.Add(lineRenderer.GetPosition(i));
            targetPoints.Add(resultLineRenderer.GetPosition(i));
        }

        int nodeCount = playerPoints.Count;

        for (int offset = 0; offset < nodeCount; offset++)
        {
            bool isForwardMatch = true;
            bool isReverseMatch = true;

            for (int i = 0; i < nodeCount; i++)
            {
                Vector3 playerPos = playerPoints[i];

                // 정방향으로 일치하는지 검사
                Vector3 targetForward = targetPoints[(i + offset) % nodeCount];
                if (Vector3.Distance(playerPos, targetForward) > 0.01f)
                {
                    isForwardMatch = false;
                }

                // 역방향으로 일치하는지 검사
                int reverseIndex = (offset - i + nodeCount) % nodeCount;
                Vector3 targetReverse = targetPoints[reverseIndex];
                if (Vector3.Distance(playerPos, targetReverse) > 0.01f)
                {
                    isReverseMatch = false;
                }
            }

            if (isForwardMatch || isReverseMatch)
            {
                return true;
            }
        }

        return false;
    }

    private void SetMaxLineCount(int value)
    {
        maxLineCount = value;

        GameUIManager.Singleton.SetMaxLineText(maxLineCount);
    }

    #region Create grid
    [ContextMenu("격자 생성하기 (Generate Grid)")]
    public void GenerateGrid()
    {
        ClearGrid(); // 중복 생성을 막기 위해 기존 격자 지우기

        float half = (distance * size) / 2;
        Vector3 spawnPos = transform.position + new Vector3(-half, -half, 0);
        gridPosition = new Vector3[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject grid = Instantiate(gridPrefab, transform);
                grid.transform.position = spawnPos;
                gridPosition[i, j] = spawnPos;
                spawnPos.x += distance;
            }

            spawnPos.x = transform.position.x - half;
            spawnPos.y += distance;
        }
    }

    [ContextMenu("격자 모두 지우기 (Clear Grid)")]
    public void ClearGrid()
    {
        // 에디터 상태에서는 Destroy 대신 DestroyImmediate를 사용해야 합니다.
        // 역순으로 지워야 자식 인덱스가 꼬이지 않습니다.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    #endregion
}
