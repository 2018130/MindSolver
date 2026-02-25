using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public enum Direction : byte
{
    Up,
    Right,
    Down,
    Left,
}

public class Pipe : NetworkBehaviour
{
    private const int MAX_ROTATE_INDEX = 3;

    NetworkVariable<int> rotateDir = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int RotateDir => rotateDir.Value;

    [SerializeField]
    private LayerMask pipeLayer;

    [SerializeField]
    private List<Direction> holeDirections = new List<Direction>();

    [SerializeField]
    private bool isStartTile = false;

    // 아래 collider의 isTrigger여부로 물길이 열려있는지 판단, true : 열림, false : 닫힘
    private Collider2D _pipeCollider;

    private bool isChecked = false;

    private Pipe[] allPipes;
    [SerializeField]
    private float rotDuration = 1f;
    private static bool s_isRotating = false;

    public static bool s_isConnected = false;
    public static int s_startPipeCount = 0;

    private void Awake()
    {
        _pipeCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        allPipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);


        rotateDir.OnValueChanged += OnRotateDirChanged;
    }

    private void OnEnable()
    {
        if (isStartTile)
        {
            SetFlowEnabled(true);

            // 인접한 타일의 타일 흐름 정보 갱신
            for (int i = 0; i < holeDirections.Count; i++)
            {
                CheckNearlyPipeAndSetFlowEnabled((int)holeDirections[i]);
            }
        }
        else
        {
            rotateDir.Value = 0;
            transform.localEulerAngles = new Vector3(0, 0, 0);
            s_isRotating = false;
        }
    }

    /// <summary>
    /// 파이프를 시계방향으로 90도 회전시킴
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RotateCW_ServerRpc()
    {
        if (s_isRotating)
            return;

        rotateDir.Value++;
        if (rotateDir.Value > MAX_ROTATE_INDEX)
            rotateDir.Value = 0;
    }

    private IEnumerator Rotate(int dir)
    {
        if (dir > MAX_ROTATE_INDEX)
            yield break;

        s_isRotating = true;
        SetAllPipesCheckState(false);

        for (int i = 0; i < holeDirections.Count; i++)
        {
            int nextDir = (byte)holeDirections[i] + 1;

            if (nextDir > 3)
                nextDir = 0;

            holeDirections[i] = (Direction)nextDir;
        }

        float startRotZ = -90 * (dir - 1 == -1 ? 3 : dir - 1);
        float destinationRotZ = -90 * dir;
        destinationRotZ = destinationRotZ == 0 ? -360 : destinationRotZ;
        float timer = 0f;
        while(timer < rotDuration)
        {
            timer += Time.deltaTime;
            yield return null;

            float rotZ = Mathf.Lerp(startRotZ, destinationRotZ, timer / rotDuration);
            transform.localEulerAngles = new Vector3(0, 0, rotZ);
        }
        transform.localEulerAngles = new Vector3(0, 0, destinationRotZ);
        s_isRotating = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(LayerMask.LayerToName(collision.gameObject.layer) == "Water" && 
            s_isRotating)
        {
            WaterSpawner.ReturnToPool(collision.gameObject);
        }
    }

    public void OnRotateDirChanged(int oldValue, int value)
    {
        s_startPipeCount = 0;
        Debug.Log($"Changed {gameObject}'s dir value {oldValue} to {value}");
        StartCoroutine(Rotate(value));

        // 인접한 타일의 타일 흐름 정보 갱신
        bool isAnyPipeFlowed = false;
        for (int i = 0; i < holeDirections.Count; i++)
        {
            if (CheckNearlyPipeAndSetFlowEnabled((int)holeDirections[i]))
            {
                isAnyPipeFlowed = true;
            }
        }

        if (isAnyPipeFlowed)
        {
            SetFlowEnabled(true);
        }
        else
        {
            SetFlowEnabled(false);
        }
    }

    private bool CheckNearlyPipeAndSetFlowEnabled(int dir)
    {
        if (_pipeCollider == null)
        {
            _pipeCollider = GetComponent<Collider2D>();
        }

        //시작 타일이 연결되어 있는지 확인 이 때 연결되어 있지 않다면 보라색 물을 없앰
        if(isStartTile)
        {
            s_startPipeCount++;

            if(s_startPipeCount == 2)
            {
                s_isConnected = true;
            }
            else
            {
                s_isConnected = false;
            }

            Debug.Log($"{gameObject} start pipe count : {s_startPipeCount}");
        }

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

            for(int j = 0; j < holeDirections.Count; j++)
            {
                Direction pipeDir = holeDirections[j];

                switch (pipeDir)
                {
                    case Direction.Up:
                        if (nearlyPipe.CheckHole(Direction.Down))
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
        }

        return isFlowed;
    }

    private void SetFlowEnabled(bool canFlow)
    {
            if (_pipeCollider == null)
            {
                _pipeCollider = GetComponent<Collider2D>();
            }
            Debug.Log($"{gameObject} flow enables to {canFlow}");
            _pipeCollider.isTrigger = canFlow;
    }

    private bool CheckHole(Direction direction)
    {
        for (int i = 0; i < holeDirections.Count; i++)
        {
            if (direction == (Direction)holeDirections[i])
                return true;
        }

        return false;
    }

    private void SetAllPipesCheckState(bool state)
    {
        foreach(var pipe in allPipes)
        {
            pipe.isChecked = state;
        }
    }
}
