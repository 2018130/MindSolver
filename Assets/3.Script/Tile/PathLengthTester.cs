using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathLengthTester : MonoBehaviour
{
    [Header("[1. 타일맵 및 물리 설정]")]
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;
    public LayerMask moveLayer;

    [Header("[2. 플레이어 설정]")]
    public Transform startPoint1;
    public Transform endPoint1;
    public int pathCount1 = 0; // 이제 빨간불 안 떠요!

    public Transform startPoint2;
    public Transform endPoint2;
    public int pathCount2 = 0;

    [ContextMenu("최단거리 계산 실행")]
    public void RunCalculate()
    {
        Debug.Log("<color=yellow> 정밀 보정 탐색을 시작합니다!</color>");
        pathCount1 = GetBFSPathCount(startPoint1, endPoint1, Color.red, "플레이어 1");
        pathCount2 = GetBFSPathCount(startPoint2, endPoint2, Color.blue, "플레이어 2");
    }

    private int GetBFSPathCount(Transform start, Transform end, Color debugColor, string label)
    {
        if (start == null || end == null) return 0;

        // 주변 타일을 찾는 정밀 보정 로직
        Vector3Int startCell = FindNearestTile(start.position);
        Vector3Int targetCell = FindNearestTile(end.position);

        if (startCell == new Vector3Int(-999, -999, -999) || targetCell == new Vector3Int(-999, -999, -999))
        {
            Debug.LogError($"{label}: 주변에 바닥 타일을 찾을 수 없습니다! 위치를 확인해주세요.");
            return 0;
        }

        // BFS 알고리즘 시작
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> dist = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(startCell);
        dist[startCell] = 0;
        Vector3Int[] neighbors = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        bool found = false;
        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (current == targetCell) { found = true; break; }
            foreach (var n in neighbors)
            {
                Vector3Int next = current + n;
                if (!dist.ContainsKey(next) && IsWalkableSimple(next))
                {
                    dist[next] = dist[current] + 1;
                    parent[next] = current;
                    queue.Enqueue(next);
                }
            }
        }

        if (found)
        {
            Debug.Log($"<color=green>{label}: 길 찾기 성공! 총 {dist[targetCell]}칸</color>");
            DrawDebugPath(parent, targetCell, debugColor);
            return dist[targetCell];
        }

        Debug.LogError($"{label}: 경로가 벽으로 막혀 있습니다.");
        return 0;
    }

    private Vector3Int FindNearestTile(Vector3 pos)
    {
        Vector3Int baseCell = groundTilemap.WorldToCell(pos);
        baseCell.z = 0;
        if (groundTilemap.HasTile(baseCell)) return baseCell;

        // 주변 1칸(3x3) 탐색
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = baseCell + new Vector3Int(x, y, 0);
                if (groundTilemap.HasTile(neighbor)) return neighbor;
            }
        }
        return new Vector3Int(-999, -999, -999);
    }

    private bool IsWalkableSimple(Vector3Int cellPos)
    {
        if (!groundTilemap.HasTile(cellPos)) return false;
        if (obstacleTilemap != null && obstacleTilemap.HasTile(cellPos)) return false;
        return Physics2D.OverlapCircle(groundTilemap.GetCellCenterWorld(cellPos), 0.1f, moveLayer) != null;
    }

    private void DrawDebugPath(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int target, Color col)
    {
        Vector3Int curr = target;
        while (parent.ContainsKey(curr))
        {
            Debug.DrawLine(groundTilemap.GetCellCenterWorld(curr), groundTilemap.GetCellCenterWorld(parent[curr]), col, 5f);
            curr = parent[curr];
        }
    }
}