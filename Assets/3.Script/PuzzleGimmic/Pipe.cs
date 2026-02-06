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
#if UNITY_EDITOR
    private int rotateDir = 0;
    public int RotateDir => rotateDir;
#else
    NetworkVariable<int> rotateDir = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int RotateDir => rotateDir.Value;
#endif

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

    private void Awake()
    {
        _pipeCollider = GetComponent<Collider2D>();
    }
#if UNITY_EDITOR
    private void Start()
    {
        allPipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);

        if (isStartTile)
        {
            SetFlowEnabled(true);

            // 인접한 타일의 타일 흐름 정보 갱신
            for (int i = 0; i < holeDirections.Count; i++)
            {
                CheckNearlyPipeAndSetFlowEnabled((int)holeDirections[i]);
            }
        }
    }
#else
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        allPipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);

        if (isStartTile)
        {
            SetFlowEnabled(true);

            // 인접한 타일의 타일 흐름 정보 갱신
            for (int i = 0; i < holeDirections.Count; i++)
            {
                CheckNearlyPipeAndSetFlowEnabled((int)holeDirections[i]);
            }
        }

        rotateDir.OnValueChanged += OnRotateDirChanged;
    }
#endif


#if UNITY_EDITOR
    public void RotateCW()
    {
        if (s_isRotating)
            return;

        rotateDir++;
        if (rotateDir > MAX_ROTATE_INDEX)
            rotateDir = 0;

    StartCoroutine(Rotate(rotateDir));

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
#else
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
#endif

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
