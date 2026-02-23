using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LineChecker : NetworkBehaviour
{
    [SerializeField]
    private float outLineLength = 1f;

    private DrawLineWithMesh _drawLineWithMesh;
    private CatmullRomPath _carmullRomPath;

    private static int checkCount = 0;
    private bool isChecked = false;

    private void Awake()
    {
        _carmullRomPath = GetComponent<CatmullRomPath>();
    }

    public void Initialize(DrawLineWithMesh drawLineWithMesh)
    {
        Debug.Log($"Init line checker, owner : {drawLineWithMesh.IsOwner} {gameObject}");
        _drawLineWithMesh = drawLineWithMesh;
        isChecked = false;
        checkCount = 0; // 초기화 시 카운트도 리셋
    }

    public void StartGame(MultiMissionType multiMissionType)
    {
        StartGame_ClientRpc();
    }

    [ClientRpc]
    private void StartGame_ClientRpc()
    {
    }

    public void CheckDistance(NetworkListEvent<Vector3> networkListEvent)
    {
        if (_drawLineWithMesh == null || isChecked ||
            _drawLineWithMesh.Vertices.Count <= 2)
        {
            return;
        }

        Debug.Log(_carmullRomPath.Waypoints.Count + " " + _drawLineWithMesh.Vertices.Count);

        if (_drawLineWithMesh.Vertices.Count % 2 == 0)
        {
            // 1. 선의 끝부분 중간점 구하기 (현재 로컬 좌표)
            Vector3 localMiddlePoint = (_drawLineWithMesh.Vertices[_drawLineWithMesh.Vertices.Count - 1] +
                                        _drawLineWithMesh.Vertices[_drawLineWithMesh.Vertices.Count - 2]) / 2f;

            // 💡 중요: 로컬 좌표를 월드 좌표로 변환하여 웨이포인트와 좌표계를 맞춥니다.
            Vector3 worldMiddlePoint = _drawLineWithMesh.transform.TransformPoint(localMiddlePoint);

            for (int i = 0; i < _carmullRomPath.Waypoints.Count; i++)
            {
                // X값 초과 여부를 '월드 좌표' 기준으로 검사합니다.
                if (_carmullRomPath.Waypoints[i].x > worldMiddlePoint.x)
                {
                    float successTargetX = Camera.main.ViewportToWorldPoint(new Vector3(0.8f, 0f, 0f)).x;

                    if (worldMiddlePoint.x >= successTargetX)
                    {
                        if (!isChecked)
                        {
                            isChecked = true;

                            // 💡 현재 static을 뺐기 때문에 이 객체 혼자서는 endCount가 2가 될 수 없습니다.
                            // 두 선이 모두 도착했는지(성공했는지)는 PuzzleMissonListener에서 체크하도록 넘기는 것이 정석입니다.
                            Debug.Log($"{gameObject} 미션 부분 성공!");
                            checkCount++;
                        }

                        if(checkCount >= 2)
                        {
                            Debug.Log("게임종료, 미션 최종 성공!!!!");
                            GetComponentInParent<PuzzleMissonListener>().EndPuzzle(true);
                        }
                    }
                    else
                    {
                        // 실패(이탈) 판정
                        Vector2 preWayPoint = (i == 0) ? (Vector2)transform.position : (Vector2)_carmullRomPath.Waypoints[i - 1];
                        Vector2 nextWayPoint = _carmullRomPath.Waypoints[i]; // i가 Count와 같아질 일은 없으므로 안전함

                        // X를 기준으로 현재 Y 위치(예측값) 계산 (선형 보간)
                        float t = (worldMiddlePoint.x - preWayPoint.x) / (nextWayPoint.x - preWayPoint.x);
                        float predictY = Mathf.Lerp(preWayPoint.y, nextWayPoint.y, t);
                        Debug.Log($"Current pos : {worldMiddlePoint}, predict : {predictY}");
                        // 범위를 벗어난 경우 (월드 좌표 Y 기준)
                        if (worldMiddlePoint.y > predictY + (outLineLength / 2f) ||
                            worldMiddlePoint.y < predictY - (outLineLength / 2f))
                        {
                            Debug.Log($"선 이탈! 미션 실패!!!!");
                            isChecked = true; // 더 이상 검사하지 않음
                            GetComponentInParent<PuzzleMissonListener>().EndPuzzle(false);
                        }

                        break; // 현재 선의 X 위치가 속한 구간을 찾았으므로 for문 탈출
                    }
                }
            }
        }
    }
}