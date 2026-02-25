using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ThreadDrawer : NetworkBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    private NetworkList<NetworkObjectReference> _netRopeNodes_ownedRed = new NetworkList<NetworkObjectReference>();
    private NetworkList<NetworkObjectReference> _netRopeNodes_ownedBlue = new NetworkList<NetworkObjectReference>();

    [Header("Setting"), Space(10f)]

    [SerializeField]
    private Joint hingePrefab;

    [SerializeField]
    private int hingeCount = 30;

    [SerializeField]
    private int angleCount = 10;
    [SerializeField]
    private float minRadius = 0.5f;
    [SerializeField]
    private float maxRadius = 0.5f;

    [SerializeField]
    private GameObject handlePrefab;
    [SerializeField]
    private Rigidbody owned_handle;

    [SerializeField]
    private float cycleCheckDelay = 0.3f;

    [Header("_ETC"), Space(10f)]

    [SerializeField]
    private GameObject background;

    [SerializeField]
    private Vector3 redHandleSpawnPoint = new Vector3(-3, 0, 0);
    [SerializeField]
    private Vector3 blueHandleSpawnPoint = new Vector3(3, 0, 0);

    private bool isGameStarting = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsServer)
        {
            SpawnHinge();
            ConnectJoint(_netRopeNodes_ownedRed);
            GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle += StartGame;
        }
        else
        {
            ConnectJoint(_netRopeNodes_ownedBlue, true);
        }
    }

    public void StartGame(MultiMissionType multiMissionType)
    {
        Debug.Log("1111");
        if(multiMissionType == MultiMissionType.ThreadDrawer)
        {
            Debug.Log("2222");
            StartGame_ClientRpc();
            SetHingePosition();
            StartCoroutine(CheckCycle());
        }
    }

    /// <summary>
    /// 서버에서 호출될 함수, 힌지를 스폰함
    /// </summary>
    private void SpawnHinge()
    {
        if (!IsServer)
            return;

        for(int i = 0; i < hingeCount / 2; i++)
        {
            Joint spawn = Instantiate(hingePrefab);
            NetworkObject networkObj = spawn.GetComponent<NetworkObject>();
            networkObj.Spawn();

            _netRopeNodes_ownedRed.Add(networkObj);
        }

        for(int i = 0; i < hingeCount/ 2; i++)
        {
            Joint spawn = Instantiate(hingePrefab);
            NetworkObject networkObj = spawn.GetComponent<NetworkObject>();
            networkObj.SpawnWithOwnership(NetworkPlayer.ClientPlayerId);

            _netRopeNodes_ownedBlue.Add(networkObj);
        }
    }

    private void SetHingePosition()
    {
        int blueCount = _netRopeNodes_ownedBlue.Count;
        if (blueCount > 0)
        {
            float blueRadius = UnityEngine.Random.Range(minRadius, maxRadius);
            Vector3[] bluePositions = CalculateNodePositions(blueRadius, Vector3.zero, blueCount);

            for (int i = 0; i < blueCount; i++)
            {
                if (_netRopeNodes_ownedBlue[i].TryGet(out NetworkObject obj))
                {
                    obj.transform.position = bluePositions[i];
                }
            }
        }

        int redCount = _netRopeNodes_ownedRed.Count;
        if (redCount > 0)
        {
            float redRadius = UnityEngine.Random.Range(minRadius, maxRadius);
            Vector3[] redPositions = CalculateNodePositions(redRadius, Vector3.zero, redCount);

            for (int i = 0; i < redCount; i++)
            {
                if (_netRopeNodes_ownedRed[i].TryGet(out NetworkObject obj))
                {
                    obj.transform.position = redPositions[i];
                }
            }
        }
    }

    private Vector3[] CalculateNodePositions(float radius, Vector3 spawnOffset, int nodeCount)
    {
        Vector3[] positions = new Vector3[nodeCount];

        if (nodeCount == 0) return positions;

        float startAngle = 270f;
        float angleStep = 360f / nodeCount;

        for (int i = 0; i < nodeCount; i++)
        {
            float angle = (angleStep * i + startAngle) * Mathf.Deg2Rad;

            Vector3 pos = spawnOffset;
            pos.x += Mathf.Cos(angle) * radius;
            pos.y += Mathf.Sin(angle) * radius;

            positions[i] = pos;
        }

        return positions;
    }

    private void InitHandle(ulong clientId)
    {
        if (IsServer)
        {
            owned_handle = Instantiate(handlePrefab, redHandleSpawnPoint, Quaternion.identity).GetComponent<Rigidbody>();
            owned_handle.GetComponent<NetworkObject>().Spawn();

            NetworkObject clientHandle = Instantiate(handlePrefab, blueHandleSpawnPoint, Quaternion.identity).GetComponent<NetworkObject>();
            clientHandle.SpawnWithOwnership(clientId);
        }
        else
        {
            foreach(var handle in FindObjectsByType<ThreadHandle>(FindObjectsSortMode.None))
            {
                if(handle.GetComponent<NetworkObject>() &&
                    handle.IsOwner)
                {
                    owned_handle = handle.GetComponent<Rigidbody>();
                }
            }
        }
    }

    private void ConnectJoint(NetworkList<NetworkObjectReference> networkObjects, bool isReverse = false)
    {
        if(owned_handle == null)
        {
            InitHandle(NetworkPlayer.ClientPlayerId);
        }

        for(int i = 0; i < networkObjects.Count; i++)
        {
            int idx = !isReverse ? i : networkObjects.Count - i - 1;
            int jointIdx = isReverse ? idx + 1 : idx - 1;

            if (jointIdx >= networkObjects.Count ||
                jointIdx < 0)
                continue;

            if(networkObjects[idx].TryGet(out NetworkObject networkObject) &&
                networkObjects[jointIdx].TryGet(out NetworkObject jointObject))
            {
                networkObject.GetComponent<Joint>().connectedBody =
                    jointObject.GetComponent<Rigidbody>();
            }
        }

        int lastIdx = !isReverse ? 0 : networkObjects.Count - 1;
        if (networkObjects[lastIdx].TryGet(out NetworkObject lastObject))
        {
            lastObject.GetComponent<Joint>().connectedBody =
                owned_handle;
        }
    }

    private void Update()
    {
        DrawHinge();
    }

    private void DrawHinge()
    {
        if (!isGameStarting)
            return;

        lineRenderer.positionCount = _netRopeNodes_ownedBlue.Count + _netRopeNodes_ownedRed.Count;

        Vector3 previousPos = Vector3.zero;
        bool hasPrevious = false;

        // --- Red 그룹 ---
        for (int i = 0; i < _netRopeNodes_ownedRed.Count; i++)
        {
            if (_netRopeNodes_ownedRed[i].TryGet(out NetworkObject networkObject))
            {
                Vector3 currentPos = networkObject.transform.position;
                lineRenderer.SetPosition(i, currentPos);

                // [디버그] 노드 위치에 빨간색 짧은 기둥 표시
                Debug.DrawRay(currentPos, Vector3.up * 0.5f, Color.red);

                // [디버그] 이전 노드와 선 연결
                if (hasPrevious)
                {
                    Debug.DrawLine(previousPos, currentPos, Color.red);
                }

                previousPos = currentPos;
                hasPrevious = true;
            }
        }

        // --- Blue 그룹 ---
        for (int i = 0; i < _netRopeNodes_ownedBlue.Count; i++)
        {
            if (_netRopeNodes_ownedBlue[i].TryGet(out NetworkObject networkObject))
            {
                Vector3 currentPos = networkObject.transform.position;
                lineRenderer.SetPosition(_netRopeNodes_ownedRed.Count + i, currentPos);

                // [디버그] 노드 위치에 파란색 짧은 기둥 표시
                Debug.DrawRay(currentPos, Vector3.up * 0.5f, Color.blue);

                // [디버그] 이전 노드와 선 연결
                if (hasPrevious)
                {
                    // Red의 마지막 노드와 Blue의 첫 노드가 만나는 구간은 노란색으로 표시
                    Color lineColor = (i == 0) ? Color.yellow : Color.cyan;
                    Debug.DrawLine(previousPos, currentPos, lineColor);
                }

                previousPos = currentPos;
                hasPrevious = true;
            }
        }
    }

    [ClientRpc]
    private void StartGame_ClientRpc()
    {
        isGameStarting = true;
        background.SetActive(true);

        if(IsServer)
        {
            owned_handle.transform.position = redHandleSpawnPoint;
        }
        else
        {
            owned_handle.transform.position = blueHandleSpawnPoint;
        }
    }

    [ClientRpc]
    private void EndGame_ClientRpc()
    {
        Debug.Log($"End game");
        background.SetActive(false);
        lineRenderer.positionCount = 0;
        isGameStarting = false;
    }

    #region Check
    private IEnumerator CheckCycle()
    {
        while (HasCycle(_netRopeNodes_ownedRed) || HasCycle(_netRopeNodes_ownedBlue))
        {
            yield return new WaitForSeconds(cycleCheckDelay);
        }

        yield return new WaitForSeconds(3f);

        EndGame_ClientRpc();
    }

    private List<Vector3> GetNodePositions(NetworkList<NetworkObjectReference> networkList)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var nodeRef in networkList)
        {
            if (nodeRef.TryGet(out NetworkObject netObj) && netObj != null)
            {
                positions.Add(netObj.transform.position);
            }
        }
        return positions;
    }

    public bool HasCycle(NetworkList<NetworkObjectReference> networkList)
    {
        if (networkList.Count < 3) return false;

        List<Vector3> positions = GetNodePositions(networkList);

        float totalAngle = 0f;

        for (int i = 0; i < positions.Count - 2; i++)
        {
            Vector3 p1 = positions[i];
            Vector3 p2 = positions[i + 1];
            Vector3 p3 = positions[i + 2];

            Vector3 dir1 = (p2 - p1).normalized;
            Vector3 dir2 = (p3 - p2).normalized;

            float angle = Vector3.SignedAngle(dir1, dir2, Vector3.right);

            totalAngle += angle;
        }

        if (Mathf.Abs(totalAngle) > 210f)
        {
            Debug.Log($"사이클 감지됨! 총 회전각: {totalAngle}");
            return true;
        }

        return false;
    }
    #endregion
}
