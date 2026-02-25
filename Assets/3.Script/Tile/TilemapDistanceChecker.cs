using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// [주의] ReadOnlyAttribute는 FallingTilemapEffect.cs에 정의된 것을 공유합니다.

public class TilemapDistanceChecker : MonoBehaviour
{
    [Header("맵 설정")]
    [Tooltip("모든 타일맵의 부모인 Grid를 넣어주세요.")]
    public Grid mapGrid;

    [Header("루트 1 설정")]
    public Transform startTarget1;
    public Transform endTarget1;

    [Header("루트 2 설정")]
    public Transform startTarget2;
    public Transform endTarget2;

    [Header("탐색 기준 (콜라이더)")]
    [Tooltip("루트 1, 2 타일맵이 사용하는 모든 레이어를 체크하세요.")]
    public LayerMask moveLayer;
    public string obstacleTag = "Obstacle";

    [Header("디버그 옵션")]
    public bool showDebugVisuals = true;

    [Header("루트 1 결과 (ReadOnly)")]
    [ReadOnly] public int shortestDistance1 = 0;
    [ReadOnly] public string pathCountDisplay1 = "0";

    [Header("루트 2 결과 (ReadOnly)")]
    [ReadOnly] public int shortestDistance2 = 0;
    [ReadOnly] public string pathCountDisplay2 = "0";

    private List<List<Vector3Int>> allShortestPaths1 = new List<List<Vector3Int>>();
    private List<List<Vector3Int>> allShortestPaths2 = new List<List<Vector3Int>>();
    private HashSet<Vector3Int> walkableTiles = new HashSet<Vector3Int>();

#if UNITY_EDITOR

    [ContextMenu("최단 거리 및 모든 경로 계산")]
    public void CalculateAllShortestPaths()
    {
        if (mapGrid == null)
        {
            Debug.LogError("오류: Grid 오브젝트가 할당되지 않았습니다.");
            return;
        }

        allShortestPaths1.Clear();
        allShortestPaths2.Clear();
        walkableTiles.Clear();

        // 각각의 시작점이 놓인 Z층을 기준으로 독립적인 탐색을 수행합니다.
        CalculatePath(startTarget1, endTarget1, ref shortestDistance1, ref pathCountDisplay1, allShortestPaths1, "루트 1");
        CalculatePath(startTarget2, endTarget2, ref shortestDistance2, ref pathCountDisplay2, allShortestPaths2, "루트 2");
    }

    private void CalculatePath(Transform start, Transform end, ref int outDistance, ref string outDisplay, List<List<Vector3Int>> outPaths, string label)
    {
        outDistance = 0;
        outDisplay = "0";

        if (start == null || end == null) return;

        Vector3Int startGridPos = mapGrid.WorldToCell(start.position);
        Vector3Int endGridPos = mapGrid.WorldToCell(end.position);

        // 각 루트가 위치한 층(Z값)을 고정하여 탐색합니다.
        int floorZ = startGridPos.z;
        endGridPos.z = floorZ;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, List<Vector3Int>> predecessors = new Dictionary<Vector3Int, List<Vector3Int>>();

        queue.Enqueue(startGridPos);
        distances[startGridPos] = 0;
        bool found = false;

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (found && distances[current] >= outDistance) continue;

            foreach (var dir in directions)
            {
                Vector3Int next = current + dir;
                next.z = floorZ;

                if (IsWalkableAt(next))
                {
                    if (showDebugVisuals) walkableTiles.Add(next);

                    if (!distances.ContainsKey(next))
                    {
                        distances[next] = distances[current] + 1;
                        predecessors[next] = new List<Vector3Int> { current };
                        queue.Enqueue(next);

                        if (next == endGridPos)
                        {
                            found = true;
                            outDistance = distances[next];
                        }
                    }
                    else if (distances[next] == distances[current] + 1)
                    {
                        predecessors[next].Add(current);
                    }
                }
            }
        }

        if (found)
        {
            bool limitReached = false;
            // 여기서 BacktrackAllPaths를 호출하며 6개의 인자를 정확히 전달합니다.
            BacktrackAllPaths(endGridPos, startGridPos, predecessors, new List<Vector3Int>(), outPaths, ref limitReached);
            outDisplay = limitReached ? "100+" : outPaths.Count.ToString();
        }
        else
        {
            outDisplay = "경로 없음";
            Debug.LogError($"[{label} 실패] {startGridPos} 층에서 경로를 찾을 수 없습니다.");
        }
    }

    private bool IsWalkableAt(Vector3Int pos)
    {
        Vector3 worldPos = mapGrid.GetCellCenterWorld(pos);
        // 레이어마스크에 포함된 모든 층의 콜라이더를 검사합니다.
        Collider2D col = Physics2D.OverlapCircle(worldPos, 0.1f, moveLayer);
        return col != null && !col.CompareTag(obstacleTag);
    }

    // [정의] 6개의 매개변수를 가진 BacktrackAllPaths 함수입니다.
    private void BacktrackAllPaths(Vector3Int current, Vector3Int startGridPos, Dictionary<Vector3Int, List<Vector3Int>> predecessors, List<Vector3Int> path, List<List<Vector3Int>> allPaths, ref bool limitReached)
    {
        if (limitReached) return;

        path.Add(current);

        if (current == startGridPos)
        {
            List<Vector3Int> finalPath = new List<Vector3Int>(path);
            finalPath.Reverse();
            allPaths.Add(finalPath);

            if (allPaths.Count >= 100)
            {
                limitReached = true;
            }
        }
        else if (predecessors.ContainsKey(current))
        {
            foreach (var prev in predecessors[current])
            {
                if (limitReached) break;
                // 재귀 호출 시에도 6개의 인자를 그대로 넘겨줍니다.
                BacktrackAllPaths(prev, startGridPos, predecessors, new List<Vector3Int>(path), allPaths, ref limitReached);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mapGrid == null) return;

        if (showDebugVisuals)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            foreach (var pos in walkableTiles)
            {
                Gizmos.DrawCube(mapGrid.GetCellCenterWorld(pos), mapGrid.cellSize * 0.7f);
            }
        }

        DrawPaths(allShortestPaths1, Color.yellow);
        DrawTarget(startTarget1, Color.green);
        DrawTarget(endTarget1, Color.red);

        DrawPaths(allShortestPaths2, Color.cyan);
        DrawTarget(startTarget2, new Color(0, 1, 0, 0.4f));
        DrawTarget(endTarget2, new Color(1, 0, 0, 0.4f));
    }

    private void DrawPaths(List<List<Vector3Int>> paths, Color color)
    {
        if (paths == null || paths.Count == 0) return;
        Gizmos.color = color;
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(mapGrid.GetCellCenterWorld(path[i]), mapGrid.GetCellCenterWorld(path[i + 1]));
            }
        }
    }

    private void DrawTarget(Transform target, Color color)
    {
        if (target == null) return;
        Gizmos.color = color;
        Gizmos.DrawWireCube(mapGrid.GetCellCenterWorld(mapGrid.WorldToCell(target.position)), mapGrid.cellSize * 0.9f);
    }
#endif
}