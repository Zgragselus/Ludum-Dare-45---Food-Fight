using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    private void Update()
    {
        HandleMovementInput();

        if (GameManager.Instance.CurrentLevel.HasPickableItemAt(CurrentPosition))
        {
            Debug.Log("pickup item");
        }
    }

    private void HandleMovementInput()
    {
        Vector2Int direction = default;
        if (Input.GetButton("Up"))
        {
            direction = Vector2Int.up;
        }
        else if (Input.GetButton("Down"))
        {
            direction = Vector2Int.down;
        }
        else if (Input.GetButton("Left"))
        {
            direction = Vector2Int.left;
        }
        else if (Input.GetButton("Right"))
        {
            direction = Vector2Int.right;
        }
        TryMove(direction);
    }
}
