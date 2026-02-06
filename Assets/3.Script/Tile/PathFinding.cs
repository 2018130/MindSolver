using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [SerializeField]
    private Transform origin;
    [SerializeField]
    private Transform destination;

    [SerializeField]
    private LayerMask moveLayer;

    [SerializeField]
    private Tile[,] tiles;
    private Tile endTile;

    private TileController tileController;

    [SerializeField]
    private PlayerController player;

    private void Start()
    {
        tileController = FindAnyObjectByType<TileController>();
        StartCoroutine(PathFinding_co());
    }

    private void CreateTiles()
    {
        int cal = tileController.Tiles.GetLength(0);
        int row = tileController.Tiles.GetLength(1);
        tiles = new Tile[cal, row];

        for(int i = 0; i < cal; i++)
        {
            for(int j = 0; j < row; j++)
            {
                Vector2 tilePos = tileController.GetTilePos(i, j);
                Collider2D col = Physics2D.OverlapCircle(tilePos, 0.01f, moveLayer);
                bool canMove = false;

                if(col != null && !col.CompareTag("Obstacle"))
                {
                    Debug.Log($"{i} {j} col count : {col.name}");
                    canMove = true;
                }

                tiles[i, j] = new Tile(new Vector2Int(j, i), canMove);
                //Debug.Log($"Create tile {i}, {j} canMove : {canMove}");
            }
        }
    }


    private IEnumerator PathFinding_co()
    {
        // 초기화
        endTile = null;
        CreateTiles(); // 주의: 이미 생성된 타일이 있다면 중복 생성될 수 있으므로 확인 필요

        // Open List 역할을 하는 우선순위 큐
        PriorityQueue<Tile> queue = new PriorityQueue<Tile>();

        // 방문 여부와 별개로, 현재 큐에 들어있는지 확인하기 위한 변수나 플래그가 있으면 좋습니다.
        // 여기서는 간단히직접 타일의 상태로 판단한다고 가정합니다.

        int[] dx = new int[] { 0, 0, -1, 1 };
        int[] dy = new int[] { 1, -1, 0, 0 };

        Vector2Int startIndex = tileController.GetTileIndex(origin.position.x, origin.position.y);
        Vector2Int destIndex = tileController.GetTileIndex(destination.position.x, destination.position.y);

        Tile startTile = tiles[startIndex.y, startIndex.x];

        // 시작 타일 설정
        startTile.g = 0;
        startTile.h = (Mathf.Abs(destIndex.x - startIndex.x) + Mathf.Abs(destIndex.y - startIndex.y)) * 10;
        startTile.f = startTile.g + startTile.h;

        queue.Enqueue(startTile); // 큐 구현에 따라 (item, priority) 형태일 수 있음

        while (queue.Count != 0)
        {
            // 1. 가장 F값이 낮은 타일을 꺼냄
            Tile curTile = queue.Dequeue();

            // 이미 방문한 곳이라면 스킵 (큐에 중복으로 들어갔을 경우 대비)
            if (curTile.closed) continue;

            curTile.closed = true;

            // 2. 목적지 도착 확인
            if (curTile.index.x == destIndex.x && curTile.index.y == destIndex.y)
            {
                endTile = curTile;
                break;
            }

            // 3. 이웃 타일 검사 (4방향)
            for (int i = 0; i < 4; i++)
            {
                int newIdxX = curTile.index.x + dx[i];
                int newIdxY = curTile.index.y + dy[i];
                //Debug.Log($"check new idx : {newIdxY}, {newIdxX}");

                // 맵 범위 체크
                if (newIdxX < 0 || newIdxX >= tileController.Tiles.GetLength(1) ||
                    newIdxY < 0 || newIdxY >= tileController.Tiles.GetLength(0))
                    continue;

                Tile checkTile = tiles[newIdxY, newIdxX];

                if (checkTile == null || !checkTile.canMove || checkTile.closed)
                    continue;


                int weight = 10;
                int newG = curTile.g + weight;
                int newH = (Mathf.Abs(destIndex.x - newIdxX) + Mathf.Abs(destIndex.y - newIdxY)) * weight;
                int newF = newG + newH;

                if (checkTile.g == 0 || true)
                {
                    checkTile.g = newG;
                    checkTile.h = newH;
                    checkTile.f = -newF;
                    checkTile.preTile = curTile;

                    queue.Enqueue(checkTile);

                    tileController.SpawnTile(new Vector2Int(newIdxX, newIdxY));
                }
            }

            // 시각화 딜레이는 여기서 주는 것이 퍼포먼스와 보기에도 좋습니다.
            yield return new WaitForSeconds(0.05f);
        }

        // 경로 역추적
        List<Tile> road = new List<Tile>();
        Tile tempTile = endTile;
        while (tempTile != null)
        {
            road.Add(tempTile);
            tempTile = tempTile.preTile;
        }
        road.Reverse();

        if (road.Count > 0)
            yield return MoveTo(road, 1f);
        else
            Debug.Log("Failed to find Path.");
    }

    private IEnumerator MoveTo(List<Tile> road, float duration)
    {
        if (road == null)
            yield break;
        //
        for(int i = 0; i < road.Count; i++)
        {
            Vector3 dest = tileController.GetTilePos(road[i].index.y, road[i].index.x);

            yield return player.MoveTo(dest);
        }
    }
}
