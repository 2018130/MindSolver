using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Right,
    Down,
    Left,
}

public class Pipe : MonoBehaviour
{
    private const int MAX_ROTATE_INDEX = 3;
    private int rotateDir = 0;
    public int RotateDir => rotateDir;

    [SerializeField]
    private LayerMask pipeLayer;

    [SerializeField]
    private List<Direction> holeDirections;

    [SerializeField]
    private bool isStartTile = false;

    // 아래 collider의 isTrigger여부로 물길이 열려있는지 판단, true : 열림, false : 닫힘
    private Collider2D _pipeCollider;

    private bool isChecked = false;

    private void Awake()
    {
        _pipeCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if(isStartTile)
        {
            SetFlowEnabled(true);

            // 인접한 타일의 타일 흐름 정보 갱신
            for (int i = 0; i < holeDirections.Count; i++)
            {
                CheckNearlyPipeAndSetFlowEnabled((int)holeDirections[i]);
            }
        }
    }

    /// <summary>
    /// 파이프를 시계방향으로 90도 회전시킴
    /// </summary>
    public void RotateCW()
    {
        rotateDir++;
        if (rotateDir > MAX_ROTATE_INDEX)
            rotateDir = 0;

        Rotate(rotateDir);

        foreach(var pipe in FindObjectsByType<Pipe>(FindObjectsSortMode.None))
        {
            pipe.isChecked = false;
        }

        // 인접한 타일의 타일 흐름 정보 갱신
        bool isAnyPipeFlowed = false;
        for (int i = 0; i < holeDirections.Count; i++)
        {
            if(CheckNearlyPipeAndSetFlowEnabled((int)holeDirections[i]))
            {
                isAnyPipeFlowed = true;
            }
        }

        if(isAnyPipeFlowed)
        {
            SetFlowEnabled(true);
        }
        else
        {
            SetFlowEnabled(false);
        }
    }

    private void Rotate(int dir)
    {
        if (dir > MAX_ROTATE_INDEX)
            return;

        for(int i = 0; i < holeDirections.Count; i++)
        {
            int holeDir = (int)holeDirections[i] + 1;
            if (holeDir > 3)
                holeDir = 0;

            holeDirections[i] = (Direction)holeDir;
        }


        transform.localEulerAngles = new Vector3(0, 0, -90 * dir);
    }

    private bool CheckNearlyPipeAndSetFlowEnabled(int dir)
    {
        int[] dx = new int[4] { 0, 1, 0, -1 };
        int[] dy = new int[4] { 1, 0, -1, 0 };
        isChecked = true;

        bool isFlowed = false;
        Vector2 dirVector = new Vector2(dx[dir], dy[dir]);
        float length = _pipeCollider.bounds.extents.x * 1.2f;
        RaycastHit2D hit = Physics2D.Raycast(transform.localPosition, dirVector, length, pipeLayer);
        Debug.DrawRay(transform.localPosition, dirVector * length, Color.red, 1f);
        Debug.Log($"{gameObject} -{dir}-> {hit.collider}");

        if (hit.collider != null)
        {
            Pipe nearlyPipe = hit.collider.GetComponent<Pipe>();
            switch ((Direction)dir)
            {
                case Direction.Up:
                    if(nearlyPipe.CheckHole(Direction.Down))
                    {
                        nearlyPipe.SetFlowEnabled(true);
                        // 인접한 타일의 타일 흐름 정보 갱신
                        for (int i = 0; i < 4; i++)
                        {
                            if(!nearlyPipe.isChecked)
                                nearlyPipe.CheckNearlyPipeAndSetFlowEnabled(i);
                        }
                        isFlowed = true;
                    }
                    break;
                case Direction.Right:
                    if (nearlyPipe.CheckHole(Direction.Left))
                    {
                        nearlyPipe.SetFlowEnabled(true);
                        // 인접한 타일의 타일 흐름 정보 갱신
                        for (int i = 0; i < 4; i++)
                        {
                            if (!nearlyPipe.isChecked)
                                nearlyPipe.CheckNearlyPipeAndSetFlowEnabled(i);
                        }
                        isFlowed = true;
                    }
                    break;
                case Direction.Down:
                    if (nearlyPipe.CheckHole(Direction.Up))
                    {
                        nearlyPipe.SetFlowEnabled(true);
                        // 인접한 타일의 타일 흐름 정보 갱신
                        for (int i = 0; i < 4; i++)
                        {
                            if (!nearlyPipe.isChecked)
                                nearlyPipe.CheckNearlyPipeAndSetFlowEnabled(i);
                        }
                        isFlowed = true;
                    }
                    break;
                case Direction.Left:
                    if (nearlyPipe.CheckHole(Direction.Right))
                    {
                        nearlyPipe.SetFlowEnabled(true);
                        // 인접한 타일의 타일 흐름 정보 갱신
                        for (int i = 0; i < 4; i++)
                        {
                            if (!nearlyPipe.isChecked)
                                nearlyPipe.CheckNearlyPipeAndSetFlowEnabled(i);
                        }
                        isFlowed = true;
                    }
                    break;
            }
        }

        return isFlowed;
    }

    private void SetFlowEnabled(bool canFlow)
    {
        Debug.Log($"{gameObject} flow enables to {canFlow}");
        _pipeCollider.isTrigger = canFlow;
    }

    private bool CheckHole(Direction direction)
    {
        for (int i = 0; i < holeDirections.Count; i++)
        {
            if (direction == holeDirections[i])
                return true;
        }

        return false;
    }
}
