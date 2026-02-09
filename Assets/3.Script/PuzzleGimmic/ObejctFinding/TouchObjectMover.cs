using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchObjectMover : MonoBehaviour
{
    private void Update()
    {
        if(InputManager.Singleton.LeftButtonClicked)
        {
            MoveTo();
        }
    }

    private void MoveTo()
    {
        Vector3 worldPosWithMouse = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        worldPosWithMouse.z = 0;
        Collider2D[] pieces = Physics2D.OverlapPointAll(worldPosWithMouse);

        foreach(var piece in pieces)
        {
            if(piece.TryGetComponent<ObjectRigidbody>(out ObjectRigidbody objectRigidbody))
            {
                objectRigidbody.AddForce(worldPosWithMouse);
            }

        }
    }
}
