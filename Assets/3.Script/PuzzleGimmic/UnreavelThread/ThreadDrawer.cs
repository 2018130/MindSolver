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
    private float distancePerHinge = 0.8f;

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

    // 게임 적용 값
    private NetworkVariable<bool> gamePlaying = new NetworkVariable<bool>(false);

    [Header("_ETC"), Space(10f)]

    [SerializeField]
    private GameObject background;

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
            ConnectJoint(_netRopeNodes_ownedBlue, true);
        }
    }

    public void StartGame()
    {
        gamePlaying.Value = true;
        StartCoroutine(CheckCycle());
        SetHingePosition();
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
        Vector3[] posVector = CalculateNodePositions(UnityEngine.Random.Range(minRadius, maxRadius), Vector3.zero);
        for(int i = 0; i < _netRopeNodes_ownedBlue.Count; i++)
        {
            if(_netRopeNodes_ownedBlue[i].TryGet(out NetworkObject obj))
            {
                obj.transform.position = posVector[i];
            }
        }

        posVector = CalculateNodePositions(UnityEngine.Random.Range(minRadius, maxRadius), Vector3.zero);
        for (int i = 0; i < _netRopeNodes_ownedRed.Count; i++)
        {
            if (_netRopeNodes_ownedRed[i].TryGet(out NetworkObject obj))
            {
                Debug.Log(obj.transform.position + " -> " + posVector[i]);
                obj.transform.position = posVector[i];
            }
        }
    }

    private Vector3[] CalculateNodePositions(float radius, Vector3 spawnOffset)
    {
        int nodeCount = hingeCount / 2;
        Vector3[] positions = new Vector3[nodeCount];
        float startAngle = 270f;
        float angleStep = 360f / angleCount;

        for (int i = 0; i < nodeCount; i++)
        {
            float angle = (angleStep * (i % angleCount) + startAngle) * Mathf.Deg2Rad;
            Vector3 pos = spawnOffset;
            pos.x += Mathf.Cos(angle) * radius;
            pos.y += Mathf.Sin(angle) * radius;
            positions[i] = pos;
        }
        return positions;
    }

    private void SpawnHandle()
    {
        owned_handle = Instantiate(handlePrefab).GetComponent<Rigidbody>();

        if (IsServer)
        {
            owned_handle.GetComponent<NetworkObject>().Spawn();

            Rigidbody clientHandle = Instantiate(handlePrefab).GetComponent<Rigidbody>();
            clientHandle.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkPlayer.ClientPlayerId);
        }
        else
        {
            if(owned_handle == null)
            {
                foreach(var handle in FindObjectsByType<ThreadHandle>(FindObjectsSortMode.None))
                {
                    if(handle.TryGetComponent(out NetworkObject networkObject) &&
                        networkObject.IsOwner)
                    {
                        owned_handle = networkObject.GetComponent<Rigidbody>();
                    }
                }
            }
        }
    }

    private void ConnectJoint(NetworkList<NetworkObjectReference> networkObjects, bool isReverse = false)
    {
        if(owned_handle == null)
        {
            SpawnHandle();
        }

        for(int i = 0; i < networkObjects.Count; i++)
        {
            int idx = !isReverse ? i : networkObjects.Count - i - 1;
            int jointIdx = isReverse ? i + 1 : i - 1;

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
        if (!gamePlaying.Value)
            return;

        if(!background.activeSelf)
        {
            background.SetActive(true);
        }

        lineRenderer.positionCount = _netRopeNodes_ownedBlue.Count + _netRopeNodes_ownedRed.Count;

        for(int i = 0; i < _netRopeNodes_ownedRed.Count; i++)
        {
            if(_netRopeNodes_ownedRed[i].TryGet(out NetworkObject networkObject))
            {
                lineRenderer.SetPosition(i, networkObject.transform.position);
            }
        }

        for (int i = 0; i < _netRopeNodes_ownedBlue.Count; i++)
        {
            if (_netRopeNodes_ownedBlue[i].TryGet(out NetworkObject networkObject))
            {
                lineRenderer.SetPosition(_netRopeNodes_ownedRed.Count + i, networkObject.transform.position);
            }
        }
    }

    #region Check
    private IEnumerator CheckCycle()
    {
        while (HasCycle(_netRopeNodes_ownedRed) || HasCycle(_netRopeNodes_ownedBlue))
        {
            yield return new WaitForSeconds(cycleCheckDelay);
        }

        gamePlaying.Value = false;
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

        if (Mathf.Abs(totalAngle) > 270f)
        {
            Debug.Log($"사이클 감지됨! 총 회전각: {totalAngle}");
            return true;
        }

        return false;
    }
    #endregion
}
