using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Interact(Vector2 worldPosFromMousePosition);

    public void EndInteract();
}
