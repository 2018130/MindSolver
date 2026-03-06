using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour, IInteractable
{
    private Transform puzzleParentTransform;

    private Vector3 initPos;
    private Collider2D[] childColliders;

    public static int s_puzzleCount;

    [SerializeField]
    private List<Sprite> puzzleList = new List<Sprite>();

    [SerializeField]
    private SpriteRenderer imageSpriteRenderer;

    private static int puzzleIdx = -1;

    private static PuzzleController clickedPuzzleOwner;

    private void Awake()
    {
        puzzleParentTransform = transform.parent;
        initPos = transform.position;
    }
    private void Start()
    {
        childColliders = transform.GetComponentsInChildren<Collider2D>();
        if (s_puzzleCount == 0)
        {
            s_puzzleCount = FindObjectsByType<PuzzleController>(FindObjectsSortMode.None).Length;
            Debug.Log("puzzleCount : " + s_puzzleCount);
        }
    }

    private void OnEnable()
    {
        if (puzzleIdx == -1)
            puzzleIdx = UnityEngine.Random.Range(0, puzzleList.Count);
        imageSpriteRenderer.sprite = puzzleList[puzzleIdx];

        transform.SetParent(puzzleParentTransform);
        transform.position = initPos;
        SetPuzzleCollider(true);
    }

    private void OnDisable()
    {
        puzzleIdx = -1;
    }

    private void SetPuzzleCollider(bool active)
    {
        foreach (var childCol in childColliders)
        {
            if (childCol.TryGetComponent(out PuzzleCollider puzzleCollider))
            {
                childCol.enabled = active;
            }
        }
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if(clickedPuzzleOwner == null ||
            clickedPuzzleOwner == this)
        {
            clickedPuzzleOwner = this;
            SetPuzzleCollider(false);

            Transform parent = transform.parent;
            Transform curPiece = transform;

            while(parent.name != "Puzzle")
            {
                curPiece = parent;
                parent = parent.parent;
            }

            curPiece.position = worldPosFromMousePosition;
        }
    }

    public void EndInteract()
    {
        SetPuzzleCollider(true);

        clickedPuzzleOwner = null;
    }
}
