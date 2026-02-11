using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathLengthTester : MonoBehaviour
{
    [Header("공통 환경 설정")]
    [Tooltip("좌표 계산의 기준이 되는 바닥 타일맵입니다.")]
    public Tilemap groundTilemap;

    [Header("장애물 설정")]
    [Tooltip("여기에 'Obstacle' 타일맵을 넣어주세요!")]
    public Tilemap obstacleTilemap;

    [Tooltip("체크(V)하면 장애물을 벽으로 인식해서 못 지나가고,\n해제하면 장애물을 무시하고 지나갑니다.")]
    public bool treatObstacleAsWall = true; 

    [Tooltip("이동 가능한 타일만 감지하기 위한 레이어 설정입니다. (Ground용)")]
    public LayerMask moveLayer;
    [Tooltip("체크 시 씬 뷰(Scene View)에 탐색된 경로를 선으로 그려줍니다.")]
    public bool showDebugLine = true;

    [Header("--- 루트 1 설정 (Cyan 색상) ---")]
    public Transform startPoint1;
    public Transform endPoint1;
    [Tooltip("루트 1의 계산된 최단 경로 타일 개수입니다.")]
    public int pathCount1 = 0;

    [Header("--- 루트 2 설정 (Magenta 색상) ---")]
    public Transform startPoint2;
    public Transform endPoint2;
    [Tooltip("루트 2의 계산된 최단 경로 타일 개수입니다.")]
    public int pathCount2 = 0;

    // A* 알고리즘을 위한 노드 클래스
    private class Node
    {
        public Vector3Int gridPos;
        public Node parent;
        public int g;
        public int h;
        public int f => g + h;

        public Node(Vector3Int pos) { gridPos = pos; }
    }

    [ContextMenu("두 경로 동시에 계산하기")]
    public void CalculatePath()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("오류: 타일맵(Ground Tilemap)이 연결되지 않았습니다.");
            return;
        }

        // --- 루트 1 계산 ---
        if (startPoint1 != null && endPoint1 != null)
        {
            pathCount1 = ProcessPath(startPoint1, endPoint1, Color.cyan, "루트 1");
        }
        else
        {
            pathCount1 = 0;
        }

        // --- 루트 2 계산 ---
        if (startPoint2 != null && endPoint2 != null)
        {
            pathCount2 = ProcessPath(startPoint2, endPoint2, Color.magenta, "루트 2");
        }
        else
        {
            pathCount2 = 0;
        }

        Debug.Log("---------- 계산 종료 ----------");
    }

    private int ProcessPath(Transform start, Transform end, Color debugColor, string routeName)
    {
        Vector3Int startCell = groundTilemap.WorldToCell(start.position);
        Vector3Int endCell = groundTilemap.WorldToCell(end.position);

        if (!IsWalkable(startCell))
        {
            Debug.LogError($"오류 [{routeName}]: 시작 지점({startCell})이 이동 불가능한 곳입니다.");
            return 0;
        }

        int count = FindPath(startCell, endCell, debugColor);

        if (count > 0)
            Debug.Log($"[{routeName}] 탐색 성공: 총 {count}칸");
        else
            Debug.LogError($"[{routeName}] 탐색 실패: 경로를 찾을 수 없습니다.");

        return count;
    }

    private int FindPath(Vector3Int startPos, Vector3Int targetPos, Color debugColor)
    {
        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        List<Node> openList = new List<Node>();
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        openList.Add(startNode);

        Vector3Int[] directions = {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0)
        };

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f < currentNode.f || (openList[i].f == currentNode.f && openList[i].h < currentNode.h))
                    currentNode = openList[i];
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.gridPos);

            if (currentNode.gridPos == targetNode.gridPos)
            {
                return CalculateStepCount(currentNode, debugColor);
            }

            foreach (var dir in directions)
            {
                Vector3Int neighborPos = currentNode.gridPos + dir;

                if (closedList.Contains(neighborPos) || !IsWalkable(neighborPos))
                    continue;

                int newMovementCost = currentNode.g + 10;
                Node neighbor = openList.Find(n => n.gridPos == neighborPos);

                if (neighbor == null || newMovementCost < neighbor.g)
                {
                    if (neighbor == null)
                    {
                        neighbor = new Node(neighborPos);
                        openList.Add(neighbor);
                    }

                    neighbor.g = newMovementCost;
                    neighbor.h = GetManhattanDistance(neighborPos, targetPos);
                    neighbor.parent = currentNode;
                }
            }
        }

        return 0;
    }

    private bool IsWalkable(Vector3Int cellPos)
    {
        // 장애물을 벽으로 취급 (True)
        if (treatObstacleAsWall)
        {
            // 장애물 타일맵에 타일이 있으면 못 감(False)
            if (obstacleTilemap != null && obstacleTilemap.HasTile(cellPos))
            {
                return false;
            }
        }
        // 만약 treatObstacleAsWall가 False(해제)라면, 위의 코드를 무시하고 아래로 내려감!
        // 즉, 장애물이 있어도 바닥만 있으면 통과 가능!

        // [바닥 체크] 물리 엔진(Collider)을 이용해 바닥이 있는지 확인
        Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellPos);
        Collider2D col = Physics2D.OverlapPoint(worldPos, moveLayer);

        if (col == null) return false; // 바닥 없으면 낙사

        return true; // 바닥 있으면 OK
    }

    private int CalculateStepCount(Node endNode, Color lineColor)
    {
        int count = 0;
        Node current = endNode;
        List<Vector3> debugPathPoints = new List<Vector3>();

        while (current != null)
        {
            count++;
            debugPathPoints.Add(groundTilemap.GetCellCenterWorld(current.gridPos));
            current = current.parent;
        }

        if (showDebugLine)
        {
            for (int i = 0; i < debugPathPoints.Count - 1; i++)
                Debug.DrawLine(debugPathPoints[i], debugPathPoints[i + 1], lineColor, 3f);
        }

        return count;
    }

    private int GetManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;
    }
}