using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RememberPiece : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Sprite backPieceImage;

    private Sprite originImage;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originImage = spriteRenderer.sprite;
    }

    public void ChangeToBackImage()
    {
        spriteRenderer.sprite = backPieceImage;
    }

    private void OnDestroy()
    {
        originImage = null;
        backPieceImage = null;
    }

    public void EndInteract()
    {

    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if(spriteRenderer.sprite == backPieceImage)
        {
            spriteRenderer.sprite = originImage;

            GetComponentInParent<ShufflePiece>().IncreaseOpenPieceCount(this);
        }
    }
}
