using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CatmullRomPath : NetworkBehaviour
{
    [SerializeField]
    private Vector2 widthRange;
    [SerializeField]
    private Vector2 heightRange;

    private NetworkList<Vector2> waypoints = new NetworkList<Vector2>();
    public NetworkList<Vector2> Waypoints => waypoints;

    [SerializeField]
    private int waypointCount = 6;

    [SerializeField]
    private int resolution = 10;

    private LineRenderer lineRenderer;

    // 소유권 정하기 용
    private static bool hasServerPath = false;
    private static int scriptCount = 0;

    private NetworkVariable<bool> isServerPath = new NetworkVariable<bool>();
    public NetworkVariable<bool> IsServerPath => isServerPath;

    private bool isGamePlaying = false;
    private NetworkVariable<NetworkObjectReference> drawLineWithMesh = new NetworkVariable<NetworkObjectReference>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        if (IsServer)
        {
            GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle += StartGame;
            GetComponentInParent<PuzzleMissonListener>().OnEndPuzzle += EndGame;
        }
    }

    public void Initialize(DrawLineWithMesh drawLineWithMesh)
    {
        this.drawLineWithMesh.Value = drawLineWithMesh.GetComponent<NetworkObject>();
        Debug.Log($"init to {drawLineWithMesh.IsOwner}");
    }

    private void StartGame(MultiMissionType multiMissionType)
    {
        if(multiMissionType == MultiMissionType.DrawLine)
        {
            CreateWaypoint(waypointCount);

            if (waypoints == null || waypoints.Count < 2)
                return;

            drawLineWithMesh.Value.TryGet(out NetworkObject networkObject);
            networkObject.GetComponent<DrawLineWithMesh>().InitGame();

            DrawCatmullRom();
            StartGame_ClientRpc();
        }
    }

    [ClientRpc]
    private void StartGame_ClientRpc()
    {
        isGamePlaying = true;
    }

    private void EndGame(bool isClear, MultiMissionType multiMissionType)
    {
        if(multiMissionType == MultiMissionType.DrawLine)
        {
            waypoints.Clear();
            EndGame_ClientRpc();
        }
    }

    [ClientRpc]
    private void EndGame_ClientRpc()
    {
        Debug.Log($"End game path, {gameObject} owned {IsOwner}");
        lineRenderer.positionCount = 0;
        isGamePlaying = false;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        waypoints = null;
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Count < 2 || !isGamePlaying)
            return;

        DrawCatmullRom();
    }

    private void DrawCatmullRom()
    {
        if (drawLineWithMesh.Value.TryGet(out NetworkObject networkObject) &&
            networkObject.IsOwner)
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        else
        {
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
        }

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector2 p0 = waypoints[ClampIndex(i - 1)];
            Vector2 p1 = waypoints[ClampIndex(i)];
            Vector2 p2 = waypoints[ClampIndex(i + 1)];
            Vector2 p3 = waypoints[ClampIndex(i + 2)];

            for (int j = 0; j < resolution; j++)
            {
                float t = (float)j / resolution;
                points.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
            }
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private int ClampIndex(int index)
    {
        if (index < 0)
            return 0;
        if (index >= waypoints.Count)
            return waypoints.Count - 1;

        return index;
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    private void CreateWaypoint(int wayCount)
    {
        waypoints.Add(transform.position);

        for (int i = 1; i < wayCount; i++)
        {
            float width = waypoints[i - 1].x + UnityEngine.Random.Range(widthRange.x, widthRange.y);
            float height = waypoints[0].y + UnityEngine.Random.Range(heightRange.x, heightRange.y);

            waypoints.Add(new Vector2(width, height));
        }
    }
}
