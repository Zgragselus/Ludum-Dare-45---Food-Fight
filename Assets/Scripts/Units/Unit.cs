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

    public bool IsDead => Health == 0;

    protected bool TryMove(Vector2Int direction)
    {
        var newPosition = CurrentPosition + direction;


        if (GameManager.Instance.CurrentLevel.IsOccupiedByUnit(newPosition, out Unit otherUnit))
        {
            Debug.Log($"{gameObject.name} ({CurrentPosition}) attacking {otherUnit.gameObject.name} ({otherUnit.CurrentPosition})");
            otherUnit.TakeDamage(Damage);
        }
        else if (GameManager.Instance.CurrentLevel.Move(CurrentPosition, newPosition))
        {
            Debug.Log($"{gameObject.name} moving to {newPosition}");
            CurrentPosition = newPosition;
            transform.position = new Vector3(newPosition.x, 0, newPosition.y);
            return true;
        }

        return false;
    }

    private void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        Health -= damage;
        Health = Math.Max(Health, 0);
    }

    public virtual void Tick()
    {
    }

    public virtual void PostTick()
    {
        if (IsDead)
        {
            GameManager.Instance.CurrentLevel.Kill(this);
        }
    }
}
