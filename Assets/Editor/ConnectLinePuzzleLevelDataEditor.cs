using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConnectLinePuzzleData))]
public class ConnectLinePuzzleLevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 원본 데이터를 가져옵니다.
        ConnectLinePuzzleData levelData = (ConnectLinePuzzleData)target;

        // 기본 인스펙터(gridSize 등)를 먼저 그려줍니다.
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎯 정답 그리기 (순서대로 클릭하세요)", EditorStyles.boldLabel);

        // 격자 버튼 UI 그리기
        // 위에서 아래로 그려지므로 y는 큰 값부터 줄어들게 (그래야 시각적으로 바닥이 y=0이 됨)
        for (int y = levelData.gridSize - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal(); // 가로로 버튼 나열 시작

            for (int x = 0; x < levelData.gridSize; x++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                int pathIndex = levelData.correctPath.IndexOf(currentPos);

                // 리스트에 들어있으면 순서(번호) 표시, 아니면 점(·) 표시
                string buttonText = pathIndex != -1 ? (pathIndex + 1).ToString() : "·";

                // 선택된 버튼은 초록색으로 칠하기
                GUI.backgroundColor = pathIndex != -1 ? Color.green : Color.white;

                // 정사각형 버튼 생성 (너비 40, 높이 40)
                if (GUILayout.Button(buttonText, GUILayout.Width(40), GUILayout.Height(40)))
                {
                    if (pathIndex == -1)
                    {
                        // 아직 선택 안 된 버튼이면 정답 경로에 추가
                        levelData.correctPath.Add(currentPos);
                    }
                    else if (pathIndex == levelData.correctPath.Count - 1)
                    {
                        // 방금 선택한(가장 마지막) 버튼을 다시 누르면 취소 (순서 꼬임 방지)
                        levelData.correctPath.RemoveAt(pathIndex);
                    }

                    // 데이터가 변경되었음을 유니티에 알림 (저장을 위해 필수)
                    EditorUtility.SetDirty(levelData);
                }

                GUI.backgroundColor = Color.white; // 색상 원상복구
            }
            EditorGUILayout.EndHorizontal(); // 가로 나열 끝
        }

        EditorGUILayout.Space(10);

        // 경로 싹 지우기 버튼
        if (GUILayout.Button("경로 초기화 (Clear Path)", GUILayout.Height(30)))
        {
            levelData.correctPath.Clear();
            EditorUtility.SetDirty(levelData);
        }
    }
}
