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

    protected void SubmitMoveAction(Vector2Int direction)
    {
        var newPosition = CurrentPosition + direction;

        GameManager.Instance.CurrentLevel.RegisterAction(this, CurrentPosition, newPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>Returns true if the player died as a result</returns>
    public bool TakeDamage(int damage)
    {
        if (IsDead)
        {
            return false;
        }

        Health -= damage;
        Health = Math.Max(Health, 0);

        return IsDead;
    }

    public virtual void SubmitActions()
    {
    }

    public void PostTick()
    {
        if (IsDead)
        {
            GameManager.Instance.CurrentLevel.Kill(this);
        }
    }

    internal void Wait()
    {
    }

    internal void Move(Vector2Int newPos)
    {
        CurrentPosition = newPos;
        transform.position = new Vector3(newPos.x, 0, newPos.y);
    }

    internal void Pickup()
    {
    }

    internal void Attack()
    {
    }

    internal void Die()
    {
    }
}
