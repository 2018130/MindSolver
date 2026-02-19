using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DrawLineWithLineRenderer : NetworkBehaviour
{
    public GameObject brush;

    public LineRenderer currentLineRenderer;

    public Vector2 lastPos;

    private bool isSpawned = false;

    private NetworkList<Vector2> positions = new NetworkList<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Update()
    {
        if (!IsOwner)
            return;
        Drawing();
    }
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
            positions.OnListChanged += OnPositionsChanged;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            positions.OnListChanged -= OnPositionsChanged;
    }
    private void OnPositionsChanged(NetworkListEvent<Vector2> changeEvent)
    {
            if (!isSpawned)
            {
                isSpawned = true;
                GameObject brushInstance = Instantiate(brush);
                currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

                currentLineRenderer.SetPosition(0, changeEvent.Value);
                currentLineRenderer.SetPosition(1, changeEvent.Value);
                Debug.Log($"Spawn lineRenderer");
            }
            else
            {
                currentLineRenderer.positionCount++;
                currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, changeEvent.Value);
                Debug.Log($"Set position to {changeEvent.Value}");
            }
    }

    void Drawing()
    {
        if (InputManager.Singleton.LeftButtonClicked)
        {
            if(!isSpawned)
            {
                isSpawned = true;
                CreateBrush();
            }
            else
            {
                PointToMousePos();
            }
        }
    }

    void CreateBrush()
    {
        Debug.Log($"Crate brush");
        GameObject brushInstance = Instantiate(brush);
        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        //because you gotta have 2 points to start a line renderer, 
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);

        currentLineRenderer.SetPosition(0, mousePos);
        currentLineRenderer.SetPosition(1, mousePos);

        positions.Add(mousePos);
        positions.Add(mousePos);
    }

    void AddAPoint(Vector2 pointPos)
    {
        currentLineRenderer.positionCount++;
        int positionIndex = currentLineRenderer.positionCount - 1;

        currentLineRenderer.SetPosition(positionIndex, pointPos);
        positions.Add(pointPos);
    }

    void PointToMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        if (lastPos != mousePos)
        {
            AddAPoint(mousePos);
            lastPos = mousePos;
            positions.Add(mousePos);
        }
    }

}
