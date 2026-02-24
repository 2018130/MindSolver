using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SandwichPiece : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private float reduceAmount = 0.05f;

    private SandwichMain owner;

    private bool isTouched = false;

    public void Initialize(SandwichMain sandwichMain)
    {
        owner = sandwichMain;
    }

    private void Update()
    {
        if(owner != null && !owner.IsPlayingGame)
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc]
    private void TouchSandwichPiece_ServerRpc()
    {
        Debug.Log($"Touch sandwich piece!!");
        owner.ReduceValue(reduceAmount, false);
        Destroy(gameObject);
    }

    public void EndInteract()
    {
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if (!IsOwner)
            return;

        if(!isTouched)
        {
            TouchSandwichPiece_ServerRpc();
            isTouched = true;
        }
    }
}
