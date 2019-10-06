using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
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

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        transform.position = GameManager.Instance.CurrentLevel.WorldParent.position + new Vector3(CurrentPosition.x, 0, CurrentPosition.y);
        if (CurrentDirection == Vector2Int.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (CurrentDirection == Vector2Int.down)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (CurrentDirection == Vector2Int.left)
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else if (CurrentDirection == Vector2Int.right)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    internal void TransferLevel(int levelIdx)
    {
        GameManager.Instance.MovePlayerToLevel(levelIdx, GameManager.Instance.CurrentLevel.Index < levelIdx);

        UpdateVisuals();
    }

    internal void Pickup(PickupObject obj)
    {
        GameObject.Destroy(obj.gameObject, 0.5f);
    }

    internal void Attack()
    {
    }

    internal void Die()
    {
    }
}
