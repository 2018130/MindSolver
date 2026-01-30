using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonBehaviour<InputManager>
{
    public Vector2 MousePosition { get; set; }
    public Action OnClickedLeftBtn { get; set; }

    public void OnMousePositionChangeEvent(InputAction.CallbackContext callback)
    {
        if(callback.performed)
        {
            MousePosition = callback.ReadValue<Vector2>();
        }
    }

    public void OnClickedLeftBtnEvent(InputAction.CallbackContext callback)
    {
        if(callback.phase == InputActionPhase.Performed)
        {
            OnClickedLeftBtn?.Invoke();
        }
    }
}
