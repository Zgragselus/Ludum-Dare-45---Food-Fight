using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public int Health;

    public int Damage;

    public Vector2Int CurrentPosition;

    protected bool TryMove(Vector2Int direction)
    {
        var newPosition = CurrentPosition + direction;

        if (GameManager.Instance.CurrentLevel.IsOccupiedByUnit(newPosition))
        {
            Debug.Log("damage");
        }

        if (GameManager.Instance.CurrentLevel.Move(CurrentPosition, newPosition))
        {
            CurrentPosition = newPosition;
            return true;
        }

        return false;
    }
}
