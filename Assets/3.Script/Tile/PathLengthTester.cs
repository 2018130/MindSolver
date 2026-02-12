using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 두 개의 서로 다른 타일맵(층)에 대해 각각 최단 경로를 계산하는 클래스입니다.
/// A* (A-Star) 알고리즘을 기반으로 작동하며, 타일맵의 존재 여부와 물리적 충돌체(Collider)를 검사합니다.
/// </summary>
public class PathLengthTester : MonoBehaviour
{
    [Header("[공통 설정] 환경 및 물리")]
    [Tooltip("체크 시 장애물 타일맵에 타일이 존재하면 이동 불가능한 벽으로 인식합니다.")]
    public bool treatObstacleAsWall = true;

    [Tooltip("이동 가능한 바닥 타일인지 확인하기 위한 물리 레이어입니다. (Tilemap Collider 2D가 설정된 레이어)")]
    public LayerMask moveLayer;

    [Tooltip("체크 시 씬 뷰(Scene View)에 계산된 경로를 시각적으로 표시합니다.")]
    public bool showDebugLine = true;


    [Header("[루트 1 설정] 1층 / Player A")]
    [Tooltip("루트 1의 경로 탐색 기준이 되는 바닥 타일맵")]
    public Tilemap groundTilemap1;
    [Tooltip("루트 1의 장애물 타일맵 (선택 사항)")]
    public Tilemap obstacleTilemap1;
    public Transform startPoint1;
    public Transform endPoint1;
    [Tooltip("계산된 루트 1의 경로 길이 (타일 개수)")]
    public int pathCount1 = 0;


    [Header("[루트 2 설정] 2층 / Player B")]
    [Tooltip("루트 2의 경로 탐색 기준이 되는 바닥 타일맵")]
    public Tilemap groundTilemap2;
    [Tooltip("루트 2의 장애물 타일맵 (선택 사항)")]
    public Tilemap obstacleTilemap2;
    public Transform startPoint2;
    public Transform endPoint2;
    [Tooltip("계산된 루트 2의 경로 길이 (타일 개수)")]
    public int pathCount2 = 0;


    /// <summary>
    /// A* 알고리즘에서 사용하는 노드 클래스입니다.
    /// 각 타일의 좌표와 비용(Cost), 부모 노드 정보를 담습니다.
    /// </summary>
    private class Node
    {
        public Vector3Int gridPos; // 타일맵 그리드 좌표
        public Node parent;        // 경로 추적을 위한 부모 노드

        public int g; // 시작점으로부터 현재 노드까지의 이동 비용 (G Cost)
        public int h; // 현재 노드로부터 목적지까지의 예상 비용 (H Cost, 휴리스틱)
        public int f => g + h; // 총 비용 (F Cost = G + H)

        public Node(Vector3Int pos) { gridPos = pos; }
    }

    /// <summary>
    /// 인스펙터의 컴포넌트 메뉴에서 '경로 계산 실행'을 클릭하면 호출됩니다.
    /// 두 개의 루트를 독립적으로 계산합니다.
    /// </summary>
    [ContextMenu("경로 계산 실행")]
    public void CalculatePath()
    {
        // 루트 1 계산 (1층 타일맵 사용)
        if (ValidateSetup(groundTilemap1, startPoint1, endPoint1, "루트 1"))
        {
            pathCount1 = ProcessPath(startPoint1, endPoint1, groundTilemap1, obstacleTilemap1, Color.cyan, "루트 1");
        }
        else
        {
            pathCount1 = 0;
        }

        // 루트 2 계산 (2층 타일맵 사용)
        if (ValidateSetup(groundTilemap2, startPoint2, endPoint2, "루트 2"))
        {
            pathCount2 = ProcessPath(startPoint2, endPoint2, groundTilemap2, obstacleTilemap2, Color.magenta, "루트 2");
        }
        else
        {
            pathCount2 = 0;
        }

        Debug.Log("--- 경로 계산 프로세스 종료 ---");
    }

