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

    // 유저간 허용거리
    //private static NetworkVariable<float> fastPosX = new NetworkVariable<float>();
    private NetworkVariable<float> fastPosX = new NetworkVariable<float>();
    [SerializeField]
    private float allowedLengthDelta = 1f;
    [SerializeField]
    private LineRenderer limitLineRenderer;

    private void Awake()
    {
        _carmullRomPath = GetComponent<CatmullRomPath>();

        // 초기에는 안 보이게 설정
        limitLineRenderer.enabled = false;
    }

    public void Initialize(DrawLineWithMesh drawLineWithMesh)
    {
        fastPosX.Value = -50f; 
        limitLineRenderer.positionCount = 4;
        _drawLineWithMesh = drawLineWithMesh;
        isChecked = false;
        checkCount = 0;
    }


    private void Update()
    {
        if(Mathf.Approximately(fastPosX.Value, -100f))
        {
            limitLineRenderer.positionCount = 0;
        }

        if(IsClient)
        {
            limitLineRenderer.enabled = true;
            float topY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
            float bottomY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f)).y;

            Vector3 v0 = new Vector3(fastPosX.Value, topY, -0.9f);
            Vector3 v1 = new Vector3(fastPosX.Value - allowedLengthDelta, topY, -0.9f);
            Vector3 v2 = new Vector3(fastPosX.Value - allowedLengthDelta, bottomY, -0.9f);
            Vector3 v3 = new Vector3(fastPosX.Value, bottomY, -0.9f);
            limitLineRenderer.positionCount = 4;
            limitLineRenderer.SetPosition(0, v0);
            limitLineRenderer.SetPosition(1, v1);
            limitLineRenderer.SetPosition(2, v2);
            limitLineRenderer.SetPosition(3, v3);
        }
    }

    public void CheckDistance(NetworkListEvent<Vector3> networkListEvent)
    {
        if (_drawLineWithMesh == null || isChecked ||
            _drawLineWithMesh.Vertices.Count <= 2)
        {
            return;
        }

        if (_drawLineWithMesh.Vertices.Count % 2 == 0)
        {
            Vector3 localMiddlePoint = (_drawLineWithMesh.Vertices[_drawLineWithMesh.Vertices.Count - 1] +
                                        _drawLineWithMesh.Vertices[_drawLineWithMesh.Vertices.Count - 2]) / 2f;

            Vector3 worldMiddlePoint = _drawLineWithMesh.transform.TransformPoint(localMiddlePoint);

            if(fastPosX.Value < worldMiddlePoint.x)
            {
                if (!limitLineRenderer.enabled)
                    limitLineRenderer.enabled = true;

                fastPosX.Value = worldMiddlePoint.x;
            }

            // 유저간 간격 격차로 인한 실패
            if(fastPosX.Value > worldMiddlePoint.x + allowedLengthDelta)
            {
                fastPosX.Value = -100f;
                isChecked = true;
                GetComponentInParent<PuzzleMissonListener>().EndPuzzle(false);
            }

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

                            Debug.Log($"{gameObject} 미션 부분 성공!");
                            checkCount++;
                        }

                        if(checkCount >= 2)
                        {
                            fastPosX.Value = -100f;
                            Debug.Log("게임종료, 미션 최종 성공!!!!");
                            GetComponentInParent<PuzzleMissonListener>().EndPuzzle(true);
                        }
                    }
                    else
                    {
                        Vector2 preWayPoint = (i == 0) ? (Vector2)transform.position : (Vector2)_carmullRomPath.Waypoints[i - 1];
                        Vector2 nextWayPoint = _carmullRomPath.Waypoints[i]; // i가 Count와 같아질 일은 없으므로 안전함

                        float t = (worldMiddlePoint.x - preWayPoint.x) / (nextWayPoint.x - preWayPoint.x);
                        float predictY = Mathf.Lerp(preWayPoint.y, nextWayPoint.y, t);

                        if (worldMiddlePoint.y > predictY + (outLineLength / 2f) ||
                            worldMiddlePoint.y < predictY - (outLineLength / 2f))
                        {
                            fastPosX.Value = -100f;
                            Debug.Log($"선 이탈! 미션 실패!!!!");
                            isChecked = true;
                            GetComponentInParent<PuzzleMissonListener>().EndPuzzle(false);
                        }

                        break;
                    }
                }
            }
        }
    }
}