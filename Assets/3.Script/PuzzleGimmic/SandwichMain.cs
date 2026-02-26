using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SandwichMain : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private SliderController sliderController;

    [SerializeField]
    private SandwichPiece sandwichPieces;

    private Vector3 bottomLeftWorldPos;
    private Vector3 topRightWorldPos;

    [SerializeField]
    private float outBoundDistanceOfCenter = 1f;

    [SerializeField]
    private float touchDelay = 1f;
    private DateTime lastTouchTime = DateTime.MinValue;

    [SerializeField]
    private float sandwichValue;
    [SerializeField]
    private float addAmount = 0.1f;
    [SerializeField]
    private float sandwichValueSyncWithUISpeed = 1f;
    private NetworkVariable<float> sliderValue = new NetworkVariable<float>(-1);
    [SerializeField]
    private Vector2 successRange;
    [SerializeField]
    private GameObject bg;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Canvas canvas;

    private bool isPlayingGame = false;
    public bool IsPlayingGame => isPlayingGame;
    private NetworkVariable<float> timer = new NetworkVariable<float>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle += StartGame;
        GetComponentInParent<PuzzleMissonListener>().OnEndPuzzle += EndGame;
    }

    private void StartGame(MultiMissionType multiMissionType)
    {
        if (multiMissionType == MultiMissionType.Sandwich)
        {
            isPlayingGame = true;
            spriteRenderer.enabled = true;
            bg.SetActive(true);
            canvas.gameObject.SetActive(true);
            Camera cam = Camera.main;
            timer.Value = 60;

            Vector3 bottomLeftScreen = new Vector3(0, 0);
            bottomLeftWorldPos = cam.ScreenToWorldPoint(bottomLeftScreen);

            Vector3 topRightScreen = new Vector3(Screen.width, Screen.height);
            topRightWorldPos = cam.ScreenToWorldPoint(topRightScreen);
            sandwichValue = 0.5f;
            sliderController.SetValue(sandwichValue);

            if (IsServer)
            {
                sliderValue.Value = sliderController.GetValue();
            }
        }
    }

    private void EndGame(bool isClear, MultiMissionType multiMissionType)
    {
        if (multiMissionType == MultiMissionType.Sandwich)
        {
            isPlayingGame = false;
            spriteRenderer.enabled = false;
            bg.SetActive(false);
            canvas.gameObject.SetActive(false);
            sliderValue.Value = -1;
        }
    }

    private void Update()
    {
        if (sliderValue.Value == -1)
        {
            isPlayingGame = false;
            spriteRenderer.enabled = false;
            bg.SetActive(false);

            return;
        }

        if (IsServer)
        {
            float sliderValue = sliderController.GetValue();
            if (!Mathf.Approximately(sliderValue, sandwichValue))
            {
                float dir = sandwichValue - sliderValue;

                sliderController.SetValue(sliderValue + dir * Time.deltaTime * sandwichValueSyncWithUISpeed);
                this.sliderValue.Value = sliderController.GetValue();
            }

            if (isPlayingGame)
            {
                timer.Value -= Time.deltaTime;
                int seconds = Mathf.FloorToInt(timer.Value);
                GameUIManager.Singleton.SetclearText(string.Format(seconds.ToString("D2")));

                if (this.sliderValue.Value < successRange.x ||
                    this.sliderValue.Value > successRange.y)
                {
                    Debug.Log($"게임 끝 {successRange.x} < {this.sliderValue.Value} < {successRange.y}");
                    isPlayingGame = false;
                    GetComponentInParent<PuzzleMissonListener>().EndPuzzle(false);
                }
            }

        }
        else
        {
            isPlayingGame = true;
            spriteRenderer.enabled = true;
            bg.SetActive(true);
        }

    }

    private void SpawnSandwich()
    {
        SandwichPiece spawnedPiece = Instantiate(sandwichPieces);

        Vector3 spawnPos = Vector3.zero;
        spawnPos.x = UnityEngine.Random.Range(bottomLeftWorldPos.x, topRightWorldPos.x);
        // x축이 가운데 겹친 경우
        if (spawnPos.x >= -outBoundDistanceOfCenter &&
            spawnPos.x <= outBoundDistanceOfCenter)
        {
            float posY1 = UnityEngine.Random.Range(bottomLeftWorldPos.y,
                -MathF.Sqrt(outBoundDistanceOfCenter * outBoundDistanceOfCenter - spawnPos.x * spawnPos.x));
            float posY2 = UnityEngine.Random.Range(MathF.Sqrt(outBoundDistanceOfCenter * outBoundDistanceOfCenter - spawnPos.x * spawnPos.x),
                topRightWorldPos.y);

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                spawnPos.y = posY1;
            }
            else
            {
                spawnPos.y = posY2;
            }
        }
        else
        {
            spawnPos.y = UnityEngine.Random.Range(bottomLeftWorldPos.y, topRightWorldPos.y);
        }
        spawnedPiece.transform.position = spawnPos;

        spawnedPiece.GetComponent<SandwichPiece>().Initialize(this);
        spawnedPiece.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkPlayer.ClientPlayerId);
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if (DateTime.Now < lastTouchTime.AddSeconds(touchDelay) || !IsOwner || !isPlayingGame)
            return;

        SpawnSandwich();
        sandwichValue += addAmount;
        sliderController.SetValue(sandwichValue);
        this.sliderValue.Value = sliderController.GetValue();
        lastTouchTime = DateTime.Now;
    }

    public void EndInteract()
    {

    }

    public void ReduceValue(float reduceAmount, bool setUIImmadiately = true)
    {
        sandwichValue -= reduceAmount;
        if (setUIImmadiately)
        {
            sliderController.SetValue(sandwichValue);
            this.sliderValue.Value = sliderController.GetValue();
        }
    }
}
