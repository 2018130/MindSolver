using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Puzzle/ConnectLine/LevelData")]
public class ConnectLinePuzzleData : ScriptableObject
{
    [Header("격자 크기")]
    public int gridSize = 10;

    [Header("정답 경로 (순서대로 저장됨)")]
    public List<Vector2Int> correctPath = new List<Vector2Int>();
}
