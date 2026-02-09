using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchObjectMover : MonoBehaviour
{
    private HashSet<IInteractable> interactableStartQueue = new HashSet<IInteractable>();

    private void Update()
    {
        if(InputManager.Singleton.LeftButtonClicked)
        {
            CallInteract();
        }
        else
        {
            EndInteract();
        }
    }

    private void CallInteract()
    {
        Vector3 worldPosWithMouse = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        worldPosWithMouse.z = 0;
        Collider2D[] pieces = Physics2D.OverlapPointAll(worldPosWithMouse);

        foreach(var piece in pieces)
        {
            if(piece.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(worldPosWithMouse);

                if(!interactableStartQueue.Contains(interactable))
                {
                    interactableStartQueue.Add(interactable);
                }
            }
        }
    }

    private void EndInteract()
    {
        if (interactableStartQueue.Count <= 0)
            return;

        foreach(var interactable in interactableStartQueue)
        {
            interactable.EndInteract();
        }

        interactableStartQueue.Clear();
    }
}
