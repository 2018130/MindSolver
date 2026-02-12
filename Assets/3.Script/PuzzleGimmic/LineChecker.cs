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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        _carmullRomPath = GetComponent<CatmullRomPath>();
        
        foreach (var meshDrawer in FindObjectsByType<DrawLineWithMesh>(FindObjectsSortMode.None))
        {
            if (_carmullRomPath.IsServerPath.Value && meshDrawer.IsServer)
            {
                _drawLineWithMesh = meshDrawer;
            }
            else
            {
                _drawLineWithMesh = meshDrawer;
            }
        }

        _drawLineWithMesh.Vertices.OnListChanged += CheckDistance;
    }

    private void CheckDistance(NetworkListEvent<Vector3> networkListEvent)
    {
        if (_drawLineWithMesh.Vertices.Count % 2 == 0)
        {
            // y값 비교
            Vector2 middlePoint = (_drawLineWithMesh.Vertices[_drawLineWithMesh.Vertices.Count - 1] +
                _drawLineWithMesh.Vertices[_drawLineWithMesh.Vertices.Count - 2]) / 2;

            for (int i = 0; i < _carmullRomPath.Waypoints.Count; i++)
            {
                // x값 초과 직후
                if (_carmullRomPath.Waypoints[i].x > middlePoint.x)
                {
                    Vector2 preWayPoint = i == 0 ? transform.position : _carmullRomPath.Waypoints[i - 1];
                    Vector2 nextWayPoint = i == _carmullRomPath.Waypoints.Count ?
                        _carmullRomPath.Waypoints[_carmullRomPath.Waypoints.Count - 1] : _carmullRomPath.Waypoints[i];

                    float t = (middlePoint.x - preWayPoint.x) / (nextWayPoint.x - preWayPoint.x);
                    float predictY = Mathf.Lerp(preWayPoint.y, nextWayPoint.y, t);

                    // 범위를 벗어난 경우
                    if (predictY + outLineLength / 2 < middlePoint.y ||
                        predictY - outLineLength / 2 > middlePoint.y)
                    {
                        Debug.Log($"게임종료!!!!");
                    }

                    break;
                }
            }
        }
    }

}
