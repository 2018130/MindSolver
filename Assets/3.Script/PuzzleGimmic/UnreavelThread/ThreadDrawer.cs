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

    private NetworkList<NetworkObjectReference> _netRopeNodes_ownedRed;
    private NetworkList<NetworkObjectReference> _netRopeNodes_ownedBlue;

    [Header("Setting"), Space(10f)]

    [SerializeField]
    private Joint hingePrefab;

    [SerializeField]
    private int hingeCount = 50;

    [SerializeField]
    private float distancePerHinge = 0.08f;

    [SerializeField]
    private int angleCount = 5;
    [SerializeField]
    private float minRadius = 2.0f;
    [SerializeField]
    private float maxRadius = 5.0f;
    [SerializeField]
    private float forwardStep = 0.5f;

    [SerializeField]
    private GameObject handlePrefab;
    [SerializeField]
    private Rigidbody owned_rb;

    [SerializeField]
    private float cycleCheckDelay = 0.3f;

    private void Awake()
    {
        _netRopeNodes_ownedRed = new NetworkList<NetworkObjectReference>();
        _netRopeNodes_ownedBlue = new NetworkList<NetworkObjectReference>();

        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        InitThread();

        if (IsServer)
        {
            foreach (var hinge in _netRopeNodes_ownedBlue)
            {
                hinge.TryGet(out NetworkObject netObj);
                netObj.GetComponent<Rigidbody>().isKinematic = true;
            }

            StartCoroutine(CheckCycle());
        }
        else
        {
            foreach (var hinge in _netRopeNodes_ownedRed)
            {
                hinge.TryGet(out NetworkObject netObj);
                netObj.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    private void InitThread()
    {
        float startAngle = 270f;
        float angleStep = 360f / angleCount;
        float radius = UnityEngine.Random.Range(minRadius, maxRadius);
        Vector3 spawnOffset = new Vector3(forwardStep, radius);

        if (IsServer)
        {
            for (int i = 0; i < hingeCount / 2; i++)
            {
                if (i != 0)
                {
                    float angle = (angleStep * (i % angleCount) + startAngle) * Mathf.Deg2Rad;

                    Vector3 spawnPos = spawnOffset;

                    spawnPos.x += Mathf.Cos(angle) * radius;
                    spawnPos.y += Mathf.Sin(angle) * radius;

                    Joint newNode = Instantiate(hingePrefab, spawnPos, Quaternion.identity, transform);
                    NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                    netObj.SpawnWithOwnership(NetworkPlayer.ServerPlayerId);

                    _netRopeNodes_ownedRed.Add(netObj);

                    if (_netRopeNodes_ownedRed[i].TryGet(out NetworkObject networkObject) &&
                        _netRopeNodes_ownedRed[i - 1].TryGet(out NetworkObject preNetworkObject))
                    {
                        networkObject.GetComponent<Joint>().connectedBody =
                            preNetworkObject.GetComponent<Rigidbody>();
                    }

                    if (i == hingeCount / 2 - 1)
                    {
                        if (_netRopeNodes_ownedRed[i].TryGet(out networkObject))
                        {
                            networkObject.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                }
                else
                {
                    owned_rb = Instantiate(handlePrefab).GetComponent<Rigidbody>();
                    owned_rb.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkPlayer.ServerPlayerId);

                    Joint newNode = Instantiate(hingePrefab, owned_rb.transform.position, Quaternion.identity, transform);
                    NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                    netObj.SpawnWithOwnership(NetworkPlayer.ServerPlayerId);

                    owned_rb.transform.position = newNode.transform.position;

                    _netRopeNodes_ownedRed.Add(netObj);

                    if (_netRopeNodes_ownedRed[i].TryGet(out NetworkObject networkObject))
                    {
                        networkObject.GetComponent<Joint>().connectedBody = owned_rb;
                    }
                }
            }


            // 클라 소유
            for (int i = 0; i < hingeCount / 2; i++)
            {
                if (i != hingeCount / 2 - 1)
                {
                    float angle = (angleStep * (i % angleCount) + startAngle) * Mathf.Deg2Rad;

                    Vector3 spawnPos = spawnOffset;

                    spawnPos.x += Mathf.Cos(angle) * radius;
                    spawnPos.y += Mathf.Sin(angle) * radius;

                    Joint newNode = Instantiate(hingePrefab, spawnPos, Quaternion.identity, transform);
                    NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                    netObj.SpawnWithOwnership(NetworkPlayer.ClientPlayerId);

                    _netRopeNodes_ownedBlue.Add(netObj);
                }
                else
                {
                    Rigidbody rb = Instantiate(handlePrefab, Vector3.right * 3f, Quaternion.identity).GetComponent<Rigidbody>();
                    rb.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkPlayer.ClientPlayerId);

                    Joint newNode = Instantiate(hingePrefab, rb.transform.position, Quaternion.identity, transform);
                    NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                    netObj.SpawnWithOwnership(NetworkPlayer.ClientPlayerId);

                    _netRopeNodes_ownedBlue.Add(netObj);
                }
            }
        }
        // 클라이언트
        else
        {
            foreach(var handle in FindObjectsByType<ThreadHandle>(FindObjectsSortMode.None))
            {
                if(handle.GetComponent<NetworkObject>().IsOwner)
                {
                    owned_rb = handle.GetComponent<Rigidbody>();
                }
            }
            for (int i = 0; i < hingeCount / 2; i++)
            {
                if (i != hingeCount / 2 - 1)
                {
                    float angle = (angleStep * (i % angleCount) + startAngle) * Mathf.Deg2Rad;

                    Vector3 spawnPos = Vector3.up;
                    if(_netRopeNodes_ownedRed[_netRopeNodes_ownedRed.Count - 1].TryGet(out NetworkObject lastRedNode))
                    {
                        spawnPos = lastRedNode.transform.position;
                    }

                    spawnPos.x += Mathf.Cos(angle) * radius;
                    spawnPos.y += Mathf.Sin(angle) * radius;

                    if (i != 0)
                    {
                        if (_netRopeNodes_ownedBlue[i - 1].TryGet(out NetworkObject networkObject) &&
                            _netRopeNodes_ownedBlue[i].TryGet(out NetworkObject postNetworkObject))
                        {
                            networkObject.GetComponent<Joint>().connectedBody =
                                postNetworkObject.GetComponent<Rigidbody>();
                        }
                    }
                    else
                    {
                        if (_netRopeNodes_ownedBlue[i].TryGet(out NetworkObject net))
                        {
                            net.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                }
                else
                {
                    if (_netRopeNodes_ownedBlue[i].TryGet(out NetworkObject networkObject))
                    {
                        networkObject.GetComponent<Joint>().connectedBody = owned_rb;
                    }

                    if (_netRopeNodes_ownedBlue[i - 1].TryGet(out NetworkObject net) &&
                           _netRopeNodes_ownedBlue[i].TryGet(out NetworkObject postNetworkObject))
                    {
                        net.GetComponent<Joint>().connectedBody =
                            postNetworkObject.GetComponent<Rigidbody>();
                    }
                }
            }
        }
    }

    private void Update()
    {
        DrawCircle();
    }

    private void DrawCircle()
    {
        if (_netRopeNodes_ownedRed == null ||
            _netRopeNodes_ownedBlue == null ||
            _netRopeNodes_ownedRed.Count == 0 ||
            _netRopeNodes_ownedRed.Count == 0)
            return;

        lineRenderer.positionCount = _netRopeNodes_ownedRed.Count + _netRopeNodes_ownedBlue.Count;

        for (int i = 0; i < _netRopeNodes_ownedRed.Count; i++)
        {
            _netRopeNodes_ownedRed[i].TryGet(out NetworkObject networkObject);
            Vector3 position = networkObject.transform.position;
            lineRenderer.SetPosition(i, position);
        }

        for (int i = 0; i < _netRopeNodes_ownedBlue.Count; i++)
        {
            _netRopeNodes_ownedBlue[i].TryGet(out NetworkObject networkObject);
            Vector3 position = networkObject.transform.position;
            lineRenderer.SetPosition(_netRopeNodes_ownedRed.Count + i, position);
        }
    }

    private IEnumerator CheckCycle()
    {
        while(HasCycle(_netRopeNodes_ownedRed) || HasCycle(_netRopeNodes_ownedBlue))
        {
            yield return new WaitForSeconds(cycleCheckDelay);
        }

        Debug.Log($"End game");
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
}
