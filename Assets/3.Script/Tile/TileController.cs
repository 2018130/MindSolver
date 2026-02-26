using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [Header("Setting"), Space(10f)]
    [SerializeField, Tooltip("격자를 정확히 맞춰서 넣어야 함")]
    private Transform startPoint;
    [SerializeField, Tooltip("격자를 정확히 맞춰서 넣어야 함")]
    private Transform destPoint;

    [Header("Tile Info"), Space(10f)]

    [SerializeField]
    private float widthPerTile = 0.5f;
    [SerializeField]
    private float heightPerTile = 0.25f;
    [SerializeField]
    private GameObject[,] tiles;
    public GameObject[,] Tiles => tiles;

    [Header("Debug"), Space(10f)]
    [SerializeField]
    private GameObject tilePointTransformPrefab;

    private void Awake()
    {
        InitTileInfo();
    }

    private void InitTileInfo()
    {
        float dx = destPoint.position.x - startPoint.position.x;
        float dy = destPoint.position.y - startPoint.position.y;

        float xIndexRaw = 0.5f * (dx / widthPerTile + dy / heightPerTile);
        float yIndexRaw = 0.5f * (dy / heightPerTile - dx / widthPerTile);

        int widthSize = Mathf.Abs(Mathf.RoundToInt(xIndexRaw));
        int heightSize = Mathf.Abs(Mathf.RoundToInt(yIndexRaw));

        widthSize = Mathf.Max(1, widthSize);
        heightSize = Mathf.Max(1, heightSize);

        tiles = new GameObject[heightSize + 1, widthSize + 1]; 
        Debug.Log($"Calculated Size -> Width: {widthSize}, Height: {heightSize}");

        /*
        for (int i = 0; i <= heightSize; i++) // <= 로 수정하여 끝점까지 생성
        {
            for (int j = 0; j <= widthSize; j++)
            {
                Instantiate(tilePointTransformPrefab, GetTilePos(i, j), Quaternion.identity);
            }
        }*/
    }

    public Vector2Int GetTileIndex(float xPos, float yPos)
    {
        float dx = xPos - startPoint.position.x;
        float dy = yPos - startPoint.position.y;

        float rawXIndex = 0.5f * (dx / widthPerTile + dy / heightPerTile);
        float rawYIndex = 0.5f * (dy / heightPerTile - dx / widthPerTile);

        return new Vector2Int(Mathf.RoundToInt(rawXIndex), Mathf.RoundToInt(rawYIndex));
    }

    public Vector2 GetTilePos(int yIndex, int xIndex)
    {
        // 수평이동 (X, Y 둘 다 증가)
        float posX = startPoint.position.x + xIndex * widthPerTile;
        float posY = startPoint.position.y + xIndex * heightPerTile;

        // 수직 이동 (X는 감소, Y는 증가)
        posX -= yIndex * widthPerTile;
        posY += yIndex * heightPerTile;

        return new Vector2(posX, posY);
    }

    public void SpawnTile(Vector2Int idx)
    {
        Instantiate(tilePointTransformPrefab, GetTilePos(idx.y, idx.x), Quaternion.identity);
    }
}
