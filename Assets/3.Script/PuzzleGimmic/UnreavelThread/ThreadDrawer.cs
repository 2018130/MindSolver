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
    
    private NetworkList<NetworkObjectReference> _netRopeNodes;

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
    private Rigidbody red_rb;
    [SerializeField]
    private Rigidbody blue_rb;

    private void Awake()
    {
        _netRopeNodes = new NetworkList<NetworkObjectReference>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        float startAngle = 270f;
        float angleStep = 360f / angleCount;
        float radius = UnityEngine.Random.Range(minRadius, maxRadius);
        Vector3 spawnOffset = new Vector3(forwardStep, radius) + red_rb.transform.position;

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
                netObj.Spawn();

                _netRopeNodes.Add(netObj);

                if(_netRopeNodes[i].TryGet(out NetworkObject networkObject) &&
                    _netRopeNodes[i - 1].TryGet(out NetworkObject preNetworkObject))
                {
                    networkObject.GetComponent<Joint>().connectedBody =
                        preNetworkObject.GetComponent<Rigidbody>();
                }

                if (i == hingeCount / 2 - 1)
                {
                    if (_netRopeNodes[i].TryGet(out networkObject))
                    {
                        networkObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
            }
            else
            {
                Joint newNode = Instantiate(hingePrefab, red_rb.transform.position, Quaternion.identity, transform);
                NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                netObj.Spawn();

                _netRopeNodes.Add(netObj);

                if(_netRopeNodes[i].TryGet(out NetworkObject networkObject))
                {
                    networkObject.GetComponent<Joint>().connectedBody = red_rb;
                }
            }
        }

        for (int i = hingeCount / 2; i < hingeCount; i++)
        {
            if (i == hingeCount - 1)
            {
                Joint newNode = Instantiate(hingePrefab, red_rb.transform.position, Quaternion.identity, transform);
                NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                netObj.Spawn();

                _netRopeNodes.Add(netObj);

                if(_netRopeNodes[i].TryGet(out NetworkObject networkObject) &&
                    _netRopeNodes[i - 1].TryGet(out NetworkObject preNetworkObject))
                {
                    networkObject.GetComponent<Joint>().connectedBody = blue_rb;
                    preNetworkObject.GetComponent<Joint>().connectedBody = networkObject.GetComponent<Rigidbody>();
                }
            }
            else
            {
                float angle = (angleStep * (i % angleCount) + startAngle) * Mathf.Deg2Rad;

                Vector3 spawnPos = spawnOffset;

                spawnPos.x += Mathf.Cos(angle) * radius;
                spawnPos.y += Mathf.Sin(angle) * radius;

                Joint newNode = Instantiate(hingePrefab, red_rb.transform.position, Quaternion.identity, transform);
                NetworkObject netObj = newNode.GetComponent<NetworkObject>();
                netObj.Spawn();

                _netRopeNodes.Add(netObj);

                if (_netRopeNodes[i].TryGet(out NetworkObject networkObject) &&
                    _netRopeNodes[i - 1].TryGet(out NetworkObject preNetworkObject))
                {
                    preNetworkObject.GetComponent<Joint>().connectedBody = networkObject.GetComponent<Rigidbody>();
                }

                if (i == hingeCount / 2)
                {
                    if (_netRopeNodes[i].TryGet(out networkObject))
                    {
                        networkObject.GetComponent<Rigidbody>().isKinematic = true;
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
        if (_netRopeNodes == null ||
            _netRopeNodes.Count == 0)
            return;

        lineRenderer.positionCount = _netRopeNodes.Count;

        for (int i = 0; i < _netRopeNodes.Count; i++)
        {
            _netRopeNodes[i].TryGet(out NetworkObject networkObject);
            Vector3 position = networkObject.transform.position;
            lineRenderer.SetPosition(i, position);
        }
    }

}
