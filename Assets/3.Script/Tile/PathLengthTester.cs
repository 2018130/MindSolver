using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathLengthTester : MonoBehaviour
{
    [Header("공통 환경 설정")]
    [Tooltip("좌표 계산의 기준이 되는 바닥 타일맵입니다.")]
    public Tilemap groundTilemap;
    [Tooltip("이동 가능한 타일만 감지하기 위한 레이어 설정입니다.")]
    public LayerMask moveLayer;
    [Tooltip("체크 시 장애물(Obstacle) 태그가 있는 타일도 이동 가능한 경로로 포함하여 계산합니다.")]
    public bool includeObstacles = true;
    [Tooltip("장애물로 인식할 태그의 이름입니다.")]
    public string obstacleTag = "Obstacle";
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
        public Vector3Int gridPos; // 타일의 그리드 좌표
        public Node parent;        // 경로 역추적을 위한 부모 노드
        public int g;              // 시작점으로부터 현재 노드까지의 이동 비용
        public int h;              // 현재 노드부터 목적지까지의 예상 비용 (휴리스틱)
        public int f => g + h;     // 총 비용 (F = G + H)

        public Node(Vector3Int pos) { gridPos = pos; }
    }

    // 컴포넌트 우클릭 메뉴를 통해 함수를 실행합니다.
    [ContextMenu("두 경로 동시에 계산하기")]
    public void CalculatePath()
    {
        // 필수 참조 요소가 연결되어 있는지 검사합니다.
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
            Debug.LogWarning("루트 1: 시작점 또는 도착점이 설정되지 않아 계산을 건너뜁니다.");
            pathCount1 = 0;
        }

        // --- 루트 2 계산 ---
        if (startPoint2 != null && endPoint2 != null)
        {
            pathCount2 = ProcessPath(startPoint2, endPoint2, Color.magenta, "루트 2");
        }
        else
        {
            Debug.LogWarning("루트 2: 시작점 또는 도착점이 설정되지 않아 계산을 건너뜁니다.");
            pathCount2 = 0;
        }

        Debug.Log("---------- 계산 종료 ----------");
    }

    // 각 루트의 유효성을 검사하고 길찾기를 실행하는 내부 함수
    private int ProcessPath(Transform start, Transform end, Color debugColor, string routeName)
    {
        Vector3Int startCell = groundTilemap.WorldToCell(start.position);
        Vector3Int endCell = groundTilemap.WorldToCell(end.position);

        // 시작점 유효성 검사
        if (!IsWalkable(startCell))
        {
            Debug.LogError($"오류 [{routeName}]: 시작 지점({startCell})에서 이동 가능한 타일을 찾을 수 없습니다.");
            return 0;
        }

        int count = FindPath(startCell, endCell, debugColor);

        if (count > 0)
            Debug.Log($"[{routeName}] 탐색 성공: 총 {count}칸");
        else
            Debug.LogError($"[{routeName}] 탐색 실패: 경로를 찾을 수 없습니다.");

        return count;
    }

    // A* 알고리즘을 사용하여 최단 경로를 탐색하는 함수
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

            // 목적지 도착 확인
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

    // 물리 엔진을 이용한 타일 이동 가능 여부 체크
    private bool IsWalkable(Vector3Int cellPos)
    {
        Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellPos);
        Collider2D col = Physics2D.OverlapPoint(worldPos, moveLayer);

        if (col == null) return false;

        if (col.CompareTag(obstacleTag))
        {
            return includeObstacles;
        }

        return true;
    }

    // 경로 역추적 및 결과 반환 (디버그 라인 그리기 포함)
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

        // 씬 뷰에 경로 그리기 (루트별로 다른 색상 적용)
        if (showDebugLine)
        {
            for (int i = 0; i < debugPathPoints.Count - 1; i++)
                Debug.DrawLine(debugPathPoints[i], debugPathPoints[i + 1], lineColor, 3f);
        }

        return count;
    }

    // 맨해튼 거리 휴리스틱
    private int GetManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;
    }
}