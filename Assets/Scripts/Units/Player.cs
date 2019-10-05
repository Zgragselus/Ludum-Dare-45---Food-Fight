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
        if (Input.GetButtonDown("Up"))
        {
            Debug.Log("up");
            direction = Vector2Int.up;
        }
        else if (Input.GetButtonDown("Down"))
        {
            Debug.Log("down");
            direction = Vector2Int.down;
        }
        else if (Input.GetButtonDown("Left"))
        {
            Debug.Log("left");
            direction = Vector2Int.left;
        }
        else if (Input.GetButtonDown("Right"))
        {
            Debug.Log("right");
            direction = Vector2Int.right;
        }
        if (direction != Vector2Int.zero && TryMove(direction))
        {
            GameManager.Instance.StepCurrentLevel();
        }
    }

    public void UpdatePosition()
    {
        transform.position = new Vector3(CurrentPosition.x, 0, CurrentPosition.y);
    }
}