    // 설정 값 유효성 검사
    private bool ValidateSetup(Tilemap map, Transform start, Transform end, string routeName)
    {
        if (map != null && start != null && end != null) return true;

        Debug.LogError($"[설정 오류] {routeName}: 타일맵 또는 시작/도착 지점이 할당되지 않았습니다.");
        return false;
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환하고, 실제 경로 탐색 알고리즘을 호출합니다.
    /// </summary>
    private int ProcessPath(Transform start, Transform end, Tilemap groundMap, Tilemap obstacleMap, Color debugColor, string routeName)
    {
        // 월드 좌표(Transform)를 타일맵의 그리드 좌표(Cell)로 변환
        Vector3Int startCell = groundMap.WorldToCell(start.position);
        Vector3Int endCell = groundMap.WorldToCell(end.position);

        // [중요] 아이소메트릭 타일맵에서 Z축 좌표 오차로 인한 인식 불가 현상을 방지하기 위해 Z를 0으로 초기화
        startCell.z = 0;
        endCell.z = 0;

        // 시작 지점 유효성 검사 (타일 존재 여부 및 장애물 여부)
        if (!IsWalkable(startCell, groundMap, obstacleMap))
        {
            Debug.LogError($"[탐색 실패] {routeName}: 시작 지점 {startCell}이 이동 불가능한 위치(타일 없음 또는 장애물)입니다.");
            return 0;
        }

        // A* 알고리즘 수행
        int count = FindPath(startCell, endCell, groundMap, obstacleMap, debugColor);

        if (count > 0)
            Debug.Log($"[탐색 성공] {routeName}: 총 {count}칸의 경로를 찾았습니다.");
        else
            Debug.LogError($"[탐색 실패] {routeName}: 목적지까지 도달할 수 있는 경로가 없습니다.");

        return count;
    }

    /// <summary>
    /// A* 알고리즘을 사용하여 최단 경로를 탐색합니다.
    /// </summary>
    private int FindPath(Vector3Int startPos, Vector3Int targetPos, Tilemap groundMap, Tilemap obstacleMap, Color debugColor)
    {
        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        // 탐색할 노드 목록 (Open List)
        List<Node> openList = new List<Node> { startNode };
        // 이미 탐색을 마친 노드 목록 (Closed List)
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        // 상, 하, 좌, 우 방향 벡터
        Vector3Int[] directions = {
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
        };

        while (openList.Count > 0)
        {
            // Open List 중에서 F 비용(G+H)이 가장 낮은 노드를 현재 노드로 선택
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
                return TracePath(currentNode, groundMap, debugColor);
            }

            // 인접한 4방향 타일 탐색
            foreach (var dir in directions)
            {
                Vector3Int neighborPos = currentNode.gridPos + dir;

                // 이미 탐색했거나 이동 불가능한 타일이면 스킵
                if (closedList.Contains(neighborPos) || !IsWalkable(neighborPos, groundMap, obstacleMap))
                    continue;

                // 이동 비용 계산 (기본 비용 10 가정)
                int newMovementCost = currentNode.g + 10;
                Node neighbor = openList.Find(n => n.gridPos == neighborPos);

                // 새로운 경로가 더 효율적이거나, 처음 발견한 노드라면 정보 갱신
                if (neighbor == null || newMovementCost < neighbor.g)
                {
                    if (neighbor == null)
                    {
                        neighbor = new Node(neighborPos);
                        openList.Add(neighbor);
                    }

                    neighbor.g = newMovementCost;
                    neighbor.h = GetManhattanDistance(neighborPos, targetPos); // 휴리스틱 계산
                    neighbor.parent = currentNode; // 경로 추적을 위해 부모 설정
                }
            }
        }

        return 0; // 경로 없음
    }

    /// <summary>
    /// 해당 좌표가 이동 가능한지 검사합니다. (3단계 검증)
    /// 1. 바닥 타일 존재 여부
    /// 2. 장애물 타일 존재 여부
    /// 3. 물리적 충돌체(Collider) 존재 여부
    /// </summary>
    private bool IsWalkable(Vector3Int cellPos, Tilemap groundMap, Tilemap obstacleMap)
    {
        // 1. 바닥 타일맵에 해당 좌표의 타일 데이터가 있는지 확인
        if (!groundMap.HasTile(cellPos)) return false;

        // 2. 장애물 타일맵 검사 (옵션이 켜져 있을 경우)
        if (treatObstacleAsWall && obstacleMap != null)
        {
            if (obstacleMap.HasTile(cellPos)) return false;
        }

        // 3. 물리 엔진을 이용한 바닥 콜라이더 검사
        // 타일의 월드 중앙 좌표를 가져옴
        Vector3 worldPos = groundMap.GetCellCenterWorld(cellPos);
        // 해당 지점에 moveLayer에 해당하는 콜라이더가 있는지 확인
        Collider2D col = Physics2D.OverlapPoint(worldPos, moveLayer);

        return col != null;
    }

    /// <summary>
    /// 도착점에서 시작점까지 부모 노드를 역추적하여 경로의 길이를 계산하고, 디버그 라인을 그립니다.
    /// </summary>
    private int TracePath(Node endNode, Tilemap map, Color lineColor)
    {
        int count = 0;
        Node current = endNode;
        List<Vector3> pathPoints = new List<Vector3>();

        // 부모 노드를 따라 시작점까지 역추적
        while (current != null)
        {
            count++;
            pathPoints.Add(map.GetCellCenterWorld(current.gridPos));
            current = current.parent;
        }

        // 씬 뷰에 경로 그리기
        if (showDebugLine && pathPoints.Count > 1)
        {
            for (int i = 0; i < pathPoints.Count - 1; i++)
                Debug.DrawLine(pathPoints[i], pathPoints[i + 1], lineColor, 5f); // 5초간 표시
        }

        return count; // 경로에 포함된 타일의 총 개수 반환
    }

    /// <summary>
    /// 휴리스틱 함수: 맨해튼 거리 (Manhattan Distance) 계산
    /// 대각선 이동이 없을 때 주로 사용하는 거리 계산 방식입니다.
    /// </summary>
    private int GetManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;
    }
}