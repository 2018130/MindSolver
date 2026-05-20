using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [SerializeField]
    private Transform origin;
    public Transform Origin { get => origin; set => origin = value; }
    [SerializeField]
    private Transform destination;
    public Transform Destination { get => destination; set => destination = value; }

    [SerializeField]
    private LayerMask moveLayer;

    [SerializeField]
    private Tile[,] tiles;
    private Tile endTile;

    private TileController tileController;

    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private PathFinding otherPathFinding;

    private List<Tile> road = new List<Tile>();

    // 서로다른 길 찾기가 수행된 횟수 카운팅
    private static int endRoadCount = 0;

    private void Start()
    {
        tileController = FindAnyObjectByType<TileController>();
        //StartCoroutine(PathFinding_co());
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
                Collider2D[] cols = Physics2D.OverlapCircleAll(tilePos, 0.01f, moveLayer);
                Debug.DrawLine(tilePos, tilePos + Vector2.up * 0.01f, Color.red, 1f);
                Debug.DrawLine(tilePos, tilePos + Vector2.down * 0.01f, Color.red, 1f);
                Debug.DrawLine(tilePos, tilePos + Vector2.left * 0.01f, Color.red, 1f);
                Debug.DrawLine(tilePos, tilePos + Vector2.right * 0.01f, Color.red, 1f);
                bool canMove = false;
                bool isObstacle = false;

                foreach(var col in cols)
                {
                    if ((gameObject.name.Contains("Red") && col.CompareTag("HostRoad")) ||
                        (gameObject.name.Contains("Blue") && col.CompareTag("ClientRoad")) ||
                        col.CompareTag("Untagged"))
                    {
                        canMove = true;
                    }

                    if (col != null && col.CompareTag("Obstacle"))
                    {
                        isObstacle = true;
                    }
                }


                tiles[i, j] = new Tile(new Vector2Int(j, i), isObstacle ? false : (canMove ? true : false));
            }
        }
    }

    public void StartPathFinding()
    {
        Debug.Log($"Start pathfinding {gameObject}");
        StartCoroutine(PathFinding_co());
    }

    private IEnumerator PathFinding_co()
    {
        endTile = null;
        CreateTiles();

        PriorityQueue<Tile> queue = new PriorityQueue<Tile>();

        int[] dx = new int[] { 0, 0, -1, 1 };
        int[] dy = new int[] { 1, -1, 0, 0 };

        Vector2Int startIndex = tileController.GetTileIndex(origin.position.x, origin.position.y);
        Vector2Int destIndex = tileController.GetTileIndex(destination.position.x, destination.position.y);

        Tile startTile = tiles[startIndex.y, startIndex.x];

        startTile.g = 0;
        startTile.h = (Mathf.Abs(destIndex.x - startIndex.x) + Mathf.Abs(destIndex.y - startIndex.y)) * 10;
        startTile.f = startTile.g + startTile.h;

        queue.Enqueue(startTile);

        while (queue.Count != 0)
        {
            Tile curTile = queue.Dequeue();

            if (curTile.closed) continue;

            curTile.closed = true;

            if (curTile.index.x == destIndex.x && curTile.index.y == destIndex.y)
            {
                endTile = curTile;
                break;
            }

            for (int i = 0; i < 4; i++)
            {
                int newIdxX = curTile.index.x + dx[i];
                int newIdxY = curTile.index.y + dy[i];

                if (newIdxX < 0 || newIdxX >= tileController.Tiles.GetLength(1) ||
                    newIdxY < 0 || newIdxY >= tileController.Tiles.GetLength(0))
                    continue;

                Tile checkTile = tiles[newIdxY, newIdxX];

                if (checkTile == null || !checkTile.canMove || checkTile.closed)
                    continue;


                int weight = 10;
                int newG = curTile.g + weight;
                int newH = (Mathf.Abs(destIndex.x - newIdxX) + Mathf.Abs(destIndex.y - newIdxY)) * weight;
                int newF = -(newG + newH);
                Debug.Log($"check node f : {curTile.f} new f : {newF}");
                if (checkTile.g == 0 || newG < checkTile.g)
                {
                    checkTile.g = newG;
                    checkTile.h = newH;
                    checkTile.f = newF;
                    checkTile.preTile = curTile;

                    queue.Enqueue(checkTile);
                }
            }

#if UNITY_EDITOR
            tileController.SpawnTile(new Vector2Int(curTile.index.x, curTile.index.y));
#endif

            //yield return null;
            yield return new WaitForSeconds(0.1f);
        }

        // 경로 역추적
        road.Clear();
        Tile tempTile = endTile;
        while (tempTile != null)
        {
            //Debug.Log($"{player.gameObject} move to {tempTile.index}");
            road.Add(tempTile);
            tempTile = tempTile.preTile;
        }
        road.Reverse();

        endRoadCount++;

        if(endRoadCount == 2)
        {
            bool isClear = road.Count == otherPathFinding.road.Count;

            StartCoroutine(MoveTo(road, 0.5f, isClear));
            StartCoroutine(otherPathFinding.MoveTo(otherPathFinding.road, 0.5f, isClear));
        }
    }

    private IEnumerator MoveTo(List<Tile> road, float duration, bool isClear)
    {
        if (road == null)
            yield break;
        for (int i = 0; i < road.Count; i++)
        {
            if(i > otherPathFinding.road.Count - 1)
            {
                Debug.Log($"미션 실패!!! {gameObject}의 최소 거리 : {road.Count} {otherPathFinding}의 최소 거리 : {otherPathFinding.road.Count}");
                GameUIManager.Singleton.SetclearText("미션 실패 ㅠㅜ, 3초 뒤에 재시작합니다.");
                
                yield return new WaitForSeconds(3f);

                if(TutorialSceneManager.singleton != null)
                {
                    TutorialSceneManager.singleton.EndOfPathfinding(false);
                }
                else
                {
                    //SceneChangeManager.Singleton.ChangeSceneByNetwork("Stage", 5f);
                }

                endRoadCount = 0;
                StopAllCoroutines();

                yield break;
            }

            Vector3 dest = tileController.GetTilePos(road[i].index.y, road[i].index.x);
            yield return player.MoveTo(dest);
        }

        if(gameObject.name.Contains("Red") && endRoadCount != 0)
        {
            Debug.Log(gameObject);
            if (isClear)
            {
                StageManager.SingletonManager?.ClearStage_ClientRpc();
            }
            else
            {
                if(TutorialSceneManager.singleton == null)
                {
                    SceneChangeManager.Singleton.ChangeSceneByNetwork("Stage", 5f);
                }
            }

            endRoadCount = 0;
        }

        TutorialSceneManager.singleton?.EndOfPathfinding(isClear);
    }
}
