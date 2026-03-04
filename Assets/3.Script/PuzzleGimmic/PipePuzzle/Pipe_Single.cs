using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe_Single : MonoBehaviour
{
    private const int MAX_ROTATE_INDEX = 3;

    // NetworkVariable 대신 일반 int 변수를 사용합니다.
    private int rotateDir = 0;
    public int RotateDir => rotateDir;

    [SerializeField]
    private LayerMask pipeLayer;

    [SerializeField]
    private List<Direction> holeDirections = new List<Direction>();

    private List<Direction> defaultDirections = new List<Direction>();

    [SerializeField]
    public bool IsStartTile { get; set; } = false;
    [SerializeField]
    public bool IsEndTile { get; set; } = false;

    private SpriteRenderer spriteRenderer;
    private Sprite originImg;
    [SerializeField]
    private Sprite bucketImg;

    // 아래 collider의 isTrigger여부로 물길이 열려있는지 판단, true : 열림, false : 닫힘
    private Collider2D _pipeCollider;

    private bool isChecked = false;

    private Pipe_Single[] allPipes;

    [SerializeField]
    private float rotDuration = 1f;
    private static bool s_isRotating = false;

    private void Awake()
    {
        _pipeCollider = GetComponent<Collider2D>();
        spriteRenderer = transform.Find("Pipe").GetComponent<SpriteRenderer>();
        originImg = spriteRenderer.sprite;

        for (int i = 0; i < holeDirections.Count; i++)
        {
            defaultDirections.Add(holeDirections[i]);
        }
    }

    private void Start()
    {
        allPipes = FindObjectsByType<Pipe_Single>(FindObjectsSortMode.None);

        // 싱글플레이에서는 값이 변할 때 이벤트를 구독할 필요 없이, 회전 메서드에서 직접 호출합니다.
    }

    private void OnEnable()
    {
        if (IsStartTile)
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
            rotateDir = 0;

            for(int i = 0; i < defaultDirections.Count; i++)
            {
                holeDirections[i] = defaultDirections[i];
            }

            transform.localEulerAngles = new Vector3(0, 0, 0);
            s_isRotating = false;
        }

        if(IsEndTile)
        {
            SetFlowEnabled(true);
            spriteRenderer.sprite = bucketImg;
        }
        else
        {
            spriteRenderer.sprite = originImg;
        }
    }

    private void OnDisable()
    {
        IsEndTile = false;
        IsStartTile = false;
    }

    /// <summary>
    /// 파이프를 시계방향으로 90도 회전시킴
    /// </summary>
    // ServerRpc 속성을 제거하고 일반 메서드로 변경했습니다.
    public void RotateCW()
    {
        if (s_isRotating)
            return;

        int oldValue = rotateDir;

        rotateDir++;
        if (rotateDir > MAX_ROTATE_INDEX)
            rotateDir = 0;

        // 값이 변경되었으므로 기존 이벤트 콜백 함수를 직접 호출해줍니다.
        OnRotateDirChanged(oldValue, rotateDir);
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
        while (timer < rotDuration)
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
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Water" &&
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
        if (_pipeCollider == null)
        {
            _pipeCollider = GetComponent<Collider2D>();
        }


        int[] dx = new int[4] { 0, 1, 0, -1 };
        int[] dy = new int[4] { 1, 0, -1, 0 };
        isChecked = true;

        bool isFlowed = false;
        Vector2 dirVector = new Vector2(dx[dir], dy[dir]);
        float length = _pipeCollider.bounds.extents.x * 1.2f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirVector, length, pipeLayer);
        Debug.DrawRay(transform.position, dirVector * length, Color.red, 1f);
        Debug.Log($"{gameObject} -{dir}-> {hit.collider}");

        if (hit.collider != null)
        {
            Pipe_Single nearlyPipe = hit.collider.GetComponent<Pipe_Single>();

                Direction pipeDir = (Direction)dir;

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
                            Debug.Log(pipeDir);
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
                            Debug.Log(pipeDir);
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
                            Debug.Log(pipeDir);
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
        foreach (var pipe in allPipes)
        {
            pipe.isChecked = state;
        }
    }
}