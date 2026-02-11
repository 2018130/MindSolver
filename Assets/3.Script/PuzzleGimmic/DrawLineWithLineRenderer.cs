using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineWithLineRenderer : MonoBehaviour
{
    public GameObject brush;

    public LineRenderer currentLineRenderer;

    public Vector2 lastPos;

    private static bool isSpawned = false;

    private void Update()
    {
        Drawing();
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
        GameObject brushInstance = Instantiate(brush);
        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        //because you gotta have 2 points to start a line renderer, 
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);

        currentLineRenderer.SetPosition(0, mousePos);
        currentLineRenderer.SetPosition(1, mousePos);

    }

    void AddAPoint(Vector2 pointPos)
    {
        currentLineRenderer.positionCount++;
        int positionIndex = currentLineRenderer.positionCount - 1;
        currentLineRenderer.SetPosition(positionIndex, pointPos);
    }

    void PointToMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        if (lastPos != mousePos)
        {
            AddAPoint(mousePos);
            lastPos = mousePos;
        }
    }

}
