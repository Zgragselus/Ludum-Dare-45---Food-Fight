using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, ILevelObject
{
    public int Health;

    public int Armor;

    public int Damage;

    public int Experience;

    public Vector2Int CurrentPosition;

    public Vector2Int CurrentDirection;

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
            transform.position = new Vector3(newPosition.x, 0, newPosition.y);
            return true;
        }

        return false;
    }

    public virtual void Tick()
    {
    }
}
