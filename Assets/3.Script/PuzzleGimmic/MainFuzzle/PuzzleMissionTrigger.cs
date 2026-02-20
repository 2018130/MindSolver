using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PuzzleMissionTrigger : MonoBehaviour, IInteractable
{
    [SerializeField]
    private bool isInteracted = false;

    [SerializeField]
    private PuzzleMissonListener listenedGame;

    public void EndInteract()
    {

    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if (!isInteracted)
        {
            isInteracted = true;

            listenedGame.StartPuzzle(this);
        }
    }

    public void ClearPuzzle()
    {
        isInteracted = false;
        Debug.Log($"Clear puzzle listened {listenedGame.gameObject}");
        //Destroy(gameObject);
    }
}
