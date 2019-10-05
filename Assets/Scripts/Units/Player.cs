using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    private void Update()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        Vector2Int direction = default;
        if (Input.GetButtonDown("Up"))
        {
            direction = Vector2Int.up;
        }
        else if (Input.GetButtonDown("Down"))
        {
            direction = Vector2Int.down;
        }
        else if (Input.GetButtonDown("Left"))
        {
            direction = Vector2Int.left;
        }
        else if (Input.GetButtonDown("Right"))
        {
            direction = Vector2Int.right;
        }
        if (direction != Vector2Int.zero)
        {
            SubmitMoveAction(direction);
            GameManager.Instance.StepCurrentLevel();
        }
    }
}
