using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonBehaviour<InputManager>
{
    public Vector2 MousePosition { get; set; }
    public Action OnClickedLeftBtn { get; set; }

    public bool LeftButtonClicked = false;

    private PlayerInput playerInput;

    protected override void Awake()
    {
        base.Awake();

        playerInput = GetComponent<PlayerInput>();
    }

    public void OnMousePositionChangeEvent(InputAction.CallbackContext callback)
    {
        if(callback.performed)
        {
            MousePosition = callback.ReadValue<Vector2>();
        }
    }

    public void OnClickedLeftBtnEvent(InputAction.CallbackContext callback)
    {
        if(callback.phase == InputActionPhase.Started)
        {
            OnClickedLeftBtn?.Invoke();
            LeftButtonClicked = true;
        }
        else if(callback.phase == InputActionPhase.Canceled)
        {
            LeftButtonClicked = false;
        }
    }

    public void OnPointEvent(InputAction.CallbackContext callback)
    {
        if(callback.phase == InputActionPhase.Started)
        {
            MousePosition = callback.ReadValue<Vector2>();
            OnClickedLeftBtn?.Invoke();
        }
    }

    private PlayerInput.ActionEvent GetActionEvent(string actionName)
    {
        for(int i = 0; i < playerInput.actionEvents.Count; i++)
        {
            if(actionName == playerInput.actionEvents[i].actionName)
            {
                return playerInput.actionEvents[i];
            }
        }

        return null;
    }
}
